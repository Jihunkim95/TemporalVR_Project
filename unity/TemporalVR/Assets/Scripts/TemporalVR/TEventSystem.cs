using UnityEngine;
using System;
using System.Collections.Generic;

namespace TemporalVR
{
    /// <summary>
    /// Temporal VR의 중앙 이벤트 시스템
    /// 모든 시간 관련 이벤트를 관리하고 브로드캐스트
    /// </summary>
    public class TemporalEventSystem : MonoBehaviour
    {
        // Singleton
        public static TemporalEventSystem Instance { get; private set; }

        // Events
        public delegate void TimeChangeHandler(float newTime);
        public event TimeChangeHandler OnTimeChanged;

        public delegate void ModeChangeHandler(TemporalVRController.TemporalMode newMode);
        public event ModeChangeHandler OnModeChanged;

        public delegate void BrushAppliedHandler(Vector3 position, float strength, float time);
        public event BrushAppliedHandler OnBrushApplied;

        public delegate void ObjectSelectedHandler(GameObject obj);
        public event ObjectSelectedHandler OnObjectSelected;

        // 현재 상태
        private float currentGlobalTime = 0f;
        private TemporalVRController.TemporalMode currentMode = TemporalVRController.TemporalMode.Scrub;
        private List<TemporalObject> registeredObjects = new List<TemporalObject>();

        // 디버그
        [Header("Debug")]
        [SerializeField] private bool logEvents = true;
        [SerializeField] private bool showDebugUI = false;

        void Awake()
        {
            // Singleton 패턴
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeEventSystem();
        }

        void InitializeEventSystem()
        {
            Debug.Log("[TemporalEventSystem] Initialized");

            // 기본 이벤트 리스너 등록
            OnTimeChanged += HandleGlobalTimeChange;
            OnModeChanged += HandleModeChange;
        }

        /// <summary>
        /// 시간 변경 브로드캐스트
        /// </summary>
        public void BroadcastTimeChange(float newTime)
        {
            currentGlobalTime = newTime;
            OnTimeChanged?.Invoke(newTime);

            if (logEvents)
            {
                Debug.Log($"[TemporalEventSystem] Time changed to: {newTime:F2}");
            }
        }

        /// <summary>
        /// 모드 변경 브로드캐스트
        /// </summary>
        public void BroadcastModeChange(TemporalVRController.TemporalMode newMode)
        {
            currentMode = newMode;
            OnModeChanged?.Invoke(newMode);

            if (logEvents)
            {
                Debug.Log($"[TemporalEventSystem] Mode changed to: {newMode}");
            }
        }

        /// <summary>
        /// 브러시 적용 브로드캐스트
        /// </summary>
        public void BroadcastBrushApplied(Vector3 position, float strength, float time)
        {
            OnBrushApplied?.Invoke(position, strength, time);

            if (logEvents && Time.frameCount % 30 == 0) // 매 30프레임마다만 로그
            {
                Debug.Log($"[TemporalEventSystem] Brush applied at {position} with strength {strength:F2}");
            }
        }

        /// <summary>
        /// 객체 선택 브로드캐스트
        /// </summary>
        public void BroadcastObjectSelected(GameObject obj)
        {
            OnObjectSelected?.Invoke(obj);

            if (logEvents)
            {
                Debug.Log($"[TemporalEventSystem] Object selected: {obj.name}");
            }
        }

        /// <summary>
        /// TemporalObject 등록
        /// </summary>
        public void RegisterTemporalObject(TemporalObject obj)
        {
            if (!registeredObjects.Contains(obj))
            {
                registeredObjects.Add(obj);
                Debug.Log($"[TemporalEventSystem] Registered: {obj.name}");
            }
        }

        /// <summary>
        /// TemporalObject 등록 해제
        /// </summary>
        public void UnregisterTemporalObject(TemporalObject obj)
        {
            if (registeredObjects.Contains(obj))
            {
                registeredObjects.Remove(obj);
                Debug.Log($"[TemporalEventSystem] Unregistered: {obj.name}");
            }
        }

        /// <summary>
        /// 모든 등록된 객체에 시간 적용
        /// </summary>
        public void ApplyTimeToAllObjects(float time)
        {
            foreach (var obj in registeredObjects)
            {
                if (obj != null)
                {
                    obj.ApplyTemporalBrush(obj.transform.position, 1f, time);
                }
            }
        }

        // 이벤트 핸들러
        private void HandleGlobalTimeChange(float newTime)
        {
            // 전역 시간 변경시 추가 처리
            // 예: UI 업데이트, 통계 수집 등
        }

        private void HandleModeChange(TemporalVRController.TemporalMode newMode)
        {
            // 모드 변경시 추가 처리
            // 예: UI 색상 변경, 도구 전환 등
        }

        // Getter
        public float GetCurrentGlobalTime() => currentGlobalTime;
        public TemporalVRController.TemporalMode GetCurrentMode() => currentMode;
        public List<TemporalObject> GetRegisteredObjects() => new List<TemporalObject>(registeredObjects);

        // 디버그 UI
        void OnGUI()
        {
            if (!showDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Box("Temporal Event System");

            GUILayout.Label($"Global Time: {currentGlobalTime:F2}");
            GUILayout.Label($"Mode: {currentMode}");
            GUILayout.Label($"Registered Objects: {registeredObjects.Count}");

            if (GUILayout.Button("Reset Time"))
            {
                BroadcastTimeChange(0f);
            }

            if (GUILayout.Button("Apply Time to All"))
            {
                ApplyTimeToAllObjects(currentGlobalTime);
            }

            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            // 이벤트 정리
            OnTimeChanged = null;
            OnModeChanged = null;
            OnBrushApplied = null;
            OnObjectSelected = null;
        }
    }
}