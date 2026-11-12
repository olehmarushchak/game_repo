
using System.Collections;
using UnityEngine;

public class IllusionRock : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private float startingScale = 0.65f;

    private Renderer[] renderers;
    private Collider[] colliders;

    private Material[][] materials;
    private Color[][] originalColors;
    private bool[][] usesBaseColor;

    private Vector3 targetScale;
    private bool initialized;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        targetScale = transform.localScale;

        materials = new Material[renderers.Length][];
        originalColors = new Color[renderers.Length][];
        usesBaseColor = new bool[renderers.Length][];

        for (int r = 0; r < renderers.Length; r++)
        {
            // Create per-rock material instances
            materials[r] = renderers[r].materials;
            originalColors[r] = new Color[materials[r].Length];
            usesBaseColor[r] = new bool[materials[r].Length];

            for (int m = 0; m < materials[r].Length; m++)
            {
                Material material = materials[r][m];

                bool hasBaseColor = material.HasProperty("_BaseColor");
                bool hasColor = material.HasProperty("_Color");

                usesBaseColor[r][m] = hasBaseColor;

                if (hasBaseColor)
                    originalColors[r][m] = material.GetColor("_BaseColor");
                else if (hasColor)
                    originalColors[r][m] = material.GetColor("_Color");
                else
                    originalColors[r][m] = Color.white;
            }
        }

        SetColliders(false);
        SetAlpha(0f);

        transform.localScale = targetScale * startingScale;
    }

    public void Initialize(
        float fadeInDuration,
        float fadeOutDuration,
        float lifetime)
    {
        if (initialized)
            return;

        initialized = true;

        StartCoroutine(PlayLifecycle(
            fadeInDuration,
            fadeOutDuration,
            lifetime
        ));
    }

    private IEnumerator PlayLifecycle(
        float fadeInDuration,
        float fadeOutDuration,
        float lifetime)
    {
        // Materialize
        yield return Animate(
            0f, 1f,
            startingScale, 1f,
            fadeInDuration
        );

        SetColliders(true);

        // Stay solid until it is time to disappear
        float solidDuration = Mathf.Max(
            0f,
            lifetime - fadeInDuration - fadeOutDuration
        );

        yield return new WaitForSeconds(solidDuration);

        // Become an illusion again
        SetColliders(false);

        yield return Animate(
            1f, 0f,
            1f, startingScale,
            fadeOutDuration
        );

        Destroy(gameObject);
    }

    private IEnumerator Animate(
        float startAlpha,
        float endAlpha,
        float startScale,
        float endScale,
        float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(endAlpha);
            transform.localScale = targetScale * endScale;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth start and finish
            float smoothT = t * t * (3f - 2f * t);

            SetAlpha(Mathf.Lerp(startAlpha, endAlpha, smoothT));

            transform.localScale = targetScale *
                Mathf.Lerp(startScale, endScale, smoothT);

            yield return null;
        }

        SetAlpha(endAlpha);
        transform.localScale = targetScale * endScale;
    }

    private void SetAlpha(float alpha)
    {
        for (int r = 0; r < materials.Length; r++)
        {
            for (int m = 0; m < materials[r].Length; m++)
            {
                Material material = materials[r][m];
                Color color = originalColors[r][m];
                color.a *= alpha;

                if (usesBaseColor[r][m])
                    material.SetColor("_BaseColor", color);
                else if (material.HasProperty("_Color"))
                    material.SetColor("_Color", color);
            }
        }
    }

    private void SetColliders(bool enabledState)
    {
        foreach (Collider obstacleCollider in colliders)
        {
            if (obstacleCollider != null)
                obstacleCollider.enabled = enabledState;
        }
    }
}