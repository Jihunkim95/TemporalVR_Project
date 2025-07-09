using UnityEngine;
using System.Collections.Generic;

namespace TemporalVR
{
    /// <summary>
    /// Mesh의 각 Vertex가 개별적인 시간을 가질 수 있도록 하는 데이터 구조
    /// </summary>
    [System.Serializable]
    public class TemporalMeshData
    {
        // Vertex별 현재 시간 (0.0 ~ 1.0)
        public float[] vertexTimes;

        // Vertex별 시간 변화 속도
        public float[] timeVelocities;

        // 전체 시간 범위
        public float minTime = 0f;
        public float maxTime = 10f;

        // Vertex 그룹 (연결된 vertex들을 함께 처리)
        public Dictionary<int, List<int>> vertexGroups;

        public TemporalMeshData(int vertexCount)
        {
            vertexTimes = new float[vertexCount];
            timeVelocities = new float[vertexCount];
            vertexGroups = new Dictionary<int, List<int>>();

            // 초기화: 모든 vertex를 0 시간으로
            for (int i = 0; i < vertexCount; i++)
            {
                vertexTimes[i] = 0f;
                timeVelocities[i] = 0f;
            }
        }

        /// <summary>
        /// 특정 위치 주변의 vertex들의 시간을 변경
        /// </summary>
        public void ApplyTemporalBrush(Vector3 brushPosition, float brushRadius,
                                      float strength, float targetTime,
                                      Vector3[] meshVertices, Transform meshTransform)
        {
            for (int i = 0; i < meshVertices.Length; i++)
            {
                // World space로 변환
                Vector3 worldVertex = meshTransform.TransformPoint(meshVertices[i]);
                float distance = Vector3.Distance(worldVertex, brushPosition);

                if (distance <= brushRadius)
                {
                    // Falloff 계산
                    float falloff = 1f - (distance / brushRadius);
                    falloff = Mathf.Pow(falloff, 2f); // Smooth falloff

                    // 시간 변경 적용
                    float influence = strength * falloff;

                    // Smooth blending
                    vertexTimes[i] = Mathf.Lerp(vertexTimes[i], targetTime, influence * Time.deltaTime);

                    // 주변 vertex에도 영향 전파 (optional)
                    PropagateTimeChange(i, influence * 0.5f);
                }
            }
        }

        /// <summary>
        /// 인접한 vertex들에게 시간 변화 전파
        /// </summary>
        private void PropagateTimeChange(int vertexIndex, float influence)
        {
            if (vertexGroups.ContainsKey(vertexIndex))
            {
                foreach (int neighborIndex in vertexGroups[vertexIndex])
                {
                    if (neighborIndex < vertexTimes.Length)
                    {
                        float currentDiff = Mathf.Abs(vertexTimes[vertexIndex] - vertexTimes[neighborIndex]);
                        if (currentDiff > 0.1f) // Threshold
                        {
                            vertexTimes[neighborIndex] = Mathf.Lerp(
                                vertexTimes[neighborIndex],
                                vertexTimes[vertexIndex],
                                influence
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mesh topology를 분석하여 연결된 vertex 그룹 생성
        /// </summary>
        public void BuildVertexGroups(int[] triangles)
        {
            vertexGroups.Clear();

            // Triangle을 통해 연결된 vertex 찾기
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int v0 = triangles[i];
                int v1 = triangles[i + 1];
                int v2 = triangles[i + 2];

                // v0의 이웃에 v1, v2 추가
                AddNeighbor(v0, v1);
                AddNeighbor(v0, v2);

                // v1의 이웃에 v0, v2 추가
                AddNeighbor(v1, v0);
                AddNeighbor(v1, v2);

                // v2의 이웃에 v0, v1 추가
                AddNeighbor(v2, v0);
                AddNeighbor(v2, v1);
            }
        }

        private void AddNeighbor(int vertex, int neighbor)
        {
            if (!vertexGroups.ContainsKey(vertex))
            {
                vertexGroups[vertex] = new List<int>();
            }

            if (!vertexGroups[vertex].Contains(neighbor))
            {
                vertexGroups[vertex].Add(neighbor);
            }
        }
    }
}