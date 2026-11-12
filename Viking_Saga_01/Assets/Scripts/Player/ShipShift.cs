using UnityEngine;
using UnityEngine.InputSystem;

public class ShipShift : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipMovement shipMovement;
    [SerializeField] private Camera camera;

    [Header("Boost")]
    [SerializeField] private float boostDuration = 1.5f;
    [SerializeField] private float boostCooldown = 5f;

    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float accelerationMultiplier = 2.2f;

    [Header("Boost Handling")]
    [SerializeField] private float lateralDragMultiplier = 1.2f;
    [SerializeField] private float steeringTorqueMultiplier = 0.85f;
    [SerializeField] private float angularDragMultiplier = 1f;
    [SerializeField] private float maxYawSpeedMultiplier = 1f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 50f;
    [SerializeField] private float boostFOV = 65f;
    [SerializeField] private float fovSmoothSpeed = 5f;

    private float defaultSpeed;
    private float defaultAcceleration;

    private float defaultLateralDrag;
    private float defaultSteeringTorque;
    private float defaultAngularDrag;
    private float defaultMaxYawSpeed;

    private bool isBoosting;
    public bool IsBoosting => isBoosting;
    private float boostTimer;
    private float cooldownTimer;


    private void Start()
    {
        defaultSpeed =
            shipMovement.maxForwardSpeed;

        defaultAcceleration =
            shipMovement.forwardAcceleration;

        defaultLateralDrag =
            shipMovement.lateralDrag;

        defaultSteeringTorque =
            shipMovement.steeringTorque;

        defaultAngularDrag =
            shipMovement.angularDrag;

        defaultMaxYawSpeed =
            shipMovement.maxYawSpeed;

        camera.fieldOfView =
            normalFOV;
    }


    private void Update()
    {
        if (Keyboard.current == null)
            return;


        UpdateCooldown();

        ReadBoostInput();

        UpdateBoost();

        UpdateFOV();
    }


    private void ReadBoostInput()
    {
        if (
            Keyboard.current.leftShiftKey.wasPressedThisFrame &&
            !isBoosting &&
            cooldownTimer <= 0f
        )
        {
            StartBoost();
        }
    }


    private void StartBoost()
    {
        isBoosting = true;

        boostTimer =
            boostDuration;


        shipMovement.maxForwardSpeed =
            defaultSpeed *
            speedMultiplier;


        shipMovement.forwardAcceleration =
            defaultAcceleration *
            accelerationMultiplier;


        shipMovement.lateralDrag =
            defaultLateralDrag *
            lateralDragMultiplier;


        shipMovement.steeringTorque =
            defaultSteeringTorque *
            steeringTorqueMultiplier;


        shipMovement.angularDrag =
            defaultAngularDrag *
            angularDragMultiplier;


        shipMovement.maxYawSpeed =
            defaultMaxYawSpeed *
            maxYawSpeedMultiplier;
    }


    private void UpdateBoost()
    {
        if (!isBoosting)
            return;


        boostTimer -=
            Time.deltaTime;


        if (boostTimer <= 0f)
        {
            StopBoost();
        }
    }


    private void StopBoost()
    {
        isBoosting = false;


        shipMovement.maxForwardSpeed =
            defaultSpeed;


        shipMovement.forwardAcceleration =
            defaultAcceleration;


        shipMovement.lateralDrag =
            defaultLateralDrag;


        shipMovement.steeringTorque =
            defaultSteeringTorque;


        shipMovement.angularDrag =
            defaultAngularDrag;


        shipMovement.maxYawSpeed =
            defaultMaxYawSpeed;


        cooldownTimer =
            boostCooldown;
    }


    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -=
                Time.deltaTime;
        }
    }


    private void UpdateFOV()
    {
        float targetFOV =
            isBoosting
            ? boostFOV
            : normalFOV;


        float smooth =
            1f - Mathf.Exp(
                -fovSmoothSpeed *
                Time.deltaTime
            );


        camera.fieldOfView =
            Mathf.Lerp(
                camera.fieldOfView,
                targetFOV,
                smooth
            );
    }
}