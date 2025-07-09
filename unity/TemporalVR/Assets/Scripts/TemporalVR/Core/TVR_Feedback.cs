using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TemporalVR
{
    /// <summary>
    /// TemporalVRController의 시각적 피드백을 강화
    /// </summary>
    public class TVRFeedback: MonoBehaviour
    {
        [Header("Controller Reference")]
        private TemporalVRController controller;

        [Header("Mode Visual Indicators")]
        [SerializeField] private GameObject modeIndicatorPrefab;
        private Dictionary<TemporalVRController.TemporalMode, GameObject> modeIndicators;

        [Header("Timeline Enhancement")]
        [SerializeField] private bool showTimeMarkers = true;
        [SerializeField] private int markerCount = 10;
        private List<GameObject> timeMarkers = new List<GameObject>();

        [Header("Interaction Effects")]
        [SerializeField] private GameObject brushTrailPrefab;
        [SerializeField] private ParticleSystem timeParticles;
        private LineRenderer brushTrail;

        [Header("Mode Colors")]
        [SerializeField] private Color scrubColor = new Color(0.2f, 0.4f, 1f);
        [SerializeField] private Color paintColor = new Color(0.2f, 1f, 0.4f);
        [SerializeField] private Color sculptColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private Color previewColor = new Color(1f, 1f, 0.2f);

        void Start()
        {
            controller = GetComponent<TemporalVRController>();
            if (controller == null)
            {
                Debug.LogError("[TemporalVR] Enhanced Feedback requires TemporalVRController!");
                enabled = false;
                return;
            }

            SetupModeIndicators();
            SetupTimelineMarkers();
            SetupInteractionEffects();
        }

        void SetupModeIndicators()
        {
            modeIndicators = new Dictionary<TemporalVRController.TemporalMode, GameObject>();

            // 각 모드별 인디케이터 생성
            CreateModeIndicator(TemporalVRController.TemporalMode.Scrub, scrubColor, new Vector3(0, 0.15f, 0));
            CreateModeIndicator(TemporalVRController.TemporalMode.Paint, paintColor, new Vector3(0.05f, 0.15f, 0));
            CreateModeIndicator(TemporalVRController.TemporalMode.Sculpt, sculptColor, new Vector3(-0.05f, 0.15f, 0));
            CreateModeIndicator(TemporalVRController.TemporalMode.Preview, previewColor, new Vector3(0, 0.15f, 0.05f));
        }

        void CreateModeIndicator(TemporalVRController.TemporalMode mode, Color color, Vector3 localPos)
        {
            GameObject indicator = modeIndicatorPrefab != null ?
                Instantiate(modeIndicatorPrefab, transform) :
                GameObject.CreatePrimitive(PrimitiveType.Sphere);

            indicator.name = $"{mode}Indicator";
            indicator.transform.localPosition = localPos;
            indicator.transform.localScale = Vector3.one * 0.1f;

            // Material 설정
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Unlit Shader 사용 (색상이 더 잘 보임)
                Material mat = new Material(Shader.Find("Sprites/Default"));
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                }
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_BaseColor", color);
                mat.SetColor("_EmissionColor", color * 2f);
                renderer.material = mat;
            }

            // Collider 제거
            Collider col = indicator.GetComponent<Collider>();
            if (col != null) Destroy(col);

            indicator.SetActive(false);
            modeIndicators[mode] = indicator;
        }

        void SetupTimelineMarkers()
        {
            if (!showTimeMarkers) return;

            for (int i = 0; i < markerCount; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = $"TimeMarker_{i}";
                marker.transform.SetParent(transform);

                // 위치 계산
                float t = i / (float)(markerCount - 1);
                float x = Mathf.Lerp(-5f, 5f, t);
                marker.transform.localPosition = new Vector3(x, -0.05f, 0);
                marker.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);

                // Material - 올바른 방법
                Renderer renderer = marker.GetComponent<Renderer>();

                // 방법 1: Sprites/Default 사용
                Material mat = new Material(Shader.Find("Sprites/Default"));
                float colorT = i / (float)(markerCount - 1);
                Color markerColor = Color.Lerp(
                    new Color(0.1f, 0.1f, 0.8f),  // 파란색 (과거)
                    new Color(0.8f, 0.1f, 0.1f),   // 빨간색 (미래)
                    colorT
                );
                mat.color = markerColor;  // _BaseColor 대신 color 사용!

                renderer.material = mat;

                // Collider 제거
                Destroy(marker.GetComponent<Collider>());

                timeMarkers.Add(marker);
            }

            Debug.Log($"[TVRFeedback] Created {markerCount} time markers with gradient colors");
        }

        void SetupInteractionEffects()
        {
            // Brush Trail 설정
            if (brushTrailPrefab != null)
            {
                GameObject trailObj = Instantiate(brushTrailPrefab, transform);
                brushTrail = trailObj.GetComponent<LineRenderer>();
            }
            else
            {
                GameObject trailObj = new GameObject("BrushTrail");
                trailObj.transform.SetParent(transform);
                brushTrail = trailObj.AddComponent<LineRenderer>();
                brushTrail.startWidth = 0.01f;
                brushTrail.endWidth = 0.005f;
                brushTrail.material = new Material(Shader.Find("Sprites/Default"));
                brushTrail.enabled = false;
            }

            // Particle System 설정
            if (timeParticles == null)
            {
                GameObject particleObj = new GameObject("TimeParticles");
                particleObj.transform.SetParent(transform);
                timeParticles = particleObj.AddComponent<ParticleSystem>();

                var main = timeParticles.main;
                main.startLifetime = 2f;
                main.startSpeed = 0.5f;
                main.maxParticles = 50;

                var shape = timeParticles.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;

                var emission = timeParticles.emission;
                emission.enabled = false;
            }
        }

        void Update()
        {
            UpdateModeIndicators();
            UpdateInteractionEffects();
        }

        void UpdateModeIndicators()
        {
            var currentMode = controller.GetCurrentMode();

            foreach (var kvp in modeIndicators)
            {
                kvp.Value.SetActive(kvp.Key == currentMode);
            }
        }

        void UpdateInteractionEffects()
        {
            var currentMode = controller.GetCurrentMode();

            // Paint 모드에서 Trail 효과
            if (currentMode == TemporalVRController.TemporalMode.Paint && brushTrail != null)
            {
                // TODO: 트리거 값에 따라 Trail 활성화
                // 실제 구현시 controller에서 트리거 값을 public으로 노출 필요
            }

            // Sculpt 모드에서 Particle 효과
            if (currentMode == TemporalVRController.TemporalMode.Sculpt && timeParticles != null)
            {
                // TODO: 양손 트리거시 파티클 활성화
            }
        }

        // Gizmos로 디버그 정보 표시
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (controller == null) return;

            // 현재 시간 위치 표시
            float currentTime = controller.GetCurrentTime();
            float normalizedTime = currentTime / 100f; // temporalRange = 100

            Vector3 timePos = Vector3.Lerp(
                transform.position - Vector3.right * 5f,
                transform.position + Vector3.right * 5f,
                normalizedTime
            );

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(timePos + Vector3.up * 0.2f, 0.05f);

            // 모드별 색상으로 컨트롤러 표시
            Gizmos.color = GetModeColor(controller.GetCurrentMode());
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
        }
        public void UpdateModeColor(TemporalVRController.TemporalMode mode)
        {
            Color targetColor = GetModeColor(mode);

            // Timeline Visualizer 직접 참조
            LineRenderer timeline = GameObject.Find("TimelineVisualizer")?.GetComponent<LineRenderer>();
            if (timeline != null)
            {
                // 모드별로 다른 그라데이션 적용
                timeline.startColor = targetColor * 0.7f;
                timeline.endColor = targetColor;
                Debug.Log($"[TVRFeedback] Timeline color updated to {targetColor}");
            }

            // Mode Indicators 업데이트
            if (modeIndicators != null)
            {
                foreach (var kvp in modeIndicators)
                {
                    kvp.Value.SetActive(kvp.Key == mode);
                }
            }
        }

        Color GetModeColor(TemporalVRController.TemporalMode mode)
        {
            switch (mode)
            {
                case TemporalVRController.TemporalMode.Scrub: return scrubColor;
                case TemporalVRController.TemporalMode.Paint: return paintColor;
                case TemporalVRController.TemporalMode.Sculpt: return sculptColor;
                case TemporalVRController.TemporalMode.Preview: return previewColor;
                default: return Color.white;
            }
        }
        // TVRFeedback.cs에 다음 메서드들을 추가하세요:

        /// <summary>
        /// 브러시 효과 표시 (브러시 위치, 크기, 시간값 시각화)
        /// </summary>
        public void ShowBrushEffect(Vector3 position, float radius, float timeValue)
        {
            // 브러시 영역 표시
            StartCoroutine(ShowBrushArea(position, radius, timeValue));
        }

        private IEnumerator ShowBrushArea(Vector3 position, float radius, float timeValue)
        {
            // 임시 브러시 영역 표시 오브젝트
            GameObject brushArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            brushArea.name = "BrushAreaEffect";
            brushArea.transform.position = position;
            brushArea.transform.localScale = Vector3.one * radius * 2f;

            // Collider 제거
            Destroy(brushArea.GetComponent<Collider>());

            // Material 설정
            Renderer renderer = brushArea.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));

            // 시간값에 따른 색상
            float normalizedTime = Mathf.Clamp01(timeValue / 100f);
            Color brushColor = Color.Lerp(
                new Color(0.2f, 0.4f, 1f, 0.3f),  // 과거 = 파란색
                new Color(1f, 0.2f, 0.2f, 0.3f),   // 미래 = 빨간색
                normalizedTime
            );
            mat.color = brushColor;
            renderer.material = mat;

            // 페이드 아웃 애니메이션
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(0.3f, 0f, t);
                float scale = Mathf.Lerp(1f, 1.2f, t);

                // 색상 페이드
                brushColor.a = alpha;
                mat.color = brushColor;

                // 크기 확장
                brushArea.transform.localScale = Vector3.one * radius * 2f * scale;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(brushArea);
        }

        /// <summary>
        /// 브러시 트레일 효과 
        /// </summary>
        public void ShowBrushImpact(Vector3 position, float strength)
        {
            // 충격 지점 효과
            GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impact.transform.position = position;

            // 🔧 이 부분을 수정하세요!
            // 기존: impact.transform.localScale = Vector3.one * 0.05f * (1f + strength);

            // 옵션 1: 더 작게 시작
            impact.transform.localScale = Vector3.one * 0.02f * (1f + strength);

            // 옵션 2: strength 영향을 줄이기
            impact.transform.localScale = Vector3.one * 0.05f * (1f + strength * 0.5f);

            // 옵션 3: 고정 크기
            impact.transform.localScale = Vector3.one * 0.03f;
            var renderer = impact.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Sprites/Default"));

            // 강도에 따른 색상
            Color impactColor = new Color(
                0.1f + (strength * 0.7f),  // R
                0.7f - (strength * 0.4f),  // G  
                1f,                        // B
                0.8f                       // A
            );
            mat.color = impactColor;
            renderer.material = mat;

            Destroy(impact.GetComponent<Collider>());

            Destroy(impact, 0.5f);
        }

        /// <summary>
        /// 연속적인 브러시 스트로크 시각화
        /// </summary>
        private Queue<Vector3> brushTrailPositions = new Queue<Vector3>();
        private GameObject brushTrailObject;
        private LineRenderer brushTrailRenderer;

        public void UpdateBrushTrail(Vector3 position, float timeValue)
        {
            if (brushTrailObject == null)
            {
                brushTrailObject = new GameObject("BrushTrail");
                brushTrailRenderer = brushTrailObject.AddComponent<LineRenderer>();
                brushTrailRenderer.startWidth = 0.02f;
                brushTrailRenderer.endWidth = 0.01f;
                brushTrailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            // 트레일 포인트 추가
            brushTrailPositions.Enqueue(position);
            if (brushTrailPositions.Count > 20) // 최대 20개 포인트
            {
                brushTrailPositions.Dequeue();
            }

            // LineRenderer 업데이트
            Vector3[] positions = brushTrailPositions.ToArray();
            brushTrailRenderer.positionCount = positions.Length;
            brushTrailRenderer.SetPositions(positions);

            // 시간에 따른 그라데이션
            Gradient gradient = new Gradient();
            float normalizedTime = Mathf.Clamp01(timeValue / 100f);
            Color trailColor = Color.Lerp(Color.blue, Color.red, normalizedTime);

            gradient.SetKeys(
                new GradientColorKey[] {
            new GradientColorKey(trailColor, 0f),
            new GradientColorKey(trailColor * 0.5f, 1f)
                },
                new GradientAlphaKey[] {
            new GradientAlphaKey(0.8f, 0f),
            new GradientAlphaKey(0f, 1f)
                }
            );
            brushTrailRenderer.colorGradient = gradient;
        }

        /// <summary>
        /// 브러시 트레일 클리어
        /// </summary>
        public void ClearBrushTrail()
        {
            brushTrailPositions.Clear();
            if (brushTrailRenderer != null)
            {
                brushTrailRenderer.positionCount = 0;
            }
        }
    }
}