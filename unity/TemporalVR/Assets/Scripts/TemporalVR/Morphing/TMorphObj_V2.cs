using UnityEngine;
using System.Collections.Generic;

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

        // 원본 메시 데이터
        private Vector3[] originalVertices;
        private Vector3[] originalNormals;
        private Color[] vertexColors;

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
                Material mat = new Material(Shader.Find("Unlit/VertexColor"));
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
            }
        }

        void InitializeTemporalData()
        {
            if (workingMesh != null)
            {
                temporalData = new TemporalMeshData(workingMesh.vertexCount);
                temporalData.BuildVertexGroups(workingMesh.triangles);
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
        /// Temporal Brush 적용 - 핵심 메서드
        /// </summary>
        public void ApplyTemporalBrush(Vector3 brushPosition, float brushRadius,
                                      float strength, float targetTime,
                                      TemporalBrushData.BrushMode mode)
        {
            if (temporalData == null || workingMesh == null) return;

            // Brush mode에 따른 target time 계산
            float adjustedTargetTime = CalculateTargetTime(brushPosition, targetTime, mode);

            // Vertex별 시간 업데이트
            temporalData.ApplyTemporalBrush(
                brushPosition,
                brushRadius,
                strength,
                adjustedTargetTime,
                originalVertices,
                transform
            );

            // Mesh 업데이트
            UpdateMeshBasedOnTime();
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

                // 해당 시간에 맞는 keyframe 찾기
                TKeyframe from = null, to = null;
                float localT = 0f;

                FindKeyframesForTime(vertexTime * temporalData.maxTime, out from, out to, out localT);

                if (from != null && to != null && i < from.vertices.Length && i < to.vertices.Length)
                {
                    // Vertex 위치 보간
                    newVertices[i] = Vector3.Lerp(from.vertices[i], to.vertices[i], localT);

                    // Normal 보간
                    if (from.normals != null && to.normals != null)
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
                    t = (time - from.time) / (to.time - from.time);
                    break;
                }
            }
        }

        /// <summary>
        /// Brush mode에 따른 target time 계산
        /// </summary>
        float CalculateTargetTime(Vector3 brushPosition, float brushTime, TemporalBrushData.BrushMode mode)
        {
            switch (mode)
            {
                case TemporalBrushData.BrushMode.Absolute:
                    return brushTime;

                case TemporalBrushData.BrushMode.Relative:
                    // 브러시 움직임 방향에 따라 시간 증감
                    // TODO: 이전 브러시 위치와 비교하여 방향 계산
                    return brushTime;

                case TemporalBrushData.BrushMode.Smooth:
                    // 주변 vertex들의 평균 시간과 블렌딩
                    return brushTime;

                case TemporalBrushData.BrushMode.Ripple:
                    // 시간에 따른 파동 효과
                    float wave = Mathf.Sin(Time.time * 2f) * 0.1f;
                    return brushTime + wave;

                default:
                    return brushTime;
            }
        }

        /// <summary>
        /// 전체 메시의 평균 시간 (디버깅용)
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
        /// 특정 위치에서의 시간 값 가져오기
        /// </summary>
        public float GetTimeAtPosition(Vector3 worldPosition)
        {
            if (temporalData == null || originalVertices == null) return 0f;

            // 가장 가까운 vertex 찾기
            float minDistance = float.MaxValue;
            int closestVertex = 0;

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 worldVertex = transform.TransformPoint(originalVertices[i]);
                float distance = Vector3.Distance(worldVertex, worldPosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestVertex = i;
                }
            }

            return temporalData.vertexTimes[closestVertex];
        }

        // Gizmos로 시간 분포 시각화
        void OnDrawGizmosSelected()
        {
            if (temporalData == null || originalVertices == null) return;
            if (!visualizeTimeGradient) return;

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
                float time = temporalData.vertexTimes[i];

                Gizmos.color = timeGradient.Evaluate(time);
                Gizmos.DrawSphere(worldPos, 0.01f);
            }
        }
    }
}