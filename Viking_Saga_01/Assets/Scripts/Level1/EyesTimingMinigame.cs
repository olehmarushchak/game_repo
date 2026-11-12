
using UnityEngine;
using UnityEngine.UI;
using System;

public class EyesTimingMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup minigamePanel;
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform targetZone;

    [Header("Timing")]
    [SerializeField] private float markerSpeed = 450f;
    [SerializeField] private float minX = -300f;
    [SerializeField] private float maxX = 300f;

    [Header("Feedback")]
    [SerializeField] private Image progressFill;

    public event Action OnMinigameCompleted;

    private bool isActive;
    private bool movingRight;

    private int requiredHits;
    private int currentHits;

    private float currentX;

    private void Start()
    {
        if (minigamePanel != null)
        {
            Debug.Log("Setting minigame panel to invisible at start.");

            minigamePanel.alpha = 0f;
            minigamePanel.interactable = false;
            minigamePanel.blocksRaycasts = false;
        }

        // gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive)
            return;

        MoveMarker();

        if (Input.GetKeyDown(KeyCode.F))
        {
            CheckTiming();
        }
    }

    public void StartMinigame(int hitsRequired)
    {
        requiredHits = Mathf.Max(1, hitsRequired);
        currentHits = 0;

        currentX = minX;
        movingRight = true;

        marker.anchoredPosition = new Vector2(
            currentX,
            marker.anchoredPosition.y
        );

        UpdateProgress();

        gameObject.SetActive(true);

        if (minigamePanel != null)
        {
            minigamePanel.alpha = 1f;
            minigamePanel.interactable = true;
            minigamePanel.blocksRaycasts = true;
        }

        isActive = true;
    }

    private void MoveMarker()
    {
        float direction = movingRight ? 1f : -1f;

        currentX += direction * markerSpeed * Time.deltaTime;

        if (currentX >= maxX)
        {
            currentX = maxX;
            movingRight = false;
        }
        else if (currentX <= minX)
        {
            currentX = minX;
            movingRight = true;
        }

        marker.anchoredPosition = new Vector2(
            currentX,
            marker.anchoredPosition.y
        );
    }

    private void CheckTiming()
    {
        float markerX = marker.anchoredPosition.x;

        float targetMin = targetZone.anchoredPosition.x -
                          targetZone.rect.width * 0.5f;

        float targetMax = targetZone.anchoredPosition.x +
                          targetZone.rect.width * 0.5f;

        if (markerX >= targetMin && markerX <= targetMax)
        {
            currentHits++;

            UpdateProgress();

            Debug.Log(
                "Eyes minigame hit: " +
                currentHits + "/" + requiredHits
            );

            if (currentHits >= requiredHits)
            {
                CompleteMinigame();
            }
        }
        else
        {
            Debug.Log("Missed timing!");
        }
    }

    private void UpdateProgress()
    {
        if (progressFill != null)
        {
            progressFill.fillAmount =
                (float)currentHits / requiredHits;
        }
    }

    private void CompleteMinigame()
    {
        isActive = false;

        if (minigamePanel != null)
        {
            minigamePanel.alpha = 0f;
            minigamePanel.interactable = false;
            minigamePanel.blocksRaycasts = false;
        }

        // gameObject.SetActive(false);

        OnMinigameCompleted?.Invoke();

        Debug.Log("Eyes minigame completed!");
    }

    public void CancelMinigame()
    {
        isActive = false;

        if (minigamePanel != null)
        {
            minigamePanel.alpha = 0f;
            minigamePanel.interactable = false;
            minigamePanel.blocksRaycasts = false;
        }

        // gameObject.SetActive(false);
    }
}
