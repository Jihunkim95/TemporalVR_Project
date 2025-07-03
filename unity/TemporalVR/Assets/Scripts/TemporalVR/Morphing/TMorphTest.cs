using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    public class TMorphTest : TMorphObj
    {
        [Header("Test Configuration")]
        public bool autoGenerateKeyframes = true;
        public float morphDuration = 10f;  // 전체 모핑 시간

        [Header("Mesh Sources")]
        [Tooltip("직접 메시 할당 (권장)")]
        public Mesh[] directMeshes;  // 우선순위 1

        [Tooltip("게임오브젝트에서 메시 추출")]
        public GameObject[] meshGameObjects;  // 우선순위 2

        [Header("Color Animation")]
        public bool animateColor = true;
        public Gradient colorOverTime;  // 시간에 따른 색상 변화

        [Header("Debug")]
        public bool showDebugInfo = true;

        void Start()
        {
            // 기본 Gradient 설정
            if (colorOverTime == null || colorOverTime.colorKeys.Length == 0)
            {
                colorOverTime = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(Color.blue, 0f);
                colorKeys[1] = new GradientColorKey(Color.cyan, 0.5f);
                colorKeys[2] = new GradientColorKey(Color.red, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                colorOverTime.SetKeys(colorKeys, alphaKeys);
            }

            // 키프레임 생성 또는 로드
            if (autoGenerateKeyframes && HasValidMeshSources())
            {
                GenerateKeyframes();
                if (autoSave) SaveMorphData();
            }
            else if (ES3.KeyExists(saveKey + gameObject.name + "_keyframes"))
            {
                LoadMorphData();
                if (showDebugInfo)
                    Debug.Log($"[TMorphTest] Loaded {keyframes.Count} keyframes from save");
            }
        }

        bool HasValidMeshSources()
        {
            return (directMeshes != null && directMeshes.Length > 0) ||
                   (meshGameObjects != null && meshGameObjects.Length > 0);
        }

        void GenerateKeyframes()
        {
            keyframes.Clear();
            List<Mesh> meshList = CollectMeshes();

            if (meshList.Count < 2)
            {
                Debug.LogWarning("[TMorphTest] Need at least 2 meshes for morphing!");
                return;
            }

            // 정점 수 검증
            int vertexCount = meshList[0].vertexCount;
            bool allMatch = true;

            for (int i = 1; i < meshList.Count; i++)
            {
                if (meshList[i].vertexCount != vertexCount)
                {
                    Debug.LogError($"[TMorphTest] Vertex count mismatch! Mesh 0: {vertexCount}, Mesh {i}: {meshList[i].vertexCount}");
                    allMatch = false;
                }
            }

            if (!allMatch)
            {
                Debug.LogError("[TMorphTest] All meshes must have the same vertex count!");
                return;
            }

            // 키프레임 생성
            for (int i = 0; i < meshList.Count; i++)
            {
                TKeyframe kf = new TKeyframe();

                // 시간 계산
                float normalizedTime = (meshList.Count > 1) ?
                    i / (float)(meshList.Count - 1) : 0f;
                kf.time = normalizedTime * morphDuration;

                // 메시 데이터 캡처
                kf.CaptureFromMesh(meshList[i]);

                // 색상 설정
                if (animateColor)
                {
                    kf.color = colorOverTime.Evaluate(normalizedTime);
                }
                else
                {
                    kf.color = Color.white;
                }

                keyframes.Add(kf);

                if (showDebugInfo)
                {
                    Debug.Log($"[TMorphTest] Keyframe {i}: Time={kf.time:F2}s, Vertices={kf.vertices.Length}, Color={kf.color}");
                }
            }

            Debug.Log($"[TMorphTest] Generated {keyframes.Count} keyframes over {morphDuration} seconds");
        }

        List<Mesh> CollectMeshes()
        {
            List<Mesh> meshes = new List<Mesh>();

            // 우선순위 1: Direct Meshes
            if (directMeshes != null && directMeshes.Length > 0)
            {
                foreach (var mesh in directMeshes)
                {
                    if (mesh != null)
                        meshes.Add(mesh);
                }

                if (showDebugInfo)
                    Debug.Log($"[TMorphTest] Using {meshes.Count} direct meshes");

                return meshes;
            }

            // 우선순위 2: GameObject에서 추출
            if (meshGameObjects != null && meshGameObjects.Length > 0)
            {
                foreach (var go in meshGameObjects)
                {
                    if (go != null)
                    {
                        MeshFilter mf = go.GetComponent<MeshFilter>();
                        if (mf != null && mf.sharedMesh != null)
                        {
                            meshes.Add(mf.sharedMesh);
                        }
                    }
                }

                if (showDebugInfo)
                    Debug.Log($"[TMorphTest] Extracted {meshes.Count} meshes from GameObjects");
            }

            return meshes;
        }

        // 테스트 헬퍼 메서드들
        [ContextMenu("Generate Test Keyframes")]
        void ForceGenerateKeyframes()
        {
            GenerateKeyframes();
            Debug.Log("[TMorphTest] Manually generated keyframes");
        }

        [ContextMenu("Clear Keyframes")]
        void ClearKeyframes()
        {
            keyframes.Clear();
            Debug.Log("[TMorphTest] Cleared all keyframes");
        }

        [ContextMenu("Preview at 50%")]
        void PreviewMidpoint()
        {
            UpdateToTime(morphDuration * 0.5f);
        }

        // 기본 테스트 메시 생성
        [ContextMenu("Create Cube to Sphere Test")]
        void CreateDefaultTest()
        {
            // ProBuilder나 기본 Unity 메시 사용
            directMeshes = new Mesh[2];

            // 큐브와 구를 임시로 생성해서 메시 추출
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            directMeshes[0] = tempCube.GetComponent<MeshFilter>().sharedMesh;
            directMeshes[1] = tempSphere.GetComponent<MeshFilter>().sharedMesh;

            // 임시 객체 삭제
            DestroyImmediate(tempCube);
            DestroyImmediate(tempSphere);

            Debug.Log("[TMorphTest] Created default cube-to-sphere test meshes");
            Debug.LogWarning("Note: Unity's default cube and sphere have different vertex counts. Use subdivided meshes for better results!");
        }
    }
}