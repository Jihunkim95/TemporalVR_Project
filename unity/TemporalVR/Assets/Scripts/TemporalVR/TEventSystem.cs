using UnityEngine;
using UnityEngine.Events;
using System;

namespace TemporalVR
{
    /// <summary>
    /// Temporal VR 시스템의 전역 이벤트 관리
    /// </summary>
    public class TemporalEventSystem : MonoBehaviour
    {
        // 싱글톤 인스턴스
        private static TemporalEventSystem instance;
        public static TemporalEventSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TemporalEventSystem>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("TemporalEventSystem");
                        instance = go.AddComponent<TemporalEventSystem>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        // 이벤트 정의
        public event Action<float> OnTimeChanged;
        public event Action<TemporalVRController.TemporalMode> OnModeChanged;
        public event Action<GameObject> OnObjectSelected;
        public event Action<bool> OnPlaybackToggled; // 추가된 이벤트

        // Unity Events (Inspector에서 설정 가능)
        [Header("Unity Events")]
        public UnityEvent<float> TimeChangedEvent;
        public UnityEvent<int> ModeChangedEvent;
        public UnityEvent<GameObject> ObjectSelectedEvent;
        public UnityEvent<bool> PlaybackToggledEvent; // 추가된 Unity Event

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 이벤트 브로드캐스트 메서드
        public void BroadcastTimeChange(float newTime)
        {
            OnTimeChanged?.Invoke(newTime);
            TimeChangedEvent?.Invoke(newTime);
        }

        public void BroadcastModeChange(TemporalVRController.TemporalMode newMode)
        {
            OnModeChanged?.Invoke(newMode);
            ModeChangedEvent?.Invoke((int)newMode);
        }

        public void BroadcastObjectSelected(GameObject selected)
        {
            OnObjectSelected?.Invoke(selected);
            ObjectSelectedEvent?.Invoke(selected);
        }

        // 추가된 메서드
        public void BroadcastPlaybackToggle(bool isPlaying)
        {
            OnPlaybackToggled?.Invoke(isPlaying);
            PlaybackToggledEvent?.Invoke(isPlaying);
        }
    }
}