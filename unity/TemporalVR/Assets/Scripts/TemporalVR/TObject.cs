using UnityEngine;
using System.Collections;

namespace TemporalVR
{
    /// <summary>
    /// Visual Feedback을 포함한 향상된 Temporal Object
    /// </summary>
    public class TObject : TemporalObject
    {
        [Header("Visual Properties")]
        [SerializeField] private Gradient timeGradient;
        [SerializeField] private AnimationCurve growthCurve;
        [SerializeField] private bool showTimeEffect = true;

        private Renderer objectRenderer;  // Material 대신 Renderer 저장
        private Vector3 originalScale;
        private float lastAppliedTime = 0f;

        void Start()
        {
            // Renderer 참조 저장
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer == null)
            {
                Debug.LogError("[TObject] No Renderer found!");
                return;
            }

            // 머티리얼 인스턴스 생성 (중요!)
            if (objectRenderer.sharedMaterial != null)
            {
                objectRenderer.material = new Material(objectRenderer.sharedMaterial);
            }
            else
            {
                objectRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            originalScale = transform.localScale;

            // 기본 Gradient 설정
            if (timeGradient == null)
            {
                timeGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[3];
                colorKeys[0] = new GradientColorKey(Color.blue, 0f);
                colorKeys[1] = new GradientColorKey(Color.green, 0.5f);
                colorKeys[2] = new GradientColorKey(Color.red, 1f);

                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                timeGradient.SetKeys(colorKeys, alphaKeys);
            }

            // 기본 Growth Curve 설정
            if (growthCurve == null)
            {
                growthCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 2f);
            }

            // 테스트: 3초 후 자동 효과
            //StartCoroutine(AutoTest());
        }
        public void UpdateToTime(float time)
        {
            if (!showTimeEffect) return;

            float normalizedTime = time / 100f;

            // 색상 업데이트
            Color timeColor = timeGradient.Evaluate(normalizedTime);
            if (objectRenderer != null && objectRenderer.material != null)
            {
                objectRenderer.material.SetColor("_BaseColor", timeColor);
                objectRenderer.material.SetColor("_EmissionColor", timeColor * 0.5f);
            }

            // 크기 업데이트 (애니메이션 없이 즉시)
            float scale = growthCurve.Evaluate(normalizedTime);
            transform.localScale = originalScale * scale;

            Debug.Log($"[TObject] Updated to time: {time} (normalized: {normalizedTime})");
        }
        IEnumerator AutoTest()
        {
            yield return new WaitForSeconds(3f);
            Debug.Log("[TObject] Auto test starting!");
            ApplyTemporalBrush(transform.position, 1f, 50f);
        }

        public override void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            base.ApplyTemporalBrush(position, strength, time);

            Debug.Log($"[TObject] ApplyTemporalBrush called! Time: {time}, Strength: {strength}");

            if (!showTimeEffect) return;

            lastAppliedTime = time;

            // 시간에 따른 색상 변화
            float normalizedTime = time / 100f;
            Color timeColor = timeGradient.Evaluate(normalizedTime);

            if (objectRenderer != null && objectRenderer.material != null)
            {
                Debug.Log($"[TObject] Changing color to: {timeColor}");
                objectRenderer.material.SetColor("_BaseColor", timeColor);
                objectRenderer.material.EnableKeyword("_EMISSION");
                objectRenderer.material.SetColor("_EmissionColor", timeColor * strength);
            }
            else
            {
                Debug.LogError("[TObject] Renderer or material is null!");
            }

            // 크기 변화 애니메이션
            float targetScale = growthCurve.Evaluate(normalizedTime) * strength;
            StartCoroutine(AnimateScale(targetScale));

            // 충격 효과
            StartCoroutine(ImpactEffect(position, strength));
        }

        public override void SetTemporalRange(float startTime, float endTime)
        {
            base.SetTemporalRange(startTime, endTime);

            if (!showTimeEffect) return;

            // 시간 범위 시각화
            StartCoroutine(ShowTimeRangeEffect(startTime, endTime));
        }

        IEnumerator AnimateScale(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = originalScale * targetScale;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = endScale;
        }

        IEnumerator ImpactEffect(Vector3 impactPoint, float strength)
        {
            // 충격 지점에 임시 오브젝트 생성
            GameObject impactObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            impactObj.transform.position = impactPoint;
            impactObj.transform.localScale = Vector3.one * 0.05f;

            Renderer renderer = impactObj.GetComponent<Renderer>();
            // Unlit Shader 사용 (색상이 더 잘 보임)
            Material impactMat = new Material(Shader.Find("Sprites/Default"));
            if (impactMat == null)
            {
                impactMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }
            impactMat.EnableKeyword("_EMISSION");
            impactMat.SetColor("_EmissionColor", Color.white * 2f);
            renderer.material = impactMat;

            Destroy(impactObj.GetComponent<Collider>());

            // 확장 애니메이션
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0.05f, 0.2f * strength, t);
                float alpha = Mathf.Lerp(1f, 0f, t);

                impactObj.transform.localScale = Vector3.one * scale;
                impactMat.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(impactObj);
        }

        // ShowTimeRangeEffect에서도 material 대신 objectRenderer.material 사용
        IEnumerator ShowTimeRangeEffect(float startTime, float endTime)
        {
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float currentTime = Mathf.Lerp(startTime, endTime, Mathf.PingPong(t * 2f, 1f));

                float normalizedTime = currentTime / 100f;
                Color timeColor = timeGradient.Evaluate(normalizedTime);

                if (objectRenderer != null && objectRenderer.material != null)
                {
                    objectRenderer.material.SetColor("_BaseColor", timeColor);
                    objectRenderer.material.SetColor("_EmissionColor", timeColor * 0.5f);
                }

                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.1f;
                transform.localScale = originalScale * pulse;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}