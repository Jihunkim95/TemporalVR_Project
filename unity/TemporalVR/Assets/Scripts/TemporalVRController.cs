using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

namespace TemporalVR
{
    /// <summary>
    /// VR 컨트롤러로 시간을 조작하는 핵심 입력 시스템
    /// </summary>
    public class TemporalVRController : MonoBehaviour
    {
        [Header("XR Controller References")]
        [SerializeField] private XRController leftController;
        [SerializeField] private XRController rightController;

        [Header("Temporal Control Settings")]
        [SerializeField] private float timelineLength = 10f; // 시간축 길이 (미터)
        [SerializeField] private float temporalRange = 100f; // 시간 범위 (프레임)
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

        // 입력 값 캐싱
        private InputDevice leftDevice;
        private InputDevice rightDevice;

        // Temporal Brush 모드
        public enum TemporalMode
        {
            Scrub,      // 시간 스크러빙
            Paint,      // Temporal Brush
            Sculpt,     // 시간 조각
            Preview     // 미리보기
        }

        [SerializeField] private TemporalMode currentMode = TemporalMode.Scrub;

        private void Start()
        {
            InitializeControllers();
            SetupTimelineVisualizer();
        }

        private void InitializeControllers()
        {
            // XR 디바이스 가져오기
            var leftHandDevices = new List<InputDevice>();
            var rightHandDevices = new List<InputDevice>();

            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

            if (leftHandDevices.Count > 0)
                leftDevice = leftHandDevices[0];
            if (rightHandDevices.Count > 0)
                rightDevice = rightHandDevices[0];

            Debug.Log($"[TemporalVR] Controllers initialized - Left: {leftDevice.isValid}, Right: {rightDevice.isValid}");
        }

        private void SetupTimelineVisualizer()
        {
            if (timelineVisualizer == null)
            {
                GameObject lineObj = new GameObject("TimelineVisualizer");
                timelineVisualizer = lineObj.AddComponent<LineRenderer>();
            }

            // 시간축 시각화 설정
            timelineVisualizer.startWidth = 0.02f;
            timelineVisualizer.endWidth = 0.02f;
            timelineVisualizer.material = new Material(Shader.Find("Sprites/Default"));
            timelineVisualizer.startColor = new Color(0.1f, 0.1f, 0.8f, 0.5f); // 과거 = 파란색
            timelineVisualizer.endColor = new Color(0.8f, 0.1f, 0.1f, 0.5f);   // 미래 = 빨간색

            // 시간축 그리기
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
            // A/X 버튼으로 모드 전환
            bool primaryButtonPressed = false;
            if (rightDevice.isValid)
            {
                rightDevice.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            }

            if (primaryButtonPressed)
            {
                currentMode = (TemporalMode)(((int)currentMode + 1) % 4);
                Debug.Log($"[TemporalVR] Mode switched to: {currentMode}");

                // 햅틱 피드백
                SendHapticFeedback(rightDevice, 0.1f, 0.5f);
            }
        }

        private void HandleTimeScrubbing()
        {
            // 오른손 트리거로 시간축 잡기
            float triggerValue = 0f;
            if (rightDevice.isValid)
            {
                rightDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            }

            // 트리거를 누르면 시간축 잡기
            if (triggerValue > 0.8f && !isGrabbingTimeline)
            {
                isGrabbingTimeline = true;
                grabStartPosition = rightController.transform.position;
                grabStartTime = currentTimePosition;

                SendHapticFeedback(rightDevice, 0.2f, 0.8f);
            }
            else if (triggerValue < 0.2f && isGrabbingTimeline)
            {
                isGrabbingTimeline = false;
            }

            // 시간축 스크러빙
            if (isGrabbingTimeline)
            {
                Vector3 currentPos = rightController.transform.position;
                float deltaX = (currentPos.x - grabStartPosition.x) * scrubSensitivity;

                // 시간 위치 계산 (0-100 범위)
                currentTimePosition = Mathf.Clamp(grabStartTime + (deltaX / timelineLength) * temporalRange, 0f, temporalRange);

                // 시간 변경 이벤트 발생
                OnTimeChanged(currentTimePosition);
            }
        }

        private void HandleTemporalPainting()
        {
            // Temporal Brush 모드 - 시간을 "칠하는" 인터랙션
            float triggerValue = 0f;
            if (rightDevice.isValid)
            {
                rightDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
            }

            if (triggerValue > 0.1f)
            {
                // 브러시 강도에 따른 시간 변화
                float brushStrength = triggerValue;
                Vector3 brushPosition = rightController.transform.position;

                // 레이캐스트로 오브젝트 선택
                RaycastHit hit;
                if (Physics.Raycast(brushPosition, rightController.transform.forward, out hit, 5f))
                {
                    TemporalObject tempObj = hit.collider.GetComponent<TemporalObject>();
                    if (tempObj != null)
                    {
                        // 시간 "페인팅" 적용
                        tempObj.ApplyTemporalBrush(hit.point, brushStrength, currentTimePosition);

                        // 약한 연속 햅틱
                        SendHapticFeedback(rightDevice, 0.05f, brushStrength * 0.3f);
                    }
                }
            }
        }

        private void HandleTemporalSculpting()
        {
            // 양손을 사용한 시간 조각 모드
            float leftTrigger = 0f, rightTrigger = 0f;

            if (leftDevice.isValid)
                leftDevice.TryGetFeatureValue(CommonUsages.trigger, out leftTrigger);
            if (rightDevice.isValid)
                rightDevice.TryGetFeatureValue(CommonUsages.trigger, out rightTrigger);

            if (leftTrigger > 0.8f && rightTrigger > 0.8f)
            {
                // 양손 거리로 시간 범위 조절
                float handDistance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                float timeRange = handDistance * 20f; // 거리를 시간 범위로 변환

                // 양손 중점에서 시간 영역 생성
                Vector3 centerPoint = (leftController.transform.position + rightController.transform.position) / 2f;

                // 시간 조각 영역 시각화
                Debug.DrawLine(leftController.transform.position, rightController.transform.position, Color.yellow);

                // 영역 내 오브젝트에 시간 효과 적용
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
            // 조이스틱으로 시간 미리보기
            Vector2 joystickValue = Vector2.zero;
            if (rightDevice.isValid)
            {
                rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue);
            }

            // 조이스틱 X축으로 시간 조절
            if (Mathf.Abs(joystickValue.x) > 0.1f)
            {
                currentTimePosition += joystickValue.x * Time.deltaTime * 10f;
                currentTimePosition = Mathf.Clamp(currentTimePosition, 0f, temporalRange);

                OnTimeChanged(currentTimePosition);
            }
        }

        private void UpdateVisualFeedback()
        {
            // 시간 커서 업데이트
            if (temporalCursor != null)
            {
                float normalizedTime = currentTimePosition / temporalRange;
                Vector3 cursorPos = Vector3.Lerp(
                    transform.position - Vector3.right * (timelineLength / 2),
                    transform.position + Vector3.right * (timelineLength / 2),
                    normalizedTime
                );
                temporalCursor.position = cursorPos;

                // 커서 색상 변화
                var renderer = temporalCursor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.Lerp(
                        new Color(0.1f, 0.1f, 0.8f), // 과거
                        new Color(0.8f, 0.1f, 0.1f), // 미래
                        normalizedTime
                    );
                }
            }

            // 시간 텍스트 업데이트
            if (timeDisplay != null)
            {
                timeDisplay.text = $"Time: {currentTimePosition:F1}\nMode: {currentMode}";
            }
        }

        private void SendHapticFeedback(InputDevice device, float duration, float amplitude)
        {
            if (device.isValid)
            {
                HapticCapabilities capabilities;
                if (device.TryGetHapticCapabilities(out capabilities) && capabilities.supportsImpulse)
                {
                    device.SendHapticImpulse(0, amplitude, duration);
                }
            }
        }

        private void OnTimeChanged(float newTime)
        {
            // 시간 변경 이벤트 - 다른 시스템에 알림
            TemporalEventSystem.Instance?.BroadcastTimeChange(newTime);

            // 디버그 정보
            Debug.Log($"[TemporalVR] Time changed to: {newTime:F1}");
        }

        // 외부에서 현재 시간 가져오기
        public float GetCurrentTime() => currentTimePosition;
        public TemporalMode GetCurrentMode() => currentMode;
    }

    /// <summary>
    /// Temporal 오브젝트 인터페이스 (임시)
    /// </summary>
    public class TemporalObject : MonoBehaviour
    {
        public void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            // Temporal Brush 효과 적용 로직
            Debug.Log($"[TemporalObject] Brush applied at {position} with strength {strength} at time {time}");
        }

        public void SetTemporalRange(float startTime, float endTime)
        {
            // 시간 범위 설정 로직
            Debug.Log($"[TemporalObject] Time range set: {startTime} - {endTime}");
        }
    }

    /// <summary>
    /// Temporal 이벤트 시스템 (싱글톤)
    /// </summary>
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