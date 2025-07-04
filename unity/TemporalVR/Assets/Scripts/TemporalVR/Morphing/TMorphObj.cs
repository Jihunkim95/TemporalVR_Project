using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    public class TMorphObj : TemporalObject
    {
        [Header("Morph Settings")]
        public List<TKeyframe> keyframes = new List<TKeyframe>();

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;  // 추가
        protected Mesh workingMesh;
        protected MaterialPropertyBlock propBlock;  // 추가

        [Header("ES3 Settings")]
        public string saveKey = "TMorphObj_";
        public bool autoSave = false;

        protected virtual void Awake()
        {
            InitializeMesh();
        }

        protected void InitializeMesh()
        {
            // MeshFilter 확인
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            // MeshRenderer 확인
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            // MaterialPropertyBlock 초기화 (성능 향상)
            propBlock = new MaterialPropertyBlock();

            // 메시가 있으면 복사
            if (meshFilter.sharedMesh != null)
            {
                workingMesh = Instantiate(meshFilter.sharedMesh);
                meshFilter.mesh = workingMesh;
            }
        }

        public void SetWorkingMesh(Mesh mesh)
        {
            if (mesh == null) return;

            workingMesh = Instantiate(mesh);
            meshFilter.mesh = workingMesh;
        }

        public void UpdateToTime(float time)
        {
            if (keyframes.Count < 2) return;
            if (workingMesh == null) return;

            time = Mathf.Clamp(time, keyframes[0].time, keyframes[keyframes.Count - 1].time);

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

            // 경계 처리
            if (from == null)
            {
                if (time <= keyframes[0].time)
                {
                    ApplyKeyframe(keyframes[0]);
                }
                else if (time >= keyframes[keyframes.Count - 1].time)
                {
                    ApplyKeyframe(keyframes[keyframes.Count - 1]);
                }
                return;
            }



            // 마지막에 색상 업데이트 추가
            if (meshRenderer != null && from != null && to != null)
            {
                float t = (time - from.time) / (to.time - from.time);
                // 보간
                LerpMesh(from, to, t);
                // 색상 보간 추가
                LerpColor(from, to, t);
                Color lerpedColor = Color.Lerp(from.color, to.color, t);
                meshRenderer.material.color = lerpedColor;
            }
        }

        void ApplyKeyframe(TKeyframe kf)
        {
            if (kf.vertices == null || workingMesh == null) return;

            workingMesh.vertices = kf.vertices;
            if (kf.normals != null)
                workingMesh.normals = kf.normals;
            else
                workingMesh.RecalculateNormals();

            workingMesh.RecalculateBounds();

            // 색상 적용
            ApplyColor(kf.color);
        }

        void LerpMesh(TKeyframe from, TKeyframe to, float t)
        {
            if (from.vertices == null || to.vertices == null) return;
            if (from.vertices.Length != to.vertices.Length) return;
            if (workingMesh == null) return;

            // 정점 보간
            Vector3[] lerpedVertices = new Vector3[from.vertices.Length];
            for (int i = 0; i < lerpedVertices.Length; i++)
            {
                lerpedVertices[i] = Vector3.Lerp(from.vertices[i], to.vertices[i], t);
            }
            workingMesh.vertices = lerpedVertices;

            // 법선 보간
            if (from.normals != null && to.normals != null &&
                from.normals.Length == to.normals.Length)
            {
                Vector3[] lerpedNormals = new Vector3[from.normals.Length];
                for (int i = 0; i < lerpedNormals.Length; i++)
                {
                    lerpedNormals[i] = Vector3.Slerp(from.normals[i], to.normals[i], t);
                }
                workingMesh.normals = lerpedNormals;
            }
            else
            {
                workingMesh.RecalculateNormals();
            }

            workingMesh.RecalculateBounds();
        }

        // 색상 보간 메서드
        void LerpColor(TKeyframe from, TKeyframe to, float t)
        {
            Color lerpedColor = Color.Lerp(from.color, to.color, t);
            ApplyColor(lerpedColor);
        }

        // 색상 적용 메서드
        void ApplyColor(Color color)
        {
            if (meshRenderer == null) return;

            // MaterialPropertyBlock 사용 (성능 최적화)
            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);

            // Emission 색상도 설정 (선택사항)
            propBlock.SetColor("_EmissionColor", color * 0.3f);

            meshRenderer.SetPropertyBlock(propBlock);
        }

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

        public override void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            base.ApplyTemporalBrush(position, strength, time);
        }
    }
}