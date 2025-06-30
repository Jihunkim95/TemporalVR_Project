using UnityEngine;
using System.Collections.Generic;

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
            indicator.transform.localScale = Vector3.one * 0.03f;

            // Material 설정
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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
                float x = Mathf.Lerp(-5f, 5f, t); // timelineLength = 10
                marker.transform.localPosition = new Vector3(x, -0.05f, 0);
                marker.transform.localScale = new Vector3(0.02f, 0.1f, 0.02f);

                // Material
                Renderer renderer = marker.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                float colorT = i / (float)(markerCount - 1);
                mat.SetColor("_BaseColor", Color.Lerp(new Color(0.1f, 0.1f, 0.8f), new Color(0.8f, 0.1f, 0.1f), colorT));
                renderer.material = mat;

                // Collider 제거
                Destroy(marker.GetComponent<Collider>());

                timeMarkers.Add(marker);
            }
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
    }
}