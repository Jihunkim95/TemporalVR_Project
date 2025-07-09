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
        [SerializeField] private bool showTemporalStats = false; // 기본값 false로 변경

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
        private float statsUpdateInterval = 5f; // 2초에서 5초로 증가
        private float lastStatsUpdate = 0f;

        // 안전한 업데이트를 위한 플래그
        private bool isUpdatingStats = false;

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

            // 통계 업데이트 (주기적으로만, 안전하게)
            if (showTemporalStats && !isUpdatingStats && Time.time - lastStatsUpdate > statsUpdateInterval)
            {
                lastStatsUpdate = Time.time;
                SafeUpdateStats();
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
                Debug.Log($"[PerformanceMonitor] Temporal Stats: {(showTemporalStats ? "ON" : "OFF")}");
            }
        }

        void SafeUpdateStats()
        {
            // 업데이트 중 플래그 설정
            isUpdatingStats = true;

            try
            {
                // 메모리 사용량 (항상 안전)
                lastUsedMemory = Profiler.GetTotalAllocatedMemoryLong() / 1048576L;
                if (lastUsedMemory > peakMemory)
                    peakMemory = lastUsedMemory;

                // Temporal 객체 통계 (안전하게)
                UpdateTemporalStats();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PerformanceMonitor] Stats update error: {e.Message}");
                // 에러 발생 시 통계 기능 자동 비활성화
                showTemporalStats = false;
            }
            finally
            {
                isUpdatingStats = false;
            }
        }

        void UpdateTemporalStats()
        {
            // TMorphObj 찾기 (한 번만)
            TMorphObj[] morphObjects = FindObjectsOfType<TMorphObj>();
            activeTemporalObjects = morphObjects.Length;

            // 객체가 너무 많으면 상세 통계 스킵
            if (morphObjects.Length > 20)
            {
                totalKeyframes = -1; // -1은 "too many"를 의미
                totalVertices = -1;
                return;
            }

            totalKeyframes = 0;
            totalVertices = 0;

            foreach (var obj in morphObjects)
            {
                // Null 체크
                if (obj == null) continue;

                // 안전한 keyframes 접근
                try
                {
                    if (obj.keyframes != null)
                    {
                        totalKeyframes += obj.keyframes.Count;
                    }
                }
                catch
                {
                    // keyframes 접근 실패 시 무시
                }

                // 안전한 mesh 접근 (GetComponent 사용 지양)
                try
                {
                    // TMorphObj가 이미 가지고 있는 public 메서드가 있다면 사용
                    // 없다면 이 부분은 스킵
                    var meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        // sharedMesh 사용 (workingMesh 대신)
                        totalVertices += meshFilter.sharedMesh.vertexCount;
                    }
                }
                catch
                {
                    // 메시 접근 실패 시 무시
                }
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
            if (showTemporalStats && activeTemporalObjects > 0)
            {
                displayText.AppendLine("=== Temporal ===");
                displayText.AppendLine($"Objects: {activeTemporalObjects}");

                if (totalKeyframes >= 0) // -1이 아닌 경우만 표시
                {
                    displayText.AppendLine($"Keyframes: {totalKeyframes}");
                    displayText.AppendLine($"Vertices: {totalVertices}");
                }
                else
                {
                    displayText.AppendLine("(Too many objects)");
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
            isUpdatingStats = false; // 플래그 초기화

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