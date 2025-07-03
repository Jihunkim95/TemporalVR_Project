using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    public class TMorphObj : TemporalObject
    {
        [Header("Morph Settings")]
        public List<TKeyframe> keyframes = new List<TKeyframe>();
        private MeshFilter meshFilter;
        private Mesh workingMesh;  // 보간용 메시

        [Header("ES3 Settings")]
        public string saveKey = "TMorphObj_";
        public bool autoSave = false;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            // 원본 메시 복사 (수정을 위해)
            workingMesh = Instantiate(meshFilter.mesh);
            meshFilter.mesh = workingMesh;
        }

        // ES3 통합
        public void SaveMorphData()
        {
            ES3.Save(saveKey + gameObject.name + "_keyframes", keyframes);
        }

        public void LoadMorphData()
        {
            string key = saveKey + gameObject.name + "_keyframes";
            if (ES3.KeyExists(key))
            {
                keyframes = ES3.Load<List<TKeyframe>>(key);
            }
        }

        // TVR_Controller 연동
        public void UpdateToTime(float time)
        {
            if (keyframes.Count < 2) return;

            // 현재 시간에 해당하는 키프레임 찾기
            TKeyframe from = null, to = null;
            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (time >= keyframes[i].time && time <= keyframes[i + 1].time)
                {
                    from = keyframes[i];
                    to = keyframes[i + 1];
                    break;
                }
            }

            if (from != null && to != null)
            {
                // 보간 비율 계산
                float t = (time - from.time) / (to.time - from.time);
                LerpMesh(from, to, t);
            }
        }
        // TemporalObject의 가상 메서드 오버라이드
        public override void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            base.ApplyTemporalBrush(position, strength, time);
            // 모핑 특화 브러시 효과
        }
        void LerpMesh(TKeyframe from, TKeyframe to, float t)
        {
            Vector3[] lerpedVertices = new Vector3[from.vertices.Length];
            for (int i = 0; i < lerpedVertices.Length; i++)
            {
                lerpedVertices[i] = Vector3.Lerp(from.vertices[i], to.vertices[i], t);
            }

            workingMesh.vertices = lerpedVertices;
            workingMesh.RecalculateBounds();
            workingMesh.RecalculateNormals();
        }
    }
}