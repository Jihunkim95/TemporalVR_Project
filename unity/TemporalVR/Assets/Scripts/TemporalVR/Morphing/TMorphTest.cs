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

            // 항상 메시가 있는지 확인
            if (meshFilter.sharedMesh == null && workingMesh == null)
            {
                Debug.Log("[TMorphTest] No mesh found. Creating default quad...");
                Mesh mesh = CreateSimpleQuadMesh();
                SetWorkingMesh(mesh);
            }

            // 키프레임이 없으면 더미 데이터 생성
            if (keyframes == null || keyframes.Count < 2)
            {
                Debug.Log("[TMorphTest] No keyframes found. Creating dummy data...");
                CreateDummyKeyframes();  // 메시는 이미 위에서 생성됨
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

        // ========== 새로운 변형 메서드들 추가 ==========

        [ContextMenu("Create Sphere Morph")]
        void CreateSphereMorph()
        {
            keyframes.Clear();
            morphDuration = 10f;

            // Quad → Sphere 변형
            keyframes.Add(CreateQuadKeyframe(0f));
            keyframes.Add(CreateDomeKeyframe(3f));
            keyframes.Add(CreateSphereKeyframe(7f));
            keyframes.Add(CreatePerfectSphereKeyframe(10f));

            Debug.Log("[TMorphTest] Created Sphere Morph keyframes");
        }

        TKeyframe CreateQuadKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            kf.vertices = new Vector3[]
            {
                new Vector3(-1f, 0f, -1f),
                new Vector3( 1f, 0f, -1f),
                new Vector3(-1f, 0f,  1f),
                new Vector3( 1f, 0f,  1f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateDomeKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 돔 형태
            kf.vertices = new Vector3[]
            {
                new Vector3(-0.8f, 0.5f, -0.8f),
                new Vector3( 0.8f, 0.5f, -0.8f),
                new Vector3(-0.8f, 0.5f,  0.8f),
                new Vector3( 0.8f, 0.5f,  0.8f)
            };

            kf.normals = new Vector3[]
            {
                new Vector3(-0.5f, 0.5f, -0.5f).normalized,
                new Vector3( 0.5f, 0.5f, -0.5f).normalized,
                new Vector3(-0.5f, 0.5f,  0.5f).normalized,
                new Vector3( 0.5f, 0.5f,  0.5f).normalized
            };

            return kf;
        }

        TKeyframe CreateSphereKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 더 구체적인 형태
            kf.vertices = new Vector3[]
            {
                new Vector3(-0.6f, 0.8f, -0.6f),
                new Vector3( 0.6f, 0.8f, -0.6f),
                new Vector3(-0.6f, 0.8f,  0.6f),
                new Vector3( 0.6f, 0.8f,  0.6f)
            };

            kf.normals = new Vector3[]
            {
                new Vector3(-1, 1, -1).normalized,
                new Vector3( 1, 1, -1).normalized,
                new Vector3(-1, 1,  1).normalized,
                new Vector3( 1, 1,  1).normalized
            };

            return kf;
        }

        TKeyframe CreatePerfectSphereKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 완전한 구 형태 근사
            float r = 0.5f;
            kf.vertices = new Vector3[]
            {
                new Vector3(-r, r * 1.4f, -r),
                new Vector3( r, r * 1.4f, -r),
                new Vector3(-r, r * 1.4f,  r),
                new Vector3( r, r * 1.4f,  r)
            };

            // 구체의 normal
            kf.normals = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                kf.normals[i] = kf.vertices[i].normalized;
            }

            return kf;
        }

        [ContextMenu("Create Star Morph")]
        void CreateStarMorph()
        {
            keyframes.Clear();
            morphDuration = 10f;

            // Quad → Star 변형
            keyframes.Add(CreateQuadKeyframe(0f));
            keyframes.Add(CreateDiamondKeyframe(3f));
            keyframes.Add(CreateStarKeyframe(7f));
            keyframes.Add(CreateSpikeyStarKeyframe(10f));

            Debug.Log("[TMorphTest] Created Star Morph keyframes");
        }

        TKeyframe CreateDiamondKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 다이아몬드 형태
            kf.vertices = new Vector3[]
            {
                new Vector3(-1.2f, 0f, 0f),     // 왼쪽
                new Vector3( 0f, 0f, -1.2f),    // 위
                new Vector3( 1.2f, 0f, 0f),     // 오른쪽
                new Vector3( 0f, 0f,  1.2f)     // 아래
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateStarKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 별 형태
            kf.vertices = new Vector3[]
            {
                new Vector3(-1.8f, 0.2f, -0.3f),    // 왼쪽 뾰족
                new Vector3( 0.3f, 0.2f, -1.8f),    // 위 뾰족
                new Vector3( 1.8f, 0.2f,  0.3f),    // 오른쪽 뾰족
                new Vector3(-0.3f, 0.2f,  1.8f)     // 아래 뾰족
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateSpikeyStarKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = colorOverTime.Evaluate(time / morphDuration);

            // 더 뾰족한 별
            kf.vertices = new Vector3[]
            {
                new Vector3(-2.5f, 0.5f, 0f),
                new Vector3( 0f, 0.5f, -2.5f),
                new Vector3( 2.5f, 0.5f, 0f),
                new Vector3( 0f, 0.5f,  2.5f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        [ContextMenu("Create Flower Growth")]
        void CreateFlowerGrowth()
        {
            keyframes.Clear();
            morphDuration = 12f;

            // 씨앗 → 새싹 → 꽃
            keyframes.Add(CreateSeedKeyframe(0f));
            keyframes.Add(CreateSproutKeyframe(3f));
            keyframes.Add(CreateBudKeyframe(6f));
            keyframes.Add(CreateFlowerKeyframe(10f));

            Debug.Log("[TMorphTest] Created Flower Growth keyframes");
        }

        TKeyframe CreateSeedKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = new Color(0.4f, 0.3f, 0.2f); // 갈색

            // 작은 씨앗
            kf.vertices = new Vector3[]
            {
                new Vector3(-0.1f, 0f, -0.1f),
                new Vector3( 0.1f, 0f, -0.1f),
                new Vector3(-0.1f, 0f,  0.1f),
                new Vector3( 0.1f, 0f,  0.1f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateSproutKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = new Color(0.2f, 0.8f, 0.2f); // 초록색

            // 새싹
            kf.vertices = new Vector3[]
            {
                new Vector3(-0.2f, 0.3f, -0.2f),
                new Vector3( 0.2f, 0.5f, -0.2f),
                new Vector3(-0.2f, 0.5f,  0.2f),
                new Vector3( 0.2f, 0.3f,  0.2f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateBudKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = new Color(0.8f, 0.2f, 0.5f); // 분홍색

            // 꽃봉오리
            kf.vertices = new Vector3[]
            {
                new Vector3(-0.4f, 0.8f, -0.4f),
                new Vector3( 0.4f, 1f, -0.4f),
                new Vector3(-0.4f, 1f,  0.4f),
                new Vector3( 0.4f, 0.8f,  0.4f)
            };

            kf.normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            return kf;
        }

        TKeyframe CreateFlowerKeyframe(float time)
        {
            TKeyframe kf = new TKeyframe();
            kf.time = time;
            kf.color = new Color(1f, 0.3f, 0.6f); // 밝은 분홍

            // 꽃
            kf.vertices = new Vector3[]
            {
                new Vector3(-1f, 1.2f, -1f),
                new Vector3( 1f, 1.5f, -1f),
                new Vector3(-1f, 1.5f,  1f),
                new Vector3( 1f, 1.2f,  1f)
            };

            kf.normals = new Vector3[]
            {
                new Vector3(-1, 1, -1).normalized,
                new Vector3( 1, 1, -1).normalized,
                new Vector3(-1, 1,  1).normalized,
                new Vector3( 1, 1,  1).normalized
            };

            return kf;
        }

        // ========== 기존 메서드들 ==========

        void Update()
        {

            HandleTestControls();

            // 색상 애니메이션 추가
            if (animateColor && meshRenderer != null)
            {
                float normalizedTime = currentTime / morphDuration;
                Color timeColor = colorOverTime.Evaluate(normalizedTime);

                if (propBlock == null) propBlock = new MaterialPropertyBlock();
                propBlock.SetColor("_BaseColor", timeColor);
                propBlock.SetColor("_EmissionColor", timeColor * 0.3f);
                meshRenderer.SetPropertyBlock(propBlock);
            }
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

            // 숫자 키로 프리셋 선택
            if (Input.GetKeyDown(KeyCode.Alpha1)) CreateDummyKeyframes();
            if (Input.GetKeyDown(KeyCode.Alpha2)) CreateSphereMorph();
            if (Input.GetKeyDown(KeyCode.Alpha3)) CreateStarMorph();
            if (Input.GetKeyDown(KeyCode.Alpha4)) CreateFlowerGrowth();
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

                Gizmos.color = keyframes[i].color;
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
            Debug.Log($"Current Time: {currentTime:F2} / {morphDuration:F2}");
        }
    }
}