using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DangerDebugHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image dangerOverlay;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text dangerText;

    [Header("Danger Settings")]
    [SerializeField] private float dangerThreshold = 0.5f;
    [SerializeField] private float maxAlpha = 0.2f;
    [SerializeField] private float fadeSpeed = 3f;

    [Header("Debug Values")]
    [Range(0f, 1f)]
    [SerializeField] private float speed01;

    [Range(0f, 1f)]
    [SerializeField] private float levelOfDanger;

    private void Start()
    {
        if (dangerOverlay == null)
        {
            Debug.LogError("Danger overlay image is not assigned.");
        }

        Color color = dangerOverlay.color;
        color.a = Mathf.MoveTowards(
            color.a,
            0,
            1
        );

        dangerOverlay.color = color;
    }

    private void Update()
    {
        // Display values
        // speedText.text = $"Speed: {speed01:F2}";
        // dangerText.text = $"Danger: {levelOfDanger:F2}";

        // Calculate overlay transparency
        float danger01 = Mathf.InverseLerp(
            dangerThreshold,
            0f,
            levelOfDanger
        );

        float targetAlpha = danger01 * maxAlpha;

        Color color = dangerOverlay.color;
        color.a = Mathf.MoveTowards(
            color.a,
            targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        dangerOverlay.color = color;
    }

    public void UpdateDebugValues(float speed, float danger)
    {
        speed01 = speed;
        levelOfDanger = danger;
    }
}