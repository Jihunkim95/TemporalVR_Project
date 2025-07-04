using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    public class TMorphTest : TMorphObj
    {
        [Header("Test Configuration")]
        public bool autoGenerateKeyframes = false;  // 기본값 false로 변경
        public float morphDuration = 10f;

        [Header("Mesh Sources")]
        public Mesh[] directMeshes;
        public GameObject[] meshGameObjects;

        [Header("Color Animation")]
        public bool animateColor = true;
        public Gradient colorOverTime;

        [Header("Debug")]
        public bool showDebugInfo = true;

        protected override void Awake()
        {
            base.Awake();  // TMorphObj의 초기화 먼저 실행
        }

        void Start()
        {
            InitializeGradient();

            // 키프레임이 없으면 더미 데이터 생성
            if (keyframes == null || keyframes.Count < 2)
            {
                Debug.Log("[TMorphTest] No keyframes found. Creating dummy data...");
                CreateDummySetup();
            }
        }

        void InitializeGradient()
        {
            if (colorOverTime == null || colorOverTime.colorKeys.Length == 0)
            {
                colorOverTime = CreateDefaultGradient();
            }
        }

        Gradient CreateDefaultGradient()
        {
            Gradient gradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.blue, 0f),
                new GradientColorKey(Color.cyan, 0.5f),
                new GradientColorKey(Color.red, 1f)
            };
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            };
            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }

        void CreateDummySetup()
        {
            // 1. 간단한 메시 생성
            Mesh dummyMesh = CreateSimpleQuadMesh();
            SetWorkingMesh(dummyMesh);

            // 2. 더미 키프레임 생성
            CreateDummyKeyframes();
        }

        Mesh CreateSimpleQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "SimpleQuad";

            mesh.vertices = new Vector3[]
            {
                new Vector3(-1f, 0f, -1f),
                new Vector3( 1f, 0f, -1f),
                new Vector3(-1f, 0f,  1f),
                new Vector3( 1f, 0f,  1f)
            };

            mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
            mesh.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.RecalculateBounds();
            return mesh;
        }

        void CreateDummyKeyframes()
        {
            keyframes.Clear();

            // 3개의 간단한 키프레임
            keyframes.Add(CreateKeyframe(0f, 0f, Color.blue));      // 평평
            keyframes.Add(CreateKeyframe(5f, 0.3f, Color.cyan));    // 중간
            keyframes.Add(CreateKeyframe(10f, 0.5f, Color.red));    // 최대

            Debug.Log($"[TMorphTest] Created {keyframes.Count} dummy keyframes");
        }

        TKeyframe CreateKeyframe(float time, float waveHeight, Color color)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = color;

            // 웨이브 패턴의 정점들
            kf.vertices = new Vector3[]
            {
                new Vector3(-1f, waveHeight, -1f),
                new Vector3( 1f, -waveHeight, -1f),
                new Vector3(-1f, -waveHeight,  1f),
                new Vector3( 1f, waveHeight,  1f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };

            return kf;
        }

        void Update()
        {
            HandleTestControls();
        }

        void HandleTestControls()
        {
            // 스페이스: 자동 재생
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(PlayAnimation());
            }

            // 좌우 화살표: 수동 제어
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                currentTime = Mathf.Max(0, currentTime - Time.deltaTime * 2f);
                UpdateToTime(currentTime);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                currentTime = Mathf.Min(morphDuration, currentTime + Time.deltaTime * 2f);
                UpdateToTime(currentTime);
            }

            // R: 리셋
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentTime = 0f;
                UpdateToTime(0f);
            }


        }

        float currentTime = 0f;

        IEnumerator PlayAnimation()
        {
            currentTime = 0f;
            while (currentTime < morphDuration)
            {
                UpdateToTime(currentTime);
                currentTime += Time.deltaTime;
                yield return null;
            }
            UpdateToTime(morphDuration);
        }

        void OnDrawGizmos()
        {
            if (!showDebugInfo || keyframes == null || keyframes.Count == 0) return;

            // 타임라인 시각화
            for (int i = 0; i < keyframes.Count; i++)
            {
                float t = keyframes[i].time / morphDuration;
                Vector3 pos = transform.position + Vector3.right * (t * 4f - 2f) + Vector3.up * 2f;

                Gizmos.color = colorOverTime.Evaluate(t);
                Gizmos.DrawWireCube(pos, Vector3.one * 0.2f);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(pos + Vector3.up * 0.3f, $"{keyframes[i].time:F1}s");
#endif
            }

            // 현재 시간 표시
            float currentT = currentTime / morphDuration;
            Vector3 currentPos = transform.position + Vector3.right * (currentT * 4f - 2f) + Vector3.up * 2f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentPos, 0.15f);
        }

        // Context Menu 메서드들
        [ContextMenu("Reset to Dummy")]
        void ResetToDummy()
        {
            CreateDummySetup();
        }

        [ContextMenu("Log Status")]
        void LogStatus()
        {
            Debug.Log($"=== TMorphTest Status ===");
            Debug.Log($"Keyframes: {keyframes.Count}");
            Debug.Log($"Working Mesh: {(workingMesh != null ? workingMesh.name : "null")}");
            Debug.Log($"Mesh Vertices: {(workingMesh != null ? workingMesh.vertexCount : 0)}");
        }
    }
}