
using UnityEngine;
using System.Collections;

public class UnderwaterEye : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float approachSpeed = 0.5f;
    [SerializeField] private float maxApproachDistance = 5f;

    [Header("Tremor")]
    [SerializeField] private float tremorAmount = 0.05f;
    [SerializeField] private float tremorFrequency = 12f;

    [Header("Scale")]
    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float pulseAmount = 0.05f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Fade")]
    [SerializeField] private Renderer[] eyeRenderers;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 movementPosition;
    private Transform approachTarget;

    private float intensity = 1f;
    private float elapsedTime;
    private float fadeAlpha = 1f;

    private MaterialPropertyBlock propertyBlock;

    private static readonly int BaseColor =
        Shader.PropertyToID("_BaseColor");

    private static readonly int ColorProperty =
        Shader.PropertyToID("_Color");

    private void Awake()
    {
        movementPosition = transform.position;
        initialPosition = transform.position;
        initialScale = transform.localScale;

        propertyBlock = new MaterialPropertyBlock();
    }

    public void Initialize(
        Transform target,
        float stageIntensity
    )
    {
        movementPosition = transform.position;
        approachTarget = target;
        intensity = Mathf.Clamp01(stageIntensity);

        initialPosition = transform.position;
        initialScale = transform.localScale;
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        UpdateMovement();
        UpdateTremor();
        UpdateScale();
        UpdateFade();
    }

    private void UpdateMovement()
    {
        if (approachTarget == null)
            return;

        Vector3 targetPosition =
            approachTarget.position -
            approachTarget.forward * maxApproachDistance;

        movementPosition = Vector3.MoveTowards(
            movementPosition,
            targetPosition,
            approachSpeed * (0.5f + intensity) * Time.deltaTime
        );
    }

    private void UpdateTremor()
    {
        float tremor = tremorAmount * intensity;

        float x = Mathf.Sin(elapsedTime * tremorFrequency) * tremor;
        float y = Mathf.Cos(elapsedTime * tremorFrequency * 1.3f) * tremor;

        transform.position = movementPosition + new Vector3(x, y, 0f);
    }

    private void UpdateScale()
    {
        float pulse = 1f +
            Mathf.Sin(elapsedTime * pulseSpeed) *
            pulseAmount * intensity;

        float targetScale = baseScale * (1f + intensity * 0.35f);

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            initialScale * targetScale * pulse,
            Time.deltaTime * 2f
        );
    }

    public IEnumerator FadeOut(float duration)
    {
        float startAlpha = fadeAlpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            fadeAlpha = Mathf.Lerp(
                startAlpha,
                0f,
                timer / duration
            );

            ApplyAlpha();

            yield return null;
        }

        fadeAlpha = 0f;
        ApplyAlpha();

        Destroy(gameObject);
    }

    private void UpdateFade()
    {
        ApplyAlpha();
    }

    private void ApplyAlpha()
    {
        if (eyeRenderers == null)
            return;

        foreach (Renderer rend in eyeRenderers)
        {
            if (rend == null)
                continue;

            foreach (Material material in rend.sharedMaterials)
            {
                if (material == null)
                    continue;

                Color color;

                if (material.HasProperty(BaseColor))
                    color = material.GetColor(BaseColor);
                else if (material.HasProperty(ColorProperty))
                    color = material.GetColor(ColorProperty);
                else
                    continue;

                rend.GetPropertyBlock(propertyBlock);

                color.a = fadeAlpha;

                if (material.HasProperty(BaseColor))
                    propertyBlock.SetColor(BaseColor, color);
                else
                    propertyBlock.SetColor(ColorProperty, color);

                rend.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
