using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace TemporalVR
{
    /// <summary>
    /// Vertex 단위로 시간을 제어할 수 있는 개선된 Morph Object
    /// </summary>
    public class TMorphObj_V2 : MonoBehaviour
    {
        [Header("Morph Settings")]
        public List<TKeyframe> keyframes = new List<TKeyframe>();

        [Header("Temporal Data")]
        private TemporalMeshData temporalData;

        [Header("Components")]
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh workingMesh;

        [Header("Visualization")]
        [SerializeField] private bool visualizeTimeGradient = true;
        [SerializeField] private Gradient timeGradient;

        [Header("Brush Settings")]
        [SerializeField] private float brushFalloffPower = 2f;
        [SerializeField] private float timeChangeSpeed = 5f;
        [SerializeField] private bool smoothTimeTransition = true;

        // 원본 메시 데이터
        private Vector3[] originalVertices;
        private Vector3[] originalNormals;
        private Color[] vertexColors;

        // 현재 시간 추적
        private float globalTime = 0f;

        private GameObject currentBrushEffect;  // 현재 활성화된 이펙트
        private Coroutine brushEffectCoroutine; // 실행 중인 코루틴

        [ContextMenu("Create Simple Test Data")]
        void CreateSimpleTestData()
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null) return;

            keyframes.Clear();

            // 키프레임 1: 원본
            TKeyframe kf1 = new TKeyframe();
            kf1.time = 0f;
            kf1.color = Color.blue;
            kf1.vertices = mesh.vertices;
            kf1.normals = mesh.normals;
            keyframes.Add(kf1);

            // 키프레임 2: 확대
            TKeyframe kf2 = new TKeyframe();
            kf2.time = 10f;
            kf2.color = Color.red;
            kf2.vertices = new Vector3[mesh.vertices.Length];
            kf2.normals = mesh.normals;

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                kf2.vertices[i] = mesh.vertices[i] * 1.5f; // 1.5배 확대
            }
            keyframes.Add(kf2);

            // Temporal Data 재초기화
            InitializeTemporalData();

            Debug.Log("Test keyframes created!");
        }

        [ContextMenu("Create Wave Test for Plane")]
        void CreateWaveTestForPlane()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf.mesh == null) return;

            keyframes.Clear();

            // 키프레임 1: 평평한 상태
            TKeyframe kf1 = new TKeyframe();
            kf1.time = 0f;
            kf1.color = Color.blue;
            kf1.CaptureFromMesh(mf.mesh);
            keyframes.Add(kf1);

            // 키프레임 2: 물결 상태
            TKeyframe kf2 = new TKeyframe();
            kf2.time = 10f;
            kf2.color = Color.red;

            // ProBuilder 메시에서 정점 가져오기
            Vector3[] verts = mf.mesh.vertices;
            Vector3[] newVerts = new Vector3[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                newVerts[i] = verts[i];
                // 물결 효과 추가
                newVerts[i].y = Mathf.Sin(verts[i].x * 3f) * 0.3f +
                                Mathf.Sin(verts[i].z * 3f) * 0.3f;
            }

            kf2.vertices = newVerts;
            kf2.normals = mf.mesh.normals;
            keyframes.Add(kf2);

            // 재초기화
            InitializeTemporalData();

            Debug.Log($"Wave test created with {verts.Length} vertices!");
        }
        void Awake()
        {
            InitializeComponents();
            InitializeTemporalData();
        }

        void InitializeComponents()
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

                // Vertex Color를 지원하는 셰이더 사용
                Material mat = new Material(Shader.Find("Unlit/VertexColor"));
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Sprites/Default"));
                }
                meshRenderer.material = mat;
            }

            // Working mesh 생성
            if (meshFilter.sharedMesh != null)
            {
                workingMesh = Instantiate(meshFilter.sharedMesh);
                meshFilter.mesh = workingMesh;

                // 원본 데이터 저장
                originalVertices = workingMesh.vertices;
                originalNormals = workingMesh.normals;

                // Vertex color 배열 초기화
                vertexColors = new Color[originalVertices.Length];
                for (int i = 0; i < vertexColors.Length; i++)
                {
                    vertexColors[i] = Color.white;
                }
                workingMesh.colors = vertexColors;
            }
        }

        void InitializeTemporalData()
        {
            if (workingMesh != null)
            {
                temporalData = new TemporalMeshData(workingMesh.vertexCount);
                temporalData.BuildVertexGroups(workingMesh.triangles);

                // 시간 범위 설정
                if (keyframes.Count > 0)
                {
                    temporalData.minTime = keyframes[0].time;
                    temporalData.maxTime = keyframes[keyframes.Count - 1].time;
                }
            }

            // 기본 그라디언트 설정
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

        /// <summary>
        /// TVR_Controller에서 호출할 메인 메서드
        /// </summary>
        public void ApplyTemporalBrush(Vector3 brushWorldPos, float brushRadius,
                                      float brushStrength, float targetTime)
        {
            if (temporalData == null || workingMesh == null) return;

            // 디버그 로깅 추가
            Debug.Log($"[Brush] World Pos: {brushWorldPos}, Radius: {brushRadius}");
            
            // Brush 위치를 로컬 좌표로 변환
            Vector3 localBrushPos = transform.InverseTransformPoint(brushWorldPos);
            Debug.Log($"[Brush] Local Pos: {localBrushPos}");

            // 영향받는 버텍스 카운트
            int affectedCount = 0;

            // 각 vertex에 대해 시간 변경 적용
            bool anyVertexChanged = false;
            for (int i = 0; i < originalVertices.Length; i++)
            {
                float distance = Vector3.Distance(originalVertices[i], localBrushPos);

                if (distance <= brushRadius)
                {
                    affectedCount++;

                    // 첫 번째 영향받는 버텍스 정보 출력
                    if (affectedCount == 1)
                    {
                        Debug.Log($"[Brush] First affected vertex {i}: pos={originalVertices[i]}, dist={distance}");
                    }

                    // Falloff 계산
                    float normalizedDistance = distance / brushRadius;
                    float falloff = Mathf.Pow(1f - normalizedDistance, brushFalloffPower);

                    // 최종 영향력
                    float influence = brushStrength * falloff;

                    // 시간 변경
                    float currentVertexTime = temporalData.vertexTimes[i];
                    float newTime;

                    if (smoothTimeTransition)
                    {
                        newTime = Mathf.Lerp(currentVertexTime, targetTime,
                                           influence * timeChangeSpeed * Time.deltaTime);
                    }
                    else
                    {
                        newTime = Mathf.Lerp(currentVertexTime, targetTime, influence);
                    }

                    // 시간 범위 제한
                    newTime = Mathf.Clamp(newTime, 0f, 1f);
                    temporalData.vertexTimes[i] = newTime;

                    anyVertexChanged = true;
                }
            }

            Debug.Log($"[Brush] Affected vertices: {affectedCount}/{originalVertices.Length}");

            // 변경사항이 있으면 mesh 업데이트
            if (anyVertexChanged)
            {
                UpdateMeshBasedOnTime();

                // 기존 이펙트가 있으면 업데이트만, 없으면 새로 생성
                UpdateOrCreateBrushEffect(brushWorldPos, brushRadius, targetTime);
            }
        }

        // 이펙트 업데이트 또는 생성
        private void UpdateOrCreateBrushEffect(Vector3 position, float radius, float timeValue)
        {
            if (currentBrushEffect != null)
            {
                // 기존 이펙트 위치만 업데이트
                currentBrushEffect.transform.position = position;
                currentBrushEffect.transform.localScale = Vector3.one * radius * 2f;
            }
            else
            {
                // 새 이펙트 생성
                if (brushEffectCoroutine != null)
                    StopCoroutine(brushEffectCoroutine);

                brushEffectCoroutine = StartCoroutine(ShowBrushEffect(position, radius, timeValue));
            }
        }
        /// <summary>
        /// 각 vertex의 시간에 따라 mesh 형태 업데이트
        /// </summary>
        void UpdateMeshBasedOnTime()
        {
            if (keyframes.Count < 2) return;

            Vector3[] newVertices = new Vector3[originalVertices.Length];
            Vector3[] newNormals = new Vector3[originalNormals.Length];

            // 각 vertex별로 시간에 따른 위치 계산
            for (int i = 0; i < originalVertices.Length; i++)
            {
                float vertexTime = temporalData.vertexTimes[i];
                float actualTime = vertexTime * temporalData.maxTime;

                // 해당 시간에 맞는 keyframe 찾기
                TKeyframe from = null, to = null;
                float localT = 0f;

                FindKeyframesForTime(actualTime, out from, out to, out localT);

                if (from != null && to != null && i < from.vertices.Length && i < to.vertices.Length)
                {
                    // Vertex 위치 보간
                    newVertices[i] = Vector3.Lerp(from.vertices[i], to.vertices[i], localT);

                    // Normal 보간
                    if (from.normals != null && to.normals != null &&
                        i < from.normals.Length && i < to.normals.Length)
                    {
                        newNormals[i] = Vector3.Slerp(from.normals[i], to.normals[i], localT);
                    }

                    // 시간에 따른 색상 (시각화용)
                    if (visualizeTimeGradient)
                    {
                        vertexColors[i] = timeGradient.Evaluate(vertexTime);
                    }
                }
                else
                {
                    // Fallback
                    newVertices[i] = originalVertices[i];
                    newNormals[i] = originalNormals[i];
                    vertexColors[i] = Color.white;
                }
            }

            // Mesh 업데이트
            workingMesh.vertices = newVertices;
            workingMesh.normals = newNormals;

            if (visualizeTimeGradient)
            {
                workingMesh.colors = vertexColors;
            }

            workingMesh.RecalculateBounds();
            workingMesh.RecalculateTangents();
        }

        /// <summary>
        /// 주어진 시간에 해당하는 keyframe 찾기
        /// </summary>
        void FindKeyframesForTime(float time, out TKeyframe from, out TKeyframe to, out float t)
        {
            from = null;
            to = null;
            t = 0f;

            if (keyframes.Count == 0) return;

            // Edge cases
            if (time <= keyframes[0].time)
            {
                from = keyframes[0];
                to = keyframes[0];
                t = 0f;
                return;
            }

            if (time >= keyframes[keyframes.Count - 1].time)
            {
                from = keyframes[keyframes.Count - 1];
                to = keyframes[keyframes.Count - 1];
                t = 1f;
                return;
            }

            // Find surrounding keyframes
            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (time >= keyframes[i].time && time <= keyframes[i + 1].time)
                {
                    from = keyframes[i];
                    to = keyframes[i + 1];

                    float duration = to.time - from.time;
                    if (duration > 0)
                    {
                        t = (time - from.time) / duration;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 브러시 효과 시각화
        /// </summary>
        IEnumerator ShowBrushEffect(Vector3 position, float radius, float timeValue)
        {
            // 기존 이펙트 정리
            if (currentBrushEffect != null)
                Destroy(currentBrushEffect);

            currentBrushEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentBrushEffect.name = "BrushEffect";
            currentBrushEffect.transform.position = position;
            currentBrushEffect.transform.localScale = Vector3.one * radius * 2.0f;

            Destroy(currentBrushEffect.GetComponent<Collider>());

            Renderer renderer = currentBrushEffect.GetComponent<Renderer>();
            Material effectMat = new Material(Shader.Find("Sprites/Default"));

            float normalizedTime = Mathf.Clamp01(timeValue);
            Color effectColor = timeGradient.Evaluate(normalizedTime);
            effectColor.a = 0.3f;
            effectMat.color = effectColor;
            renderer.material = effectMat;

            // 페이드 아웃
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (currentBrushEffect == null) yield break; // 중간에 삭제됐으면 중단

                float t = elapsed / duration;
                effectColor.a = Mathf.Lerp(0.3f, 0f, t);
                effectMat.color = effectColor;
                currentBrushEffect.transform.localScale = Vector3.one * radius * 2f * (1f + t * 0.5f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (currentBrushEffect != null)
            {
                Destroy(currentBrushEffect);
                currentBrushEffect = null;
            }

            brushEffectCoroutine = null;
        }

        /// <summary>
        /// 전체 시간 설정 (테스트용)
        /// </summary>
        public void SetGlobalTime(float time)
        {
            globalTime = Mathf.Clamp01(time);

            // 모든 vertex를 같은 시간으로 설정
            for (int i = 0; i < temporalData.vertexTimes.Length; i++)
            {
                temporalData.vertexTimes[i] = globalTime;
            }

            UpdateMeshBasedOnTime();
        }

        /// <summary>
        /// 현재 평균 시간 가져오기
        /// </summary>
        public float GetAverageTime()
        {
            if (temporalData == null || temporalData.vertexTimes == null) return 0f;

            float sum = 0f;
            foreach (float t in temporalData.vertexTimes)
            {
                sum += t;
            }
            return sum / temporalData.vertexTimes.Length;
        }

        /// <summary>
        /// 시간 범위 가져오기
        /// </summary>
        public void GetTimeRange(out float minTime, out float maxTime)
        {
            minTime = temporalData?.minTime ?? 0f;
            maxTime = temporalData?.maxTime ?? 10f;
        }

        // 테스트용 컨텍스트 메뉴
        [ContextMenu("Reset All Vertices to Start")]
        void ResetToStart()
        {
            SetGlobalTime(0f);
        }

        [ContextMenu("Set All Vertices to End")]
        void SetToEnd()
        {
            SetGlobalTime(1f);
        }

        [ContextMenu("Randomize Vertex Times")]
        void RandomizeVertexTimes()
        {
            if (temporalData == null) return;

            for (int i = 0; i < temporalData.vertexTimes.Length; i++)
            {
                temporalData.vertexTimes[i] = Random.Range(0f, 1f);
            }

            UpdateMeshBasedOnTime();
        }

        // Gizmos로 시간 분포 시각화
        void OnDrawGizmosSelected()
        {
            if (temporalData == null || originalVertices == null || !visualizeTimeGradient) return;

            // 샘플링 (모든 vertex 그리면 너무 많음)
            int step = Mathf.Max(1, originalVertices.Length / 50);

            for (int i = 0; i < originalVertices.Length; i += step)
            {
                Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
                float time = temporalData.vertexTimes[i];

                Gizmos.color = timeGradient.Evaluate(time);
                Gizmos.DrawSphere(worldPos, 0.01f);
            }
        }
    }
}