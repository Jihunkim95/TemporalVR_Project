using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TemporalVR
{
    [System.Serializable]
    public class TKeyframe
    {
        public float time;
        public Vector3[] vertices;      // 메시 정점 데이터 직접 저장
        public Vector3[] normals;       // 법선 데이터
        public Color color;

        // Mesh에서 데이터 추출
        public void CaptureFromMesh(Mesh mesh)
        {
            vertices = mesh.vertices;
            normals = mesh.normals;
        }

        // Mesh에 데이터 적용
        public void ApplyToMesh(Mesh mesh)
        {
            if (vertices != null && vertices.Length == mesh.vertexCount)
            {
                mesh.vertices = vertices;
                mesh.normals = normals;
                mesh.RecalculateBounds();
            }
        }
    }
}