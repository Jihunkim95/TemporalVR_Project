using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TemporalVR
{
    /// <summary>
    /// Space-Time Hypercube 개념을 구현하는 개선된 Temporal Object
    /// 브러시로 시간 정보를 마킹하고, 타임라인 재생으로 변형을 확인
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

        [Header("Playback Settings")]
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private float playbackSpeed = 1f;
        [SerializeField] private bool loopPlayback = false;

        [Header("Brush Settings")]
        [SerializeField] private float brushFalloffPower = 2f;
        [SerializeField] private float brushInterpolationStep = 0.5f;
        [SerializeField] private float timeChangeSpeed = 2f;  // 누적 속도 조절 추가

        // 원본 메시 데이터
        private Vector3[] originalVertices;
        private Vector3[] originalNormals;
        private Color[] vertexColors;

        // 시간 추적 - 필드명 변경 (currentTime -> playbackPosition)
        private float playbackPosition = 0f;  // 변경된 필드명
        private float globalTimeScale = 10f;

        // 브러시 효과
        private GameObject currentBrushEffect;
        private Coroutine brushEffectCoroutine;

        // 브러시 연속성
        private Vector3 lastBrushWorldPos;
        private bool wasBrushing = false;

        void Awake()
        {
            InitializeComponents();
            InitializeTemporalData();

            // TemporalEventSystem에 등록
            if (TemporalEventSystem.Instance != null)
            {
                TemporalEventSystem.Instance.OnTimeChanged += OnGlobalTimeChanged;
                TemporalEventSystem.Instance.OnPlaybackToggled += OnPlaybackToggled;
            }
        }

        void OnDestroy()
        {
            // 이벤트 해제
            if (TemporalEventSystem.Instance != null)
            {
                TemporalEventSystem.Instance.OnTimeChanged -= OnGlobalTimeChanged;
                TemporalEventSystem.Instance.OnPlaybackToggled -= OnPlaybackToggled;
            }
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
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Sprites/Default"));
                }
                meshRenderer.material = mat;
            }

            if (meshFilter.sharedMesh != null)
            {
                workingMesh = Instantiate(meshFilter.sharedMesh);
                meshFilter.mesh = workingMesh;

                originalVertices = workingMesh.vertices;
                originalNormals = workingMesh.normals;

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

                if (keyframes.Count > 0)
                {
                    temporalData.minTime = keyframes[0].time;
                    temporalData.maxTime = keyframes[keyframes.Count - 1].time;
                    globalTimeScale = temporalData.maxTime;
                }
            }

            if (timeGradient == null)
            {
                timeGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Color.blue, 0f),    // 과거
                    new GradientColorKey(Color.green, 0.5f), // 현재
                    new GradientColorKey(Color.red, 1f)      // 미래
                };
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                };
                timeGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        void Update()
        {
            // 재생 모드일 때만 애니메이션 업데이트
            if (isPlaying)
            {
                playbackPosition += Time.deltaTime * playbackSpeed;  // 변경된 필드명

                if (playbackPosition > globalTimeScale)
                {
                    if (loopPlayback)
                    {
                        playbackPosition = 0f;
                    }
                    else
                    {
                        playbackPosition = globalTimeScale;
                        isPlaying = false;
                    }
                }

                UpdateMeshForPlayback();
            }
        }

        /// <summary>
        /// 브러시로 시간 정보 마킹 (즉시 변형이 아닌 시간 정보만 저장)
        /// </summary>
        public void MarkTemporalData(Vector3 brushWorldPos, float brushRadius,
                                    float brushStrength, float targetTime)
        {
            if (temporalData == null || workingMesh == null) return;

            // 브러시 연속성 처리
            if (wasBrushing && Vector3.Distance(lastBrushWorldPos, brushWorldPos) > brushRadius * brushInterpolationStep)
            {
                InterpolateBrushStroke(lastBrushWorldPos, brushWorldPos, brushRadius, brushStrength, targetTime);
            }
            else
            {
                MarkTimeAtPosition(brushWorldPos, brushRadius, brushStrength, targetTime);
            }

            lastBrushWorldPos = brushWorldPos;
            wasBrushing = true;

            // 시각적 피드백만 표시 (실제 변형 없음)
            ShowTemporalMarkingFeedback(brushWorldPos, brushRadius, targetTime);
        }

        /// <summary>
        /// 특정 위치에 시간 정보 마킹
        /// </summary>
        /// <summary>
        /// 특정 위치에 시간 정보 마킹 - 누적 방식으로 변경
        /// </summary>
        private void MarkTimeAtPosition(Vector3 brushWorldPos, float brushRadius,
                                       float brushStrength, float targetTime)
        {
            Vector3 localBrushPos = transform.InverseTransformPoint(brushWorldPos);
            float brushRadiusSqr = brushRadius * brushRadius;

            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 diff = originalVertices[i] - localBrushPos;
                float distanceSqr = diff.sqrMagnitude;

                if (distanceSqr <= brushRadiusSqr)
                {
                    float distance = Mathf.Sqrt(distanceSqr);
                    float normalizedDistance = distance / brushRadius;
                    float falloff = Mathf.Pow(1f - normalizedDistance, brushFalloffPower);
                    float influence = brushStrength * falloff;

                    // 시간을 누적하는 방식으로 변경
                    float currentVertexTime = temporalData.vertexTimes[i];

                    // 누적 방식: 기존 시간에 추가
                    float timeIncrement = influence * Time.deltaTime * timeChangeSpeed;
                    float newTime = currentVertexTime + timeIncrement;

                    // 최대값 1로 제한 (빨간색이 최대)
                    temporalData.vertexTimes[i] = Mathf.Clamp01(newTime);
                }
            }

            // 시간 그라디언트 업데이트
            if (visualizeTimeGradient)
            {
                UpdateTimeVisualization();
            }
        }

        /// <summary>
        /// 브러시 스트로크 보간
        /// </summary>
        private void InterpolateBrushStroke(Vector3 fromPos, Vector3 toPos, float radius,
                                          float strength, float targetTime)
        {
            float distance = Vector3.Distance(fromPos, toPos);
            int steps = Mathf.Max(2, Mathf.CeilToInt(distance / (radius * brushInterpolationStep)));

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector3 interpPos = Vector3.Lerp(fromPos, toPos, t);
                MarkTimeAtPosition(interpPos, radius, strength, targetTime);
            }
        }

        /// <summary>
        /// 재생 모드에서 시간에 따른 메시 업데이트
        /// </summary>
        private void UpdateMeshForPlayback()
        {
            if (keyframes.Count < 2) return;

            Vector3[] newVertices = new Vector3[originalVertices.Length];
            Vector3[] newNormals = new Vector3[originalNormals.Length];

            for (int i = 0; i < originalVertices.Length; i++)
            {
                // 버텍스별 개별 시간 계산
                float vertexTimeOffset = temporalData.vertexTimes[i] * globalTimeScale;
                float effectiveTime = playbackPosition + vertexTimeOffset;  // 변경된 필드명

                // 시간 범위 제한
                effectiveTime = Mathf.Clamp(effectiveTime, temporalData.minTime, temporalData.maxTime);

                TKeyframe from = null, to = null;
                float localT = 0f;

                FindKeyframesForTime(effectiveTime, out from, out to, out localT);

                if (from != null && to != null && i < from.vertices.Length && i < to.vertices.Length)
                {
                    newVertices[i] = Vector3.Lerp(from.vertices[i], to.vertices[i], localT);

                    if (from.normals != null && to.normals != null &&
                        i < from.normals.Length && i < to.normals.Length)
                    {
                        newNormals[i] = Vector3.Slerp(from.normals[i], to.normals[i], localT);
                    }
                }
                else
                {
                    newVertices[i] = originalVertices[i];
                    newNormals[i] = originalNormals[i];
                }
            }

            workingMesh.vertices = newVertices;
            workingMesh.normals = newNormals;
            workingMesh.RecalculateBounds();
            workingMesh.RecalculateTangents();
        }

        /// <summary>
        /// 시간 시각화만 업데이트 (메시 변형 없음)
        /// </summary>
        private void UpdateTimeVisualization()
        {
            for (int i = 0; i < vertexColors.Length; i++)
            {
                vertexColors[i] = timeGradient.Evaluate(temporalData.vertexTimes[i]);
            }
            workingMesh.colors = vertexColors;
        }

        /// <summary>
        /// 시간 마킹 시각적 피드백
        /// </summary>
        private void ShowTemporalMarkingFeedback(Vector3 position, float radius, float timeValue)
        {
            if (currentBrushEffect == null)
            {
                currentBrushEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                currentBrushEffect.name = "TemporalMarker";
                Destroy(currentBrushEffect.GetComponent<Collider>());
            }

            currentBrushEffect.transform.position = position;
            currentBrushEffect.transform.localScale = Vector3.one * radius * 0.5f;

            Renderer renderer = currentBrushEffect.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                if (mat == null)
                {
                    mat = new Material(Shader.Find("Sprites/Default"));
                    renderer.material = mat;
                }

                Color markerColor = timeGradient.Evaluate(timeValue);
                markerColor.a = 0.3f;
                mat.color = markerColor;
            }
        }

        /// <summary>
        /// 재생 시작/정지
        /// </summary>
        public void TogglePlayback()
        {
            isPlaying = !isPlaying;
            if (isPlaying && playbackPosition >= globalTimeScale)  // 변경된 필드명
            {
                playbackPosition = 0f;
            }
        }

        /// <summary>
        /// 재생 위치 설정
        /// </summary>
        public void SetPlaybackTime(float time)
        {
            playbackPosition = Mathf.Clamp(time, 0f, globalTimeScale);  // 변경된 필드명
            if (isPlaying)
            {
                UpdateMeshForPlayback();
            }
        }

        /// <summary>
        /// 현재 재생 위치 가져오기
        /// </summary>
        public float GetPlaybackPosition()
        {
            return playbackPosition;
        }

        /// <summary>
        /// 이벤트 핸들러: 전역 시간 변경
        /// </summary>
        private void OnGlobalTimeChanged(float globalTime)
        {
            SetPlaybackTime(globalTime * globalTimeScale);
        }

        /// <summary>
        /// 이벤트 핸들러: 재생 토글
        /// </summary>
        private void OnPlaybackToggled(bool playing)
        {
            isPlaying = playing;
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
        /// 마킹된 시간 데이터 초기화
        /// </summary>
        [ContextMenu("Reset Temporal Marking")]
        public void ResetTemporalMarking()
        {
            if (temporalData != null)
            {
                for (int i = 0; i < temporalData.vertexTimes.Length; i++)
                {
                    temporalData.vertexTimes[i] = 0f;
                }
                UpdateTimeVisualization();
            }
        }

        /// <summary>
        /// 디버깅용: 현재 상태 정보
        /// </summary>
        [ContextMenu("Log Temporal State")]
        void LogTemporalState()
        {
            Debug.Log($"[TMorphObj_V2] State Info:");
            Debug.Log($"- Playback: {(isPlaying ? "Playing" : "Stopped")} at {playbackPosition:F2}/{globalTimeScale:F2}");
            Debug.Log($"- Keyframes: {keyframes.Count}");
            Debug.Log($"- Marked Vertices: {GetMarkedVertexCount()}/{originalVertices?.Length ?? 0}");
        }

        private int GetMarkedVertexCount()
        {
            if (temporalData == null) return 0;
            int count = 0;
            for (int i = 0; i < temporalData.vertexTimes.Length; i++)
            {
                if (temporalData.vertexTimes[i] > 0.01f) count++;
            }
            return count;
        }

        // Gizmos로 시간 분포 시각화
        void OnDrawGizmosSelected()
        {
            if (temporalData == null || originalVertices == null || !visualizeTimeGradient) return;

            int step = Mathf.Max(1, originalVertices.Length / 50);

            for (int i = 0; i < originalVertices.Length; i += step)
            {
                Vector3 worldPos = transform.TransformPoint(originalVertices[i]);
                float time = temporalData.vertexTimes[i];

                if (time > 0.01f) // 마킹된 버텍스만 표시
                {
                    Gizmos.color = timeGradient.Evaluate(time);
                    Gizmos.DrawSphere(worldPos, 0.02f);
                }
            }
        }

        // 테스트용 키프레임 생성 메서드들
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
                kf2.vertices[i] = mesh.vertices[i] * 1.5f;
            }
            keyframes.Add(kf2);

            InitializeTemporalData();
            Debug.Log("Test keyframes created!");
        }

        [ContextMenu("Create Wave Test for Plane")]
        void CreateWaveTestForPlane()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf.mesh == null) return;

            keyframes.Clear();

            TKeyframe kf1 = new TKeyframe();
            kf1.time = 0f;
            kf1.color = Color.blue;
            kf1.CaptureFromMesh(mf.mesh);
            keyframes.Add(kf1);

            TKeyframe kf2 = new TKeyframe();
            kf2.time = 10f;
            kf2.color = Color.red;

            Vector3[] verts = mf.mesh.vertices;
            Vector3[] newVerts = new Vector3[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                newVerts[i] = verts[i];
                newVerts[i].y = Mathf.Sin(verts[i].x * 3f) * 0.3f +
                                Mathf.Sin(verts[i].z * 3f) * 0.3f;
            }

            kf2.vertices = newVerts;
            kf2.normals = mf.mesh.normals;
            keyframes.Add(kf2);

            InitializeTemporalData();
            Debug.Log($"Wave test created with {verts.Length} vertices!");
        }
    }
}