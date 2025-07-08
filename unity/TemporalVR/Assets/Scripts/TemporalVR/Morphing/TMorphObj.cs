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
        protected MeshRenderer meshRenderer;
        protected Mesh workingMesh;
        protected MaterialPropertyBlock propBlock;

        [Header("ES3 Settings")]
        public string saveKey = "TMorphObj_";
        public bool autoSave = false;

        // 업데이트 중 플래그 (충돌 방지)
        private bool isUpdatingMesh = false;
        private object meshLock = new object();

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

                // Sprites/Default 대신 Unlit/Color 사용
                Material mat = new Material(Shader.Find("Sprites/Default"));
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                }
                meshRenderer.material = mat;
            }

            // MaterialPropertyBlock 초기화
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

            lock (meshLock)
            {
                // 기존 workingMesh 정리
                if (workingMesh != null && workingMesh != meshFilter.sharedMesh)
                {
                    DestroyImmediate(workingMesh);
                }

                workingMesh = Instantiate(mesh);
                meshFilter.mesh = workingMesh;
            }
        }

        public void UpdateToTime(float time)
        {
            // 업데이트 중이면 스킵
            if (isUpdatingMesh) return;

            if (keyframes.Count < 2) return;
            if (workingMesh == null) return;

            // 업데이트 시작
            isUpdatingMesh = true;

            try
            {
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

                // 보간
                if (from != null && to != null)
                {
                    float t = (time - from.time) / (to.time - from.time);

                    lock (meshLock)
                    {
                        LerpMesh(from, to, t);
                    }

                    // 색상 보간 (Material 접근은 메인 스레드에서만)
                    if (meshRenderer != null)
                    {
                        LerpColor(from, to, t);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TMorphObj] UpdateToTime error: {e.Message}");
            }
            finally
            {
                isUpdatingMesh = false;
            }
        }

        void ApplyKeyframe(TKeyframe kf)
        {
            if (kf.vertices == null || workingMesh == null) return;

            lock (meshLock)
            {
                workingMesh.vertices = kf.vertices;
                if (kf.normals != null)
                    workingMesh.normals = kf.normals;
                else
                    workingMesh.RecalculateNormals();

                workingMesh.RecalculateBounds();
            }

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

            // 메시 업데이트
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
            if (propBlock == null) propBlock = new MaterialPropertyBlock();

            try
            {
                // MaterialPropertyBlock 사용 (성능 최적화)
                meshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", color);

                // Emission 색상도 설정 (선택사항)
                propBlock.SetColor("_EmissionColor", color * 0.3f);

                meshRenderer.SetPropertyBlock(propBlock);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TMorphObj] Color apply error: {e.Message}");
            }
        }

        // Public 읽기 전용 속성 추가 (PerformanceMonitor를 위해)
        public int KeyframeCount => keyframes?.Count ?? 0;
        public int VertexCount => workingMesh?.vertexCount ?? 0;
        public bool IsUpdating => isUpdatingMesh;

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

        // 정리
        void OnDestroy()
        {
            lock (meshLock)
            {
                if (workingMesh != null && workingMesh != meshFilter.sharedMesh)
                {
                    DestroyImmediate(workingMesh);
                }
            }
        }
    }
}