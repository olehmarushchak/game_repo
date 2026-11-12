
using UnityEngine;
using BKPureNature;
using System.Collections;

public class LevelOneController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource dangerAudio;
    [SerializeField] private AudioSource oceanAmbientAudio;

    [Header("Level Systems")]
    [SerializeField] private PureVolumetricFog levelFog;
    [SerializeField] private ShipMovement shipMovement;
    [SerializeField] private DangerDebugHUD debugHUD;
    [SerializeField] private DredgeCameraFollow cameraController;
    [SerializeField] private IllusionObstacleSpawner obstacleSpawner;

    [Header("Underwater Eyes")]
    [SerializeField] private UnderwaterEyesController eyesController;
    [SerializeField] private float eyesAppearDelay = 2f;

    [Header("Level of Danger")]
    [SerializeField, Range(0f, 1f)]
    private float levelOfDanger = 0f;

    [SerializeField] private float thresholdDanger = 0.95f;
    [SerializeField] private float thresholdDanger2 = 0.75f;

    [Header("Danger Recovery")]
    [SerializeField] private float recoveryDangerLevel = 0.25f;
    [SerializeField] private float recoveryDuration = 8f;

    private bool playerInside;
    private bool collisionPending;
    private bool eyesEncounterActive;
    private bool recoveryActive;

    private float eyesDelayTimer;
    private float recoveryTimer;

    private float oldCameraLookHeight;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 0.1f;
    [SerializeField] private float fadeSpeedDistance = 20f;

    [Header("Fog")]
    [SerializeField] private float normalFogDensity = 0.2f;
    [SerializeField] private float dangerFogDensity = 0.5f;

    [SerializeField] private float normalFogDistance = 400f;
    [SerializeField] private float dangerFogDistance = 100f;

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";

    private void OnEnable()
    {
        if (eyesController != null)
        {
            eyesController.EncounterSucceeded += OnEyesEncounterSucceeded;
                    Debug.Log("ENABLE EYES ENCOUNTER");
        }


    }

    private void OnDisable()
    {
        if (eyesController != null)
        {
            eyesController.EncounterSucceeded -= OnEyesEncounterSucceeded;
        }
    }

    private void Start()
    {
        if (dangerAudio != null)
        {
            dangerAudio.loop = true;
            dangerAudio.volume = 0f;
            dangerAudio.Play();
        }

        if (cameraController != null)
        {
            oldCameraLookHeight = cameraController.height;
        }
    }

    private void UpdateObstacleDanger()
    {
        if (obstacleSpawner == null)
            return;

        float obstacleDanger = 1f - levelOfDanger;

        obstacleSpawner.SetDangerLevel(obstacleDanger);
    }
    private void Update()
    {
        if (!playerInside)
            return;

        // 1. Danger calculation / Recovery
        if (recoveryActive)
        {
            UpdateRecovery();
        }
        else
        {
            DecideDangerLevel();
        }

        // 2. Danger values
        DangerController();
        UpdateObstacleDanger();

        // 3. Audio
        AudioController();

        // 4. Fog
        if (levelFog != null)
        {
            FogController(dangerFogDensity, dangerFogDistance);
        }

        // 5. Camera
        CameraController();

        // 6. Underwater Eyes
        // UpdateEyesEncounter();
        StartEyesEncounter();


        // 7. Debug
        // Debug.Log("Speed: " + shipMovement.Speed01);
        // Debug.Log("Danger: " + levelOfDanger);
    }

    // --------------------------------------------------
    // DANGER SYSTEM
    // --------------------------------------------------

    private void DecideDangerLevel()
    {
        if (collisionPending)
        {
            levelOfDanger = 1f;
            return;
        }

        if (shipMovement == null)
            return;

        if (shipMovement.Speed01 > thresholdDanger)
        {
            levelOfDanger = 1f;
        }
        else if (shipMovement.Speed01 > thresholdDanger2)
        {
            levelOfDanger = 0.5f;
        }
        else
        {
            levelOfDanger = 0f;
        }
    }

    private void DangerController()
    {
        if (levelOfDanger >= 1f)
        {
            dangerFogDensity = 0.15f;
            dangerFogDistance = 140f;
        }
        else if (levelOfDanger >= 0.5f)
        {
            dangerFogDensity = 0.25f;
            dangerFogDistance = 70f;
        }
        else
        {
            dangerFogDensity = 0.35f;
            dangerFogDistance = 50f;
        }
    }

    // --------------------------------------------------
    // UNDERWATER EYES SYSTEM
    // --------------------------------------------------

    private void UpdateEyesEncounter()
    {
        if (recoveryActive || eyesEncounterActive)
            return;

        if (!collisionPending)
        {
            eyesDelayTimer = 0f;
            return;
        }

        // Wait until the critical danger state is reached.
        if (levelOfDanger < 1f)
            return;

        eyesDelayTimer += Time.deltaTime;

        if (eyesDelayTimer >= eyesAppearDelay)
        {
            StartEyesEncounter();
        }
    }

    private void StartEyesEncounter()
    {
        if (eyesController == null)
            return;

        eyesEncounterActive = true;

        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetSpawningEnabled(false);
        }

        eyesController.StartEncounter();
    }

    // Called when the minigame is successfully completed.
    private void OnEyesEncounterSucceeded()
    {
        eyesEncounterActive = false;
        collisionPending = false;

        if (obstacleSpawner != null)
        {
            obstacleSpawner.SetSpawningEnabled(true);
        }

        StartRecovery();
    }

    // --------------------------------------------------
    // RECOVERY SYSTEM
    // --------------------------------------------------

    private void StartRecovery()
    {
        recoveryActive = true;
        recoveryTimer = recoveryDuration;

        levelOfDanger = recoveryDangerLevel;
    }

    private void UpdateRecovery()
    {
        recoveryTimer -= Time.deltaTime;

        // Keep danger low during recovery.
        levelOfDanger = recoveryDangerLevel;

        if (recoveryTimer <= 0f)
        {
            recoveryActive = false;
            levelOfDanger = 0f;

            Debug.Log("Recovery finished. Danger system restored.");
        }
    }

    // --------------------------------------------------
    // COLLISION EVENT
    // --------------------------------------------------

    public void NotifyObstacleCollision()
    {
        if (!playerInside || recoveryActive || eyesEncounterActive)
            return;

        collisionPending = true;
        eyesDelayTimer = 0f;

        levelOfDanger = 1f;

        Debug.Log("Obstacle collision! Critical danger activated.");
    }

    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void AudioController()
    {
        if (dangerAudio != null)
        {
            float targetVolume = 0.7f;

            dangerAudio.volume = Mathf.MoveTowards(
                dangerAudio.volume,
                targetVolume,
                fadeSpeed * Time.deltaTime
            );
        }

        if (oceanAmbientAudio != null)
        {
            oceanAmbientAudio.volume = Mathf.MoveTowards(
                oceanAmbientAudio.volume,
                0.15f,
                fadeSpeed * Time.deltaTime
            );
        }
    }

    // --------------------------------------------------
    // FOG
    // --------------------------------------------------

    private void FogController(float targetDensity, float targetDistance)
    {
        if (levelFog == null)
            return;

        levelFog.density = Mathf.MoveTowards(
            levelFog.density,
            targetDensity,
            fadeSpeed * Time.deltaTime
        );

        levelFog.nearFadeDistance = Mathf.MoveTowards(
            levelFog.nearFadeDistance,
            targetDistance,
            fadeSpeedDistance * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // CAMERA
    // --------------------------------------------------

    private void CameraController()
    {
        if (cameraController == null)
            return;

        cameraController.SetDangerLevel(
            levelOfDanger,
            playerInside
        );

        float targetHeight = recoveryActive
            ? oldCameraLookHeight
            : 15f;

        cameraController.height = Mathf.MoveTowards(
            cameraController.height,
            targetHeight,
            2f * Time.deltaTime
        );
    }

    // --------------------------------------------------
    // TRIGGER
    // --------------------------------------------------

    private IEnumerator DelayedDangerUpdate()
    {
        yield return new WaitForSeconds(3f);

        if (obstacleSpawner != null && playerInside)
        {
            obstacleSpawner.SetDangerLevel(0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;

        StartCoroutine(DelayedDangerUpdate());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;

        collisionPending = false;
        eyesEncounterActive = false;
        eyesDelayTimer = 0f;

        if (eyesController != null)
        {
            eyesController.CancelEncounter();
        }
    }
}
