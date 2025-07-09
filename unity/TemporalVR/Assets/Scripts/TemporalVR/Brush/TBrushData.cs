using UnityEngine;
using System.Collections.Generic;

namespace TemporalVR
{
    /// <summary>
    /// Temporal Brush의 스트로크 데이터를 저장하는 구조체
    /// </summary>
    [System.Serializable]
    public struct BrushStroke
    {
        public Vector3 position;
        public float pressure;      // 0-1, 트리거 압력
        public float timeValue;     // 적용할 시간 값
        public float timestamp;     // 스트로크가 기록된 실제 시간

        public BrushStroke(Vector3 pos, float press, float time, float stamp)
        {
            position = pos;
            pressure = press;
            timeValue = time;
            timestamp = stamp;
        }
    }

    /// <summary>
    /// Temporal Brush의 설정과 동작을 관리
    /// </summary>
    public class TemporalBrushData : MonoBehaviour
    {
        [Header("Brush Settings")]
        [SerializeField] private float brushRadius = 0.5f;
        [SerializeField] private AnimationCurve falloffCurve;
        [SerializeField] private float maxTimeChange = 10f;    // 한 번에 변경 가능한 최대 시간

        // Brush Mode 열거형 정의
        public enum BrushMode
        {
            Absolute,    // 절대 시간 설정 (해당 시간으로 직접 설정)
            Relative,    // 상대 시간 변경 (현재 시간에 더하기/빼기)
            Smooth,      // 부드러운 전환 (주변과 블렌딩)
            Ripple       // 파장 효과 (시간이 물결처럼 퍼짐)
        }

        [Header("Brush Modes")]
        [SerializeField] private BrushMode currentMode = BrushMode.Relative;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject brushVisualizerPrefab;
        [SerializeField] private TrailRenderer brushTrail;
        [SerializeField] private ParticleSystem timeParticles;

        // 스트로크 기록
        public bool IsRecording { get; private set; } = false;
        private List<BrushStroke> currentStroke = new List<BrushStroke>();


        // 브러시 시각화
        private GameObject brushVisualizer;
        private Material brushMaterial;

        void Start()
        {
            InitializeBrush();
            SetupFalloffCurve();
        }

        void InitializeBrush()
        {
            // 브러시 시각화 오브젝트 생성
            if (brushVisualizerPrefab != null)
            {
                brushVisualizer = Instantiate(brushVisualizerPrefab, transform);
            }
            else
            {
                // 기본 구 형태의 브러시 생성
                brushVisualizer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                brushVisualizer.transform.SetParent(transform);
                brushVisualizer.transform.localScale = Vector3.one * brushRadius * 2f;

                // 콜라이더 제거
                Destroy(brushVisualizer.GetComponent<Collider>());

                // 반투명 머티리얼 설정
                var renderer = brushVisualizer.GetComponent<Renderer>();
                brushMaterial = new Material(Shader.Find("Sprites/Default"));
                brushMaterial.SetFloat("_Surface", 1); // Transparent
                brushMaterial.SetFloat("_Blend", 0);   // Alpha
                brushMaterial.color = new Color(0.3f, 0.7f, 1f, 0.3f);
                renderer.material = brushMaterial;
            }

            // Trail 설정
            if (brushTrail == null)
            {
                GameObject trailObj = new GameObject("BrushTrail");
                trailObj.transform.SetParent(transform);
                brushTrail = trailObj.AddComponent<TrailRenderer>();
                brushTrail.time = 1f;
                brushTrail.startWidth = brushRadius;
                brushTrail.endWidth = 0.01f;
                brushTrail.material = new Material(Shader.Find("Sprites/Default"));

                // 시간 그라데이션 설정
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.blue, 0.0f),
                        new GradientColorKey(Color.cyan, 0.5f),
                        new GradientColorKey(Color.white, 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.8f, 0.0f),
                        new GradientAlphaKey(0.5f, 0.5f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                brushTrail.colorGradient = gradient;
            }
        }

        void SetupFalloffCurve()
        {
            if (falloffCurve == null || falloffCurve.keys.Length == 0)
            {
                // 기본 Falloff 곡선 생성 (중심에서 가장자리로 갈수록 약해짐)
                falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            }
        }

        /// <summary>
        /// 브러시 스트로크 시작
        /// </summary>
        public void StartStroke(Vector3 position, float pressure, float timeValue)
        {
            IsRecording = true;
            currentStroke.Clear();

            // 첫 번째 포인트 기록
            currentStroke.Add(new BrushStroke(position, pressure, timeValue, Time.time));

            // 시각 효과 시작
            if (brushTrail != null)
                brushTrail.emitting = true;

            if (timeParticles != null)
                timeParticles.Play();

            Debug.Log($"[TemporalBrush] Started stroke at {position} with time {timeValue}");
        }

        /// <summary>
        /// 브러시 스트로크 업데이트 (드래그 중)
        /// </summary>
        public void UpdateStroke(Vector3 position, float pressure, float timeValue)
        {
            if (!IsRecording) return;

            // 스트로크 포인트 추가
            currentStroke.Add(new BrushStroke(position, pressure, timeValue, Time.time));

            // 브러시 위치 업데이트
            if (brushVisualizer != null)
            {
                brushVisualizer.transform.position = position;
                brushVisualizer.transform.localScale = Vector3.one * brushRadius * 2f * pressure;
            }

            // 실시간으로 시간 적용
            ApplyTemporalBrushToNearbyObjects(position, pressure, timeValue);
        }

        /// <summary>
        /// 브러시 스트로크 종료
        /// </summary>
        public void EndStroke()
        {
            IsRecording = false;

            // 시각 효과 중지
            if (brushTrail != null)
                brushTrail.emitting = false;

            if (timeParticles != null)
                timeParticles.Stop();

            Debug.Log($"[TemporalBrush] Ended stroke with {currentStroke.Count} points");

            // 스트로크 데이터 저장 (나중에 Undo/Redo 구현시 사용)
            // SaveStrokeData(currentStroke);
        }

        /// <summary>
        /// 주변 객체에 시간 변화 적용
        /// </summary>
        private void ApplyTemporalBrushToNearbyObjects(Vector3 position, float pressure, float timeValue)
        {
            // 브러시 반경 내의 모든 객체 찾기
            Collider[] colliders = Physics.OverlapSphere(position, brushRadius);

            foreach (var collider in colliders)
            {
                // TMorphObj 찾기
                TMorphObj morphObj = collider.GetComponent<TMorphObj>();
                if (morphObj != null)
                {
                    // 거리 기반 falloff 계산
                    float distance = Vector3.Distance(position, collider.transform.position);
                    float normalizedDistance = distance / brushRadius;
                    float falloff = falloffCurve.Evaluate(normalizedDistance);

                    // 압력과 falloff를 고려한 최종 강도
                    float strength = pressure * falloff;

                    // 모드에 따른 시간 적용
                    float targetTime = CalculateTargetTime(morphObj, timeValue, strength);

                    // 시간 적용
                    morphObj.UpdateToTime(targetTime);

                    // 시각적 피드백
                    ShowTemporalImpact(collider.transform.position, strength, targetTime);
                }
            }
        }

        /// <summary>
        /// 브러시 모드에 따른 목표 시간 계산
        /// </summary>
        private float CalculateTargetTime(TMorphObj morphObj, float brushTime, float strength)
        {
            // 현재 객체의 시간 가져오기 (TMorphObj에 GetCurrentTime 메서드 추가 필요)
            float currentTime = 0f;
            morphObj.GetCurrentTime();

            switch (currentMode)
            {
                case BrushMode.Absolute:
                    // 브러시의 시간값으로 직접 설정
                    return brushTime;

                case BrushMode.Relative:
                    // 현재 시간에 상대적으로 추가
                    float deltaTime = (brushTime - 50f) * 0.2f * strength; // -10 ~ +10 범위
                    return Mathf.Clamp(currentTime + deltaTime, 0f, 100f);

                case BrushMode.Smooth:
                    // 부드럽게 블렌딩
                    return Mathf.Lerp(currentTime, brushTime, strength * 0.1f);

                case BrushMode.Ripple:
                    // 파장 효과 (사인파 적용)
                    float wave = Mathf.Sin(Time.time * 2f) * strength;
                    return currentTime + wave * maxTimeChange;

                default:
                    return brushTime;
            }
        }

        /// <summary>
        /// 시간 변화 시각 효과
        /// </summary>
        private void ShowTemporalImpact(Vector3 position, float strength, float timeValue)
        {
            if (timeParticles != null)
            {
                // 파티클 색상을 시간값에 따라 변경
                var main = timeParticles.main;
                float normalizedTime = timeValue / 100f;
                main.startColor = Color.Lerp(Color.blue, Color.red, normalizedTime);

                // 충격 지점에서 파티클 방출
                timeParticles.transform.position = position;
                timeParticles.Emit((int)(strength * 10));
            }
        }

        /// <summary>
        /// 브러시 설정 변경
        /// </summary>
        public void SetBrushRadius(float radius)
        {
            brushRadius = Mathf.Max(0.1f, radius);
            if (brushVisualizer != null)
                brushVisualizer.transform.localScale = Vector3.one * brushRadius * 2f;
        }

        public void SetBrushMode(BrushMode mode)
        {
            currentMode = mode;
            UpdateBrushVisuals();
        }

        private void UpdateBrushVisuals()
        {
            if (brushMaterial == null) return;

            // 모드별 색상 설정
            Color brushColor = currentMode switch
            {
                BrushMode.Absolute => new Color(1f, 0.3f, 0.3f, 0.3f),
                BrushMode.Relative => new Color(0.3f, 0.7f, 1f, 0.3f),
                BrushMode.Smooth => new Color(0.3f, 1f, 0.3f, 0.3f),
                BrushMode.Ripple => new Color(1f, 0.3f, 1f, 0.3f),
                _ => Color.white
            };

            brushMaterial.color = brushColor;
        }

        // Gizmos로 브러시 영역 표시
        void OnDrawGizmos()
        {
            if (!IsRecording) return;

            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, brushRadius);

            // 스트로크 경로 그리기
            if (currentStroke.Count > 1)
            {
                for (int i = 1; i < currentStroke.Count; i++)
                {
                    Gizmos.color = Color.Lerp(Color.blue, Color.red,
                        currentStroke[i].timeValue / 100f);
                    Gizmos.DrawLine(currentStroke[i - 1].position, currentStroke[i].position);
                }
            }
        }
    }
}