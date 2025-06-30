using UnityEngine;
using TMPro;

namespace TemporalVR
{
    /// <summary>
    /// TemporalVRController의 Visual Feedback 오브젝트를 자동으로 생성하고 설정
    /// </summary>
    [RequireComponent(typeof(TemporalVRController))]
    public class TVRSetup : MonoBehaviour
    {
        private TemporalVRController controller;

        [Header("Visual Preferences")]
        [SerializeField] private float cursorSize = 0.1f;
        [SerializeField] private Color pastColor = new Color(0.1f, 0.1f, 0.8f);
        [SerializeField] private Color futureColor = new Color(0.8f, 0.1f, 0.1f);

        [ContextMenu("Setup All Visual Feedback")]
        public void SetupAllVisualFeedback()
        {
            controller = GetComponent<TemporalVRController>();

            // Timeline Visualizer는 이미 코드에서 자동 생성됨

            // Temporal Cursor 생성
            SetupTemporalCursor();

            // Time Display 생성
            SetupTimeDisplay();

            // Event System 생성
            SetupEventSystem();

            Debug.Log("[TemporalVR] Visual Feedback setup complete!");
        }

        private void SetupTemporalCursor()
        {
            // 기존 커서 찾기 또는 생성
            GameObject cursorObj = GameObject.Find("TemporalCursor");
            if (cursorObj == null)
            {
                cursorObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                cursorObj.name = "TemporalCursor";
            }

            // 크기 설정
            cursorObj.transform.localScale = Vector3.one * cursorSize;

            // Material 설정 (URP)
            Renderer renderer = cursorObj.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_BaseColor", pastColor);
            mat.SetColor("_EmissionColor", pastColor * 1.5f);
            renderer.material = mat;

            // Collider 제거 (커서가 레이캐스트를 방해하지 않도록)
            Collider col = cursorObj.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);

            // Controller에 할당
            var serializedObject = new UnityEditor.SerializedObject(controller);
            serializedObject.FindProperty("temporalCursor").objectReferenceValue = cursorObj.transform;
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupTimeDisplay()
        {
            // Time Display 오브젝트 생성
            GameObject displayObj = GameObject.Find("TimeDisplay");
            if (displayObj == null)
            {
                displayObj = new GameObject("TimeDisplay");
            }

            // TextMesh 또는 TextMeshPro 설정
            TextMesh textMesh = displayObj.GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = displayObj.AddComponent<TextMesh>();
            }

            // 텍스트 설정
            textMesh.text = "Time: 0.0\nMode: Scrub";
            textMesh.fontSize = 24;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.white;

            // 위치 설정 (컨트롤러 위)
            displayObj.transform.position = transform.position + Vector3.up * 0.3f;
            displayObj.transform.SetParent(transform);

            // Controller에 할당
            var serializedObject = new UnityEditor.SerializedObject(controller);
            serializedObject.FindProperty("timeDisplay").objectReferenceValue = textMesh;
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupEventSystem()
        {
            // TemporalEventSystem 찾기 또는 생성
            if (TemporalEventSystem.Instance == null)
            {
                GameObject eventSystemObj = new GameObject("TemporalEventSystem");
                eventSystemObj.AddComponent<TemporalEventSystem>();
            }
        }
    }
}