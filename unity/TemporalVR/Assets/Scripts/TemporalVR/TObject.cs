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

        private Material material;
        private Vector3 originalScale;
        private float lastAppliedTime = 0f;

        void Start()
        {
            // Material 가져오기 또는 생성
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                material = renderer.material;
                if (material == null)
                {
                    material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    renderer.material = material;
                }
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
        }

        public new void ApplyTemporalBrush(Vector3 position, float strength, float time)
        {
            base.ApplyTemporalBrush(position, strength, time);

            if (!showTimeEffect) return;

            lastAppliedTime = time;

            // 시간에 따른 색상 변화
            float normalizedTime = time / 100f;
            Color timeColor = timeGradient.Evaluate(normalizedTime);

            if (material != null)
            {
                material.SetColor("_BaseColor", timeColor);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", timeColor * strength);
            }

            // 크기 변화 애니메이션
            float targetScale = growthCurve.Evaluate(normalizedTime) * strength;
            StartCoroutine(AnimateScale(targetScale));

            // 충격 효과
            StartCoroutine(ImpactEffect(position, strength));
        }

        public new void SetTemporalRange(float startTime, float endTime)
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
            Material impactMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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

        IEnumerator ShowTimeRangeEffect(float startTime, float endTime)
        {
            float duration = 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float currentTime = Mathf.Lerp(startTime, endTime, Mathf.PingPong(t * 2f, 1f));

                // 시간에 따른 색상 애니메이션
                float normalizedTime = currentTime / 100f;
                Color timeColor = timeGradient.Evaluate(normalizedTime);

                if (material != null)
                {
                    material.SetColor("_BaseColor", timeColor);
                    material.SetColor("_EmissionColor", timeColor * 0.5f);
                }

                // 펄스 효과
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.1f;
                transform.localScale = originalScale * pulse;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 원래 상태로 복구
            transform.localScale = originalScale;
        }
    }
}