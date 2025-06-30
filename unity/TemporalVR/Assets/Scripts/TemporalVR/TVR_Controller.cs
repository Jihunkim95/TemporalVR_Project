using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

namespace TemporalVR
{
    /// <summary>
    /// VR 컨트롤러로 시간을 조작하는 핵심 입력 시스템
    /// </summary>
    public class TemporalVRController : MonoBehaviour
    {
        [Header("XR Controller References")]
        [SerializeField] private Transform leftController;
        [SerializeField] private Transform rightController;

        [Header("Temporal Control Settings")]
        [SerializeField] private float timelineLength = 10f;
        [SerializeField] private float temporalRange = 100f;
        [SerializeField] private float scrubSensitivity = 2f;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer timelineVisualizer;
        [SerializeField] private Transform temporalCursor;
        [SerializeField] private TextMesh timeDisplay;

        // 입력 상태
        private bool isGrabbingTimeline = false;
        private float currentTimePosition = 0f;
        private Vector3 grabStartPosition;
        private float grabStartTime;

        // ActionBasedController 참조
        private ActionBasedController leftActionController;
        private ActionBasedController rightActionController;

        // 버튼 상태 추적
        private bool wasPrimaryButtonPressed = false;

        public enum TemporalMode
        {
            Scrub,
            Paint,
            Sculpt,
            Preview
        }

        [SerializeField] private TemporalMode currentMode = TemporalMode.Scrub;

        private void Start()
        {
            StartCoroutine(InitializeControllers());
            SetupTimelineVisualizer();
        }

        private IEnumerator InitializeControllers()
        {
            // XR 시스템 초기화 대기
            yield return new WaitForSeconds(0.5f);

            // ActionBasedController 컴포넌트 가져오기
            if (leftController != null)
                leftActionController = leftController.GetComponent<ActionBasedController>();
            if (rightController != null)
                rightActionController = rightController.GetComponent<ActionBasedController>();

            if (leftActionController != null && rightActionController != null)
            {
                Debug.Log("[TemporalVR] ActionBasedControllers found!");

                // Actions이 할당되었는지 확인
                if (rightActionController.activateAction != null)
                {
                    Debug.Log("[TemporalVR] Activate Action is set");
                }
                if (rightActionController.selectAction != null)
                {
                    Debug.Log("[TemporalVR] Select Action is set");
                }
            }
            else
            {
                Debug.LogWarning("[TemporalVR] ActionBasedController not found on controllers!");
            }
        }

        private void SetupTimelineVisualizer()
        {
            if (timelineVisualizer == null)
            {
                GameObject lineObj = new GameObject("TimelineVisualizer");
                timelineVisualizer = lineObj.AddComponent<LineRenderer>();
            }

            timelineVisualizer.startWidth = 0.02f;
            timelineVisualizer.endWidth = 0.02f;
            timelineVisualizer.material = new Material(Shader.Find("Sprites/Default"));
            timelineVisualizer.startColor = new Color(0.1f, 0.1f, 0.8f, 0.5f);
            timelineVisualizer.endColor = new Color(0.8f, 0.1f, 0.1f, 0.5f);

            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position - Vector3.right * (timelineLength / 2);
            positions[1] = transform.position + Vector3.right * (timelineLength / 2);
            timelineVisualizer.SetPositions(positions);
        }

        private void Update()
        {
            HandleModeSwitch();

            switch (currentMode)
            {
                case TemporalMode.Scrub:
                    HandleTimeScrubbing();
                    break;
                case TemporalMode.Paint:
                    HandleTemporalPainting();
                    break;
                case TemporalMode.Sculpt:
                    HandleTemporalSculpting();
                    break;
                case TemporalMode.Preview:
                    HandlePreviewMode();
                    break;
            }

            UpdateVisualFeedback();
        }

        private void HandleModeSwitch()
        {
            if (rightActionController == null) return;

            // selectAction이 Primary Button (A/X)
            bool primaryButtonPressed = false;
            if (rightActionController.selectAction != null && rightActionController.selectAction.action != null)
            {
                primaryButtonPressed = rightActionController.selectAction.action.ReadValue<float>() > 0.5f;
            }

            if (primaryButtonPressed && !wasPrimaryButtonPressed)
            {
                currentMode = (TemporalMode)(((int)currentMode + 1) % 4);
                Debug.Log($"[TemporalVR] Mode switched to: {currentMode}");
            }

            wasPrimaryButtonPressed = primaryButtonPressed;
        }

        private void HandleTimeScrubbing()
        {
            if (rightActionController == null) return;

            float triggerValue = 0f;

            // activateAction이 Trigger
            if (rightActionController.activateAction != null && rightActionController.activateAction.action != null)
            {
                triggerValue = rightActionController.activateAction.action.ReadValue<float>();
            }

            if (triggerValue > 0.8f && !isGrabbingTimeline)
            {
                isGrabbingTimeline = true;
                grabStartPosition = rightController.position;
                grabStartTime = currentTimePosition;
            }
            else if (triggerValue < 0.2f && isGrabbingTimeline)
            {
                isGrabbingTimeline = false;
            }

            if (isGrabbingTimeline)
            {
                Vector3 currentPos = rightController.position;
                float deltaX = (currentPos.x - grabStartPosition.x) * scrubSensitivity;
                currentTimePosition = Mathf.Clamp(grabStartTime + (deltaX / timelineLength) * temporalRange, 0f, temporalRange);
                OnTimeChanged(currentTimePosition);
            }
        }

        private void HandleTemporalPainting()
        {
            if (rightActionController == null) return;

            float triggerValue = 0f;
            if (rightActionController.activateAction != null && rightActionController.activateAction.action != null)
            {
                triggerValue = rightActionController.activateAction.action.ReadValue<float>();
            }

            if (triggerValue > 0.1f)
            {
                float brushStrength = triggerValue;
                Vector3 brushPosition = rightController.position;

                RaycastHit hit;
                if (Physics.Raycast(brushPosition, rightController.forward, out hit, 5f))
                {
                    TemporalObject tempObj = hit.collider.GetComponent<TemporalObject>();
                    if (tempObj != null)
                    {
                        tempObj.ApplyTemporalBrush(hit.point, brushStrength, currentTimePosition);
                    }
                }
            }
        }

        private void HandleTemporalSculpting()
        {
            if (leftActionController == null || rightActionController == null) return;

            float leftTrigger = 0f;
            float rightTrigger = 0f;

            if (leftActionController.activateAction != null && leftActionController.activateAction.action != null)
                leftTrigger = leftActionController.activateAction.action.ReadValue<float>();

            if (rightActionController.activateAction != null && rightActionController.activateAction.action != null)
                rightTrigger = rightActionController.activateAction.action.ReadValue<float>();

            if (leftTrigger > 0.8f && rightTrigger > 0.8f)
            {
                float handDistance = Vector3.Distance(leftController.position, rightController.position);
                float timeRange = handDistance * 20f;
                Vector3 centerPoint = (leftController.position + rightController.position) / 2f;

                Debug.DrawLine(leftController.position, rightController.position, Color.yellow);

                Collider[] colliders = Physics.OverlapSphere(centerPoint, handDistance / 2f);
                foreach (var col in colliders)
                {
                    TemporalObject tempObj = col.GetComponent<TemporalObject>();
                    if (tempObj != null)
                    {
                        tempObj.SetTemporalRange(currentTimePosition - timeRange / 2, currentTimePosition + timeRange / 2);
                    }
                }
            }
        }

        private void HandlePreviewMode()
        {
            // Preview 모드는 일단 비워둠 (조이스틱 입력은 나중에 추가)
        }

        private void UpdateVisualFeedback()
        {
            if (temporalCursor != null)
            {
                float normalizedTime = currentTimePosition / temporalRange;
                Vector3 cursorPos = Vector3.Lerp(
                    transform.position - Vector3.right * (timelineLength / 2),
                    transform.position + Vector3.right * (timelineLength / 2),
                    normalizedTime
                );
                temporalCursor.position = cursorPos;

                var renderer = temporalCursor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.Lerp(
                        new Color(0.1f, 0.1f, 0.8f),
                        new Color(0.8f, 0.1f, 0.1f),
                        normalizedTime
                    );
                }
            }

            if (timeDisplay != null)
            {
                timeDisplay.text = $"Time: {currentTimePosition:F1}\nMode: {currentMode}";
            }
        }

        private void OnTimeChanged(float newTime)
        {
            TemporalEventSystem.Instance?.BroadcastTimeChange(newTime);
            Debug.Log($"[TemporalVR] Time changed to: {newTime:F1}");
        }

        public float GetCurrentTime() => currentTimePosition;
        public TemporalMode GetCurrentMode() => currentMode;
    }

    // 나머지 클래스는 동일
    public class TemporalObject : MonoBehaviour
    {
        public void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            Debug.Log($"[TemporalObject] Brush applied at {position} with strength {strength} at time {time}");
        }

        public void SetTemporalRange(float startTime, float endTime)
        {
            Debug.Log($"[TemporalObject] Time range set: {startTime} - {endTime}");
        }
    }

    public class TemporalEventSystem : MonoBehaviour
    {
        public static TemporalEventSystem Instance { get; private set; }
        public delegate void TimeChangeHandler(float newTime);
        public event TimeChangeHandler OnTimeChanged;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void BroadcastTimeChange(float newTime)
        {
            OnTimeChanged?.Invoke(newTime);
        }
    }
}