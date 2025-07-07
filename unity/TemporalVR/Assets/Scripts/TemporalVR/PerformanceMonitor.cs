using UnityEngine;
using UnityEngine.Profiling;
using System.Text;

namespace TemporalVR
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private bool showFPS = true;
        [SerializeField] private bool showMemory = true;
        [SerializeField] private bool showDrawCalls = true;
        [SerializeField] private bool showTemporalStats = true;

        [Header("Toggle Keys")]
        [SerializeField] private KeyCode toggleKey = KeyCode.P;
        [SerializeField] private KeyCode statsKey = KeyCode.F9;

        [Header("UI Settings")]
        [SerializeField] private int fontSize = 20;
        [SerializeField] private Color goodColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;

        [Header("VR Display")]
        [SerializeField] private bool useWorldSpaceUI = true;
        [SerializeField] private Transform vrDisplayTarget;
        [SerializeField] private float displayDistance = 2f;
        [SerializeField] private TextMesh worldTextDisplay;

        // FPS 계산
        private float deltaTime = 0.0f;
        private float fps = 0.0f;
        private int frameCount = 0;
        private float elapsedTime = 0.0f;

        // 성능 통계
        private float minFPS = float.MaxValue;
        private float maxFPS = 0f;
        private float avgFPS = 0f;

        // Temporal 객체 통계
        private int activeTemporalObjects = 0;
        private int totalKeyframes = 0;
        private int totalVertices = 0;

        // 메모리
        private long lastUsedMemory = 0;
        private long peakMemory = 0;

        private StringBuilder displayText = new StringBuilder();
        private bool isVisible = true;

        // 통계 업데이트 주기 조절
        private float statsUpdateInterval = 2f; // 1초에서 2초로 변경
        private float lastStatsUpdate = 0f;

        void Start()
        {
            if (useWorldSpaceUI && worldTextDisplay == null)
            {
                CreateWorldSpaceDisplay();
            }
        }

        void CreateWorldSpaceDisplay()
        {
            GameObject displayObj = new GameObject("Performance Display");
            displayObj.transform.SetParent(transform);
            displayObj.transform.localPosition = Vector3.forward * displayDistance;

            worldTextDisplay = displayObj.AddComponent<TextMesh>();
            worldTextDisplay.fontSize = 24;
            worldTextDisplay.anchor = TextAnchor.UpperLeft;
            worldTextDisplay.alignment = TextAlignment.Left;
            worldTextDisplay.characterSize = 0.05f;
            worldTextDisplay.color = Color.white;

            displayObj.AddComponent<Billboard>();
        }

        void Update()
        {
            HandleKeyInput();

            // FPS 계산
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            frameCount++;
            elapsedTime += Time.unscaledDeltaTime;

            if (elapsedTime >= 0.5f)
            {
                fps = frameCount / elapsedTime;
                frameCount = 0;
                elapsedTime = 0f;

                if (fps < minFPS) minFPS = fps;
                if (fps > maxFPS) maxFPS = fps;
                avgFPS = (avgFPS + fps) * 0.5f;
            }

            // 통계 업데이트 (주기적으로만)
            if (Time.time - lastStatsUpdate > statsUpdateInterval)
            {
                lastStatsUpdate = Time.time;
                if (showTemporalStats) // Temporal Stats가 켜져있을 때만 업데이트
                {
                    UpdateStats();
                }
            }

            // 디스플레이 업데이트
            if (isVisible)
            {
                UpdateDisplay();
            }
        }

        void HandleKeyInput()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isVisible = !isVisible;
                if (worldTextDisplay != null)
                    worldTextDisplay.gameObject.SetActive(isVisible);
            }

            if (Input.GetKeyDown(statsKey))
            {
                showTemporalStats = !showTemporalStats;
            }
        }

        void UpdateStats()
        {
            try
            {
                // Temporal 객체 수만 세기 (내부 데이터 접근 최소화)
                TMorphObj[] morphObjects = FindObjectsOfType<TMorphObj>();
                activeTemporalObjects = morphObjects.Length;

                // 메모리 사용량
                lastUsedMemory = Profiler.GetTotalAllocatedMemoryLong() / 1048576L;
                if (lastUsedMemory > peakMemory)
                    peakMemory = lastUsedMemory;

                // 상세 통계는 옵션으로
                if (morphObjects.Length > 0 && morphObjects.Length < 10) // 객체가 너무 많으면 스킵
                {
                    totalKeyframes = 0;
                    totalVertices = 0;

                    foreach (var obj in morphObjects)
                    {
                        if (obj == null) continue;

                        // null 체크 강화
                        if (obj.keyframes != null)
                        {
                            totalKeyframes += obj.keyframes.Count;
                        }

                        // MeshFilter 접근 시 주의
                        try
                        {
                            var meshFilter = obj.GetComponent<MeshFilter>();
                            if (meshFilter != null && meshFilter.mesh != null)
                            {
                                totalVertices += meshFilter.mesh.vertexCount;
                            }
                        }
                        catch
                        {
                            // 메시 접근 실패 시 무시
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PerformanceMonitor] Stats update error: {e.Message}");
            }
        }

        void UpdateDisplay()
        {
            displayText.Clear();

            // FPS 표시
            if (showFPS)
            {
                Color fpsColor = GetFPSColor(fps);
                displayText.AppendLine($"FPS: {fps:F1}");
                displayText.AppendLine($"Min: {minFPS:F1} | Max: {maxFPS:F1}");
                displayText.AppendLine();
            }

            // 메모리 표시
            if (showMemory)
            {
                displayText.AppendLine($"Memory: {lastUsedMemory} MB");
                displayText.AppendLine();
            }

            // Temporal 통계 (간소화)
            if (showTemporalStats)
            {
                displayText.AppendLine("=== Temporal ===");
                displayText.AppendLine($"Objects: {activeTemporalObjects}");

                if (activeTemporalObjects > 0 && activeTemporalObjects < 10)
                {
                    displayText.AppendLine($"Keyframes: {totalKeyframes}");
                    displayText.AppendLine($"Vertices: {totalVertices}");
                }
            }

            displayText.AppendLine();
            displayText.AppendLine("[P] Toggle | [F9] Stats");

            // World Space UI 업데이트
            if (useWorldSpaceUI && worldTextDisplay != null)
            {
                worldTextDisplay.text = displayText.ToString();
            }
        }

        Color GetFPSColor(float currentFPS)
        {
            if (currentFPS >= 90) return goodColor;
            if (currentFPS >= 60) return warningColor;
            return criticalColor;
        }

        void OnGUI()
        {
            if (useWorldSpaceUI || !isVisible) return;

            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(10, 10, 300, 200);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = fontSize;
            style.normal.textColor = Color.white;

            GUI.Box(rect, "Performance Monitor");
            GUI.Label(new Rect(15, 30, 280, 160), displayText.ToString(), style);
        }

        // 컴포넌트 비활성화 시 정리
        void OnDisable()
        {
            if (worldTextDisplay != null)
                worldTextDisplay.gameObject.SetActive(false);
        }
    }

    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                                cam.transform.rotation * Vector3.up);
            }
        }
    }
}