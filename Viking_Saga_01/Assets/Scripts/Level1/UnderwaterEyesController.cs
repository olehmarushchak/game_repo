
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UnderwaterEyesController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject eyePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform approachTarget;

    [Header("Minigame")]
    [SerializeField] private EyesTimingMinigame minigame;

    [Header("Stage Settings")]
    [SerializeField] private float stageDuration = 5f;
    [SerializeField] private float minigameDelay = 2f;

    [SerializeField, Range(1, 3)]
    private int minigameStartStage = 3;

    [Header("Eyes Count")]
    [SerializeField] private int stage1Eyes = 1;
    [SerializeField] private int stage2Eyes = 3;
    [SerializeField] private int stage3Eyes = 5;

    [Header("Audio")]
    [SerializeField] private AudioSource whisperAudio;
    [SerializeField] private AudioLowPassFilter whisperFilter;

    [SerializeField] private float maxWhisperVolume = 0.8f;
    [SerializeField] private float audioFadeSpeed = 0.4f;

    [Header("Audio Stage Values")]
    [SerializeField] private float stage1Pitch = 0.8f;
    [SerializeField] private float stage2Pitch = 1f;
    [SerializeField] private float stage3Pitch = 1.2f;

    [Header("Visual Feedback")]
    [SerializeField] private CanvasGroup distortionOverlay;
    [SerializeField] private float distortionFadeSpeed = 1f;

    [Header("Recovery")]
    [SerializeField] private float eyesFadeDuration = 2f;

    public event Action EncounterSucceeded;

    private readonly List<UnderwaterEye> activeEyes =
        new List<UnderwaterEye>();

    private readonly List<int> usedSpawnPoints =
        new List<int>();

    private bool encounterActive;
    private bool minigameStarted;

    private int currentStage = 1;

    private float encounterTimer;
    private float currentAudioTarget;
    private float currentDistortionTarget;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (whisperAudio != null)
        {
            whisperAudio.loop = true;
            whisperAudio.volume = 0f;
            whisperAudio.Stop();
        }

        if (distortionOverlay != null)
        {
            distortionOverlay.alpha = 0f;
        }
    }

    private void Update()
    {
        if (!encounterActive)
            return;

        encounterTimer += Time.deltaTime;

        UpdateAudio();
        UpdateDistortion();

        UpdateStages();
    }

    // --------------------------------------------------
    // START ENCOUNTER
    // --------------------------------------------------

    public void StartEncounter()
    {
        Debug.Log("Start encounter called.");

        if (encounterActive)
            return;

        if (eyePrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "Underwater Eyes: Missing eye prefab or spawn points!",
                this
            );

            return;
        }

        if (minigame == null)
        {
            Debug.LogError(
                "Underwater Eyes: Minigame is not assigned!",
                this
            );

            return;
        }

        encounterActive = true;
        minigameStarted = false;

        encounterTimer = 0f;
        currentStage = 1;

        usedSpawnPoints.Clear();

        SpawnEyes(stage1Eyes);

        SetStageEffects(1);

        if (whisperAudio != null)
        {
            whisperAudio.Play();
        }

        Debug.Log("Underwater Eyes: Stage 1 started.");
    }

    // --------------------------------------------------
    // STAGE PROGRESSION
    // --------------------------------------------------

    private void UpdateStages()
    {
        if (minigameStarted)
            return;
            
        if (currentStage == 1 &&
            encounterTimer >= stageDuration)
        {
            ChangeStage(2);
        }

        if (currentStage == 2 &&
            encounterTimer >= stageDuration * 2f)
        {
            ChangeStage(3);
        }

        if (!minigameStarted &&
            currentStage >= minigameStartStage &&
            encounterTimer >=
            stageDuration * (minigameStartStage - 1) +
            minigameDelay)
        {
            StartTimingMinigame();
        }
    }

    private void ChangeStage(int newStage)
    {
        currentStage = newStage;

        int targetEyeCount = GetEyeCount(currentStage);

        SpawnEyes(targetEyeCount);

        SetStageEffects(currentStage);

        Debug.Log(
            "Underwater Eyes: Stage " +
            currentStage + " started."
        );
    }

    private int GetEyeCount(int stage)
    {
        switch (stage)
        {
            case 1:
                return stage1Eyes;

            case 2:
                return stage2Eyes;

            case 3:
                return stage3Eyes;

            default:
                return stage1Eyes;
        }
    }

    // --------------------------------------------------
    // SPAWNING EYES
    // --------------------------------------------------

    private void SpawnEyes(int targetCount)
    {
        targetCount = Mathf.Min(
            targetCount,
            spawnPoints.Length
        );

        while (activeEyes.Count < targetCount)
        {
            int spawnIndex = GetRandomSpawnPoint();

            if (spawnIndex == -1)
                return;

            GameObject eyeObject = Instantiate(
                eyePrefab,
                spawnPoints[spawnIndex].position,
                spawnPoints[spawnIndex].rotation
            );

            UnderwaterEye eye =
                eyeObject.GetComponent<UnderwaterEye>();

            if (eye == null)
            {
                Debug.LogError(
                    "Eye prefab requires UnderwaterEye component!"
                );

                Destroy(eyeObject);
                continue;
            }

            eye.Initialize(
                approachTarget,
                GetStageIntensity(currentStage)
            );

            activeEyes.Add(eye);
        }
    }

    private int GetRandomSpawnPoint()
    {
        List<int> available = new List<int>();

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null &&
                !usedSpawnPoints.Contains(i))
            {
                available.Add(i);
            }
        }

        if (available.Count == 0)
            return -1;

        int selected = available[
            UnityEngine.Random.Range(0, available.Count)
        ];

        usedSpawnPoints.Add(selected);

        return selected;
    }

    private float GetStageIntensity(int stage)
    {
        switch (stage)
        {
            case 1:
                return 0.25f;

            case 2:
                return 0.6f;

            case 3:
                return 1f;

            default:
                return 0.25f;
        }
    }

    // --------------------------------------------------
    // STAGE EFFECTS
    // --------------------------------------------------

    private void SetStageEffects(int stage)
    {
        float intensity = GetStageIntensity(stage);

        currentAudioTarget = maxWhisperVolume * intensity;
        currentDistortionTarget = intensity * 0.65f;

        if (whisperAudio != null)
        {
            switch (stage)
            {
                case 1:
                    whisperAudio.pitch = stage1Pitch;
                    break;

                case 2:
                    whisperAudio.pitch = stage2Pitch;
                    break;

                case 3:
                    whisperAudio.pitch = stage3Pitch;
                    break;
            }
        }

        if (whisperFilter != null)
        {
            switch (stage)
            {
                case 1:
                    whisperFilter.cutoffFrequency = 5000f;
                    break;

                case 2:
                    whisperFilter.cutoffFrequency = 2500f;
                    break;

                case 3:
                    whisperFilter.cutoffFrequency = 1000f;
                    break;
            }
        }

        foreach (UnderwaterEye eye in activeEyes)
        {
            if (eye != null)
            {
                eye.SetIntensity(intensity);
            }
        }
    }

    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void UpdateAudio()
    {
        if (whisperAudio == null)
            return;

        whisperAudio.volume = Mathf.MoveTowards(
            whisperAudio.volume,
            currentAudioTarget,
            audioFadeSpeed * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // DISTORTION
    // --------------------------------------------------

    private void UpdateDistortion()
    {
        if (distortionOverlay == null)
            return;

        distortionOverlay.alpha = Mathf.MoveTowards(
            distortionOverlay.alpha,
            currentDistortionTarget,
            distortionFadeSpeed * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // MINIGAME
    // --------------------------------------------------

    private void StartTimingMinigame()
    {
        if (minigameStarted)
            return;

        minigameStarted = true;

        minigame.StartMinigame(currentStage);

        Debug.Log(
            "Eyes minigame started. Required hits: " +
            currentStage
        );
    }

    // --------------------------------------------------
    // SUCCESS
    // --------------------------------------------------

    private void OnMinigameCompleted()
    {
        if (!encounterActive)
            return;

        Debug.Log("Underwater Eyes defeated!");

        encounterActive = false;

        currentAudioTarget = 0f;
        currentDistortionTarget = 0f;

        if (whisperAudio != null)
        {
            StartCoroutine(FadeWhisperAudio());
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAllEyes());

        EncounterSucceeded?.Invoke();
    }

    private IEnumerator FadeWhisperAudio()
    {
        while (whisperAudio != null &&
               whisperAudio.volume > 0.01f)
        {
            whisperAudio.volume = Mathf.MoveTowards(
                whisperAudio.volume,
                0f,
                audioFadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (whisperAudio != null)
        {
            whisperAudio.volume = 0f;
            whisperAudio.Stop();
        }
    }

    private IEnumerator FadeAllEyes()
    {
        foreach (UnderwaterEye eye in activeEyes)
        {
            if (eye != null)
            {
                StartCoroutine(
                    eye.FadeOut(eyesFadeDuration)
                );
            }
        }

        yield return new WaitForSeconds(eyesFadeDuration);

        activeEyes.Clear();
        usedSpawnPoints.Clear();
    }

    // --------------------------------------------------
    // CANCEL
    // --------------------------------------------------

    public void CancelEncounter()
    {
        encounterActive = false;

        if (minigame != null)
        {
            minigame.CancelMinigame();
        }

        if (whisperAudio != null)
        {
            whisperAudio.Stop();
            whisperAudio.volume = 0f;
        }

        currentAudioTarget = 0f;
        currentDistortionTarget = 0f;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        foreach (UnderwaterEye eye in activeEyes)
        {
            if (eye != null)
            {
                Destroy(eye.gameObject);
            }
        }

        activeEyes.Clear();
        usedSpawnPoints.Clear();

        if (distortionOverlay != null)
        {
            distortionOverlay.alpha = 0f;
        }

        minigameStarted = false;
        encounterTimer = 0f;
        currentStage = 1;
    }

    private void OnEnable()
    {
        if (minigame != null)
        {
            minigame.OnMinigameCompleted +=
                OnMinigameCompleted;
        }
    }

    private void OnDisable()
    {
        if (minigame != null)
        {
            minigame.OnMinigameCompleted -=
                OnMinigameCompleted;
        }
    }
}
