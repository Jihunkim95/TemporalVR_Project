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
    [RequireComponent(typeof(TVRFeedback))]
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

        [Header("Visual Settings")]
        [SerializeField] private Gradient timeGradient;
        // 기존 Header들 다음에 추가
        [Header("Feedback System")]
        private TVRFeedback feedback;

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

            feedback = GetComponent<TVRFeedback>();
            if (feedback == null)
            {
                Debug.LogWarning("[TemporalVR] TVRFeedback not found, adding it now...");
                feedback = gameObject.AddComponent<TVRFeedback>();
                Debug.Log("[TemporalVR] TVRFeedback component added!");
            }

            // TemporalEventSystem 초기화 확인
            if (TemporalEventSystem.Instance == null)
            {
                GameObject eventSystemObj = new GameObject("TemporalEventSystem");
                eventSystemObj.AddComponent<TemporalEventSystem>();
            }

            // 기본 timeGradient 설정
            if (timeGradient == null)
            {
                timeGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[]
                {
            new GradientColorKey(Color.blue, 0f),
            new GradientColorKey(Color.green, 0.5f),
            new GradientColorKey(Color.red, 1f)
                };
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
                {
            new GradientAlphaKey(1f, 0f),
            new GradientAlphaKey(1f, 1f)
                };
                timeGradient.SetKeys(colorKeys, alphaKeys);
            }
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
                lineObj.transform.SetParent(transform);
                timelineVisualizer = lineObj.AddComponent<LineRenderer>();
            }

            timelineVisualizer.startWidth = 0.02f;
            timelineVisualizer.endWidth = 0.02f;

            // Unlit Shader 사용 (색상이 더 잘 보임)
            Material mat = new Material(Shader.Find("Sprites/Default"));
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }
            timelineVisualizer.material = mat;

            // Use World Space
            timelineVisualizer.useWorldSpace = true;

            // 초기 색상 설정
            Color initialColor = GetModeColor(currentMode);
            timelineVisualizer.startColor = initialColor;
            timelineVisualizer.endColor = initialColor;

            // 위치 설정
            Vector3[] positions = new Vector3[2];
            positions[0] = transform.position - Vector3.right * (timelineLength / 2);
            positions[1] = transform.position + Vector3.right * (timelineLength / 2);
            timelineVisualizer.SetPositions(positions);

            Debug.Log($"[TemporalVR] Timeline setup complete with color: {initialColor}");
        }

        // GetModeColor 메서드 추가
        private Color GetModeColor(TemporalMode mode)
        {
            switch (mode)
            {
                case TemporalMode.Scrub: return new Color(0.2f, 0.4f, 1f);
                case TemporalMode.Paint: return new Color(0.2f, 1f, 0.4f);
                case TemporalMode.Sculpt: return new Color(1f, 0.4f, 0.2f);
                case TemporalMode.Preview: return new Color(1f, 1f, 0.2f);
                default: return Color.white;
            }
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

        // 2. HandleModeSwitch 수정 - EventSystem 활용
        private void HandleModeSwitch()
        {
            if (rightActionController == null) return;

            bool primaryButtonPressed = false;
            if (rightActionController.selectAction != null && rightActionController.selectAction.action != null)
            {
                primaryButtonPressed = rightActionController.selectAction.action.ReadValue<float>() > 0.5f;
            }

            if (primaryButtonPressed && !wasPrimaryButtonPressed)
            {
                currentMode = (TemporalMode)(((int)currentMode + 1) % 4);
                Debug.Log($"[TemporalVR] Mode switched to: {currentMode}");

                // EventSystem으로 브로드캐스트
                TemporalEventSystem.Instance?.BroadcastModeChange(currentMode);

                if (feedback != null)
                {
                    feedback.UpdateModeColor(currentMode);
                    Debug.Log($"[TemporalVR] UpdateModeColor called with {currentMode}");
                }
                else
                {
                    Debug.LogError("[TemporalVR] TVRFeedback is null!");
                }

                // Timeline 색상 변경
                if (timelineVisualizer != null)
                {
                    Color newColor = GetModeColor(currentMode);
                    timelineVisualizer.startColor = newColor * 0.7f;
                    timelineVisualizer.endColor = newColor;
                    Debug.Log($"[TemporalVR] Direct timeline color update: {newColor}");
                }
            }
            wasPrimaryButtonPressed = primaryButtonPressed;
        }


        private void HandleTimeScrubbing()
        {
            if (rightActionController == null) return;

            float triggerValue = 0f;
            if (rightActionController.activateAction != null && rightActionController.activateAction.action != null)
            {
                triggerValue = rightActionController.activateAction.action.ReadValue<float>();
            }

            if (triggerValue > 0.5f && !isGrabbingTimeline)  // 임계값 낮춤
            {
                isGrabbingTimeline = true;
                grabStartPosition = rightController.position;
                grabStartTime = currentTimePosition;
                Debug.Log($"[Scrub] Started grabbing - StartPos: {grabStartPosition}, StartTime: {grabStartTime}");
            }
            else if (triggerValue < 0.2f && isGrabbingTimeline)
            {
                isGrabbingTimeline = false;
                Debug.Log($"[Scrub] Stopped grabbing - Final time: {currentTimePosition}");
            }

            if (isGrabbingTimeline)
            {
                Vector3 currentPos = rightController.position;
                float deltaX = (currentPos.x - grabStartPosition.x) * scrubSensitivity;

                // 디버깅 정보
                Debug.Log($"[Scrub] CurrentPos: {currentPos}, StartPos: {grabStartPosition}");
                Debug.Log($"[Scrub] DeltaX: {deltaX}, Sensitivity: {scrubSensitivity}");

                currentTimePosition = Mathf.Clamp(grabStartTime + (deltaX / timelineLength) * temporalRange, 0f, temporalRange);

                Debug.Log($"[Scrub] Calculated Time: {currentTimePosition}");

                OnTimeChanged(currentTimePosition);
                // 모든 TMorphObject 업데이트
                TMorphObj[] allMorphObjs = FindObjectsOfType<TMorphObj>();
                foreach (var morphObj in allMorphObjs)
                {
                    morphObj.UpdateToTime(currentTimePosition);
                }

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
                float brushRadius = 0.05f + (triggerValue * 0.08f);
                float brushStrength = triggerValue;
                float targetTime = Mathf.Clamp01((rightController.position.y - 0.5f) / 2f);

                // Ray를 사용해서 정확한 hit point 찾기
                RaycastHit hit;
                bool hasHit = Physics.Raycast(
                    rightController.position,
                    rightController.forward,
                    out hit,
                    2f  // 최대 거리 2미터
                );

                if (hasHit)
                {
                    // Hit point를 브러시 위치로 사용
                    Vector3 brushPosition = hit.point;

                    // 충돌한 오브젝트에서 TMorphObj_V2 찾기
                    TMorphObj_V2 morphV2 = hit.collider.GetComponent<TMorphObj_V2>();
                    if (morphV2 != null)
                    {
                        // Hit point에 브러시 적용
                        morphV2.ApplyTemporalBrush(brushPosition, brushRadius, brushStrength, targetTime);

                        // 피드백 효과도 hit point에 표시
                        if (feedback != null)
                        {
                            feedback.ShowBrushEffect(brushPosition, brushRadius, targetTime);
                        }
                    }

                    // 기존 TMorphTest 지원
                    TMorphTest morphTest = hit.collider.GetComponent<TMorphTest>();
                    if (morphTest != null)
                    {
                        float minTime, maxTime;
                        morphTest.GetTimeRange(out minTime, out maxTime);
                        float actualTime = Mathf.Lerp(minTime, maxTime, targetTime);
                        morphTest.LerpToTime(actualTime, brushStrength * Time.deltaTime * 5f);
                        morphTest.OnTemporalBrushApplied(brushStrength, brushPosition);
                    }

                    // 브러시 트레일 업데이트 (hit point 기준)
                    if (feedback != null)
                    {
                        feedback.UpdateBrushTrail(brushPosition, targetTime);
                    }
                }
                else
                {
                    // Ray가 아무것도 hit하지 않았을 때
                    // 옵션 1: 아무것도 하지 않음
                    // 옵션 2: 컨트롤러 앞 고정 거리에 프리뷰 표시
                    if (feedback != null)
                    {
                        Vector3 previewPosition = rightController.position + rightController.forward * 0.5f;
                        // 투명한 프리뷰 효과 (선택사항)
                        // feedback.ShowBrushPreview(previewPosition, brushRadius * 0.5f, targetTime);
                    }
                }

                // Shift 모드는 별도 처리 (정밀 타겟팅)
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    // 더 긴 거리의 Ray
                    if (Physics.Raycast(rightController.position, rightController.forward, out hit, 5f))
                    {
                        TemporalEventSystem.Instance?.BroadcastObjectSelected(hit.collider.gameObject);

                        TMorphObj_V2 morphV2 = hit.collider.GetComponent<TMorphObj_V2>();
                        if (morphV2 != null)
                        {
                            morphV2.ApplyTemporalBrush(hit.point, brushRadius * 0.5f, brushStrength * 1.5f, targetTime);
                        }
                    }

                    Debug.DrawRay(rightController.position, rightController.forward * 5f, timeGradient.Evaluate(targetTime));
                }
            }
            else
            {
                if (feedback != null)
                {
                    feedback.ClearBrushTrail();
                }
            }

            // 디버그: Ray 시각화 (개발 중에만)
            Debug.DrawRay(rightController.position, rightController.forward * 2f, Color.cyan);
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

            // Time Display 업데이트
            if (timeDisplay != null)
            {
                string modeText = currentMode.ToString();
                string timeText = $"T: {currentTimePosition:F2}s";
                timeDisplay.text = $"{modeText}\n{timeText}";
            }
        }

        private void OnTimeChanged(float newTime)
        {
            // 기존 코드 대체
            TemporalEventSystem.Instance?.BroadcastTimeChange(newTime);
            Debug.Log($"[TemporalVR] Time changed to: {newTime:F1}");
        }

        public float GetCurrentTime() => currentTimePosition;
        public TemporalMode GetCurrentMode() => currentMode;


    }

    // 나머지 클래스는 동일
    public class TemporalObject : MonoBehaviour
    {
        public virtual void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            //Debug.Log($"[TemporalObject] Brush applied at {position} with strength {strength} at time {time}");
        }

        public virtual void SetTemporalRange(float startTime, float endTime)
        {
            //Debug.Log($"[TemporalObject] Time range set: {startTime} - {endTime}");
        }
    }


}