using UnityEngine;

public class ShipStability : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OceanWaveSampler ocean;
    [SerializeField] private Rigidbody shipRigidbody;
    [SerializeField] private ShipMovement shipMovement;

    [Header("Float Points")]
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;


    [Header("Wave Influence")]
    [SerializeField] private float wavePitchMultiplier = 1.0f;
    [SerializeField] private float waveRollMultiplier = 1.0f;


    [Header("Turning Bank")]
    [Tooltip("Нахил корабля при повороті.")]
    [SerializeField] private float turnBankStrength = 7f;


    [Header("Drift Bank")]
    [Tooltip("Наскільки бокове ковзання нахиляє корабель.")]
    [SerializeField] private float driftBankStrength = 1.5f;


    [Header("Stability")]
    [Tooltip("Наскільки сильно корпус намагається повернутися до target angle.")]
    [SerializeField] private float restoringStrength = 9f;

    [Tooltip("Гасіння коливань. Менше = корабель довше розгойдується.")]
    [SerializeField] private float damping = 4f;


    [Header("Limits")]
    [SerializeField] private float maxPitch = 18f;
    [SerializeField] private float maxRoll = 25f;


    [Header("Gameplay Stability")]
    [SerializeField] private float dangerPitch = 12f;
    [SerializeField] private float dangerRoll = 16f;


    private Quaternion startLocalRotation;

    private float currentPitch;
    private float currentRoll;

    private float pitchVelocity;
    private float rollVelocity;


    public float CurrentPitch => currentPitch;
    public float CurrentRoll => currentRoll;

    public float Stability01 { get; private set; } = 1f;


    private void Awake()
    {
        startLocalRotation = transform.localRotation;

        if (shipRigidbody == null)
        {
            shipRigidbody =
                GetComponentInParent<Rigidbody>();
        }

        if (shipMovement == null)
        {
            shipMovement =
                GetComponentInParent<ShipMovement>();
        }
    }


    private void LateUpdate()
    {
        if (
            ocean == null ||
            shipRigidbody == null ||
            frontPoint == null ||
            backPoint == null ||
            leftPoint == null ||
            rightPoint == null
        )
        {
            return;
        }

        SimulateStability();

        ApplyRotation();

        CalculateStability();
    }


    private void SimulateStability()
    {
        //--------------------------------------------------
        // 1. SAMPLE WATER HEIGHTS
        //--------------------------------------------------

        float frontHeight =
            ocean.GetWaveHeight(
                frontPoint.position
            );

        float backHeight =
            ocean.GetWaveHeight(
                backPoint.position
            );

        float leftHeight =
            ocean.GetWaveHeight(
                leftPoint.position
            );

        float rightHeight =
            ocean.GetWaveHeight(
                rightPoint.position
            );


        //--------------------------------------------------
        // 2. DISTANCE BETWEEN SAMPLE POINTS
        //--------------------------------------------------

        float forwardDistance =
            Vector3.Distance(
                frontPoint.position,
                backPoint.position
            );

        float sideDistance =
            Vector3.Distance(
                leftPoint.position,
                rightPoint.position
            );


        if (forwardDistance < 0.01f)
            forwardDistance = 0.01f;

        if (sideDistance < 0.01f)
            sideDistance = 0.01f;


        //--------------------------------------------------
        // 3. WAVE PITCH
        //--------------------------------------------------

        float wavePitch =
            Mathf.Atan2(
                frontHeight - backHeight,
                forwardDistance
            )
            * Mathf.Rad2Deg;


        wavePitch *=
            wavePitchMultiplier;


        //--------------------------------------------------
        // 4. WAVE ROLL
        //--------------------------------------------------

        float waveRoll =
            Mathf.Atan2(
                rightHeight - leftHeight,
                sideDistance
            )
            * Mathf.Rad2Deg;


        waveRoll *=
            waveRollMultiplier;


        //--------------------------------------------------
        // 5. TURN BANK
        //--------------------------------------------------

        float turnBank = 0f;

        if (shipMovement != null)
        {
            turnBank =
                -shipMovement.SteeringInput *
                shipMovement.Speed01 *
                turnBankStrength;
        }


        //--------------------------------------------------
        // 6. DRIFT BANK
        //--------------------------------------------------

        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                shipRigidbody.linearVelocity,
                Vector3.up
            );


        float lateralSpeed =
            Vector3.Dot(
                horizontalVelocity,
                shipRigidbody.transform.right
            );


        float driftBank =
            -lateralSpeed *
            driftBankStrength;


        //--------------------------------------------------
        // 7. TARGET ANGLES
        //--------------------------------------------------

        float targetPitch =
            -wavePitch;


        float targetRoll =
            waveRoll +
            turnBank +
            driftBank;


        targetPitch =
            Mathf.Clamp(
                targetPitch,
                -maxPitch,
                maxPitch
            );


        targetRoll =
            Mathf.Clamp(
                targetRoll,
                -maxRoll,
                maxRoll
            );


        //--------------------------------------------------
        // 8. SPRING PHYSICS
        //
        // Instead of:
        //
        // currentRoll = targetRoll
        //
        // we simulate:
        //
        // target
        //   ↓
        // restoring force
        //   ↓
        // angular velocity
        //   ↓
        // current angle
        //--------------------------------------------------


        float pitchAcceleration =
            (
                targetPitch -
                currentPitch
            )
            * restoringStrength
            -
            pitchVelocity *
            damping;


        float rollAcceleration =
            (
                targetRoll -
                currentRoll
            )
            * restoringStrength
            -
            rollVelocity *
            damping;


        //--------------------------------------------------
        // 9. ANGULAR VELOCITY
        //--------------------------------------------------

        pitchVelocity +=
            pitchAcceleration *
            Time.deltaTime;


        rollVelocity +=
            rollAcceleration *
            Time.deltaTime;


        //--------------------------------------------------
        // 10. ANGLES
        //--------------------------------------------------

        currentPitch +=
            pitchVelocity *
            Time.deltaTime;


        currentRoll +=
            rollVelocity *
            Time.deltaTime;


        //--------------------------------------------------
        // 11. SAFETY LIMIT
        //--------------------------------------------------

        currentPitch =
            Mathf.Clamp(
                currentPitch,
                -maxPitch,
                maxPitch
            );


        currentRoll =
            Mathf.Clamp(
                currentRoll,
                -maxRoll,
                maxRoll
            );
    }


    private void ApplyRotation()
    {
        Quaternion stabilityRotation =
            Quaternion.Euler(
                currentPitch,
                0f,
                currentRoll
            );


        transform.localRotation =
            startLocalRotation *
            stabilityRotation;
    }


    private void CalculateStability()
    {
        float pitchDanger =
            Mathf.InverseLerp(
                0f,
                dangerPitch,
                Mathf.Abs(currentPitch)
            );


        float rollDanger =
            Mathf.InverseLerp(
                0f,
                dangerRoll,
                Mathf.Abs(currentRoll)
            );


        float danger =
            Mathf.Max(
                pitchDanger,
                rollDanger
            );


        Stability01 =
            1f -
            Mathf.Clamp01(danger);
    }
}