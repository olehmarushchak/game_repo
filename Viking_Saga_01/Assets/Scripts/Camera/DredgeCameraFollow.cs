
using UnityEngine;
using UnityEngine.InputSystem;

public class DredgeCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Position")]
    [SerializeField] private float distance = 12f;
    [SerializeField] public float height = 9f;

    [Header("Look")]
    [SerializeField] public float lookAhead = 7f;
    [SerializeField] public float lookHeight = 1.5f;

    [Header("Orbit")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float maxYawAngle = 40f;
    [SerializeField] private float yawReturnSpeed = 2f;

    [Header("Follow Smooth")]
    [SerializeField] private float positionSmoothTime = 0.12f;
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask cameraCollisionMask;
    [SerializeField] private float cameraCollisionRadius = 0.35f;
    [SerializeField] private float collisionOffset = 0.25f;
    [SerializeField] private float minimumDistance = 3.5f;

    [Header("Suspense Shake")]
    [Range(0f, 1f)]
    [SerializeField] private float dangerLevel = 0f;

    [SerializeField] private float maxPositionShake = 0.12f;
    [SerializeField] private float maxRotationShake = 1.2f;
    [SerializeField] private float shakeFrequency = 2.5f;
    [SerializeField] private float shakeSmoothness = 3f;

    private Vector3 positionVelocity;
    private float yawOffset;
    private float timeSinceMouseInput;

    // Shake state
    private Vector3 previousShakePosition;
    private Quaternion previousShakeRotation = Quaternion.identity;
    private bool hasPreviousShake;

    private Vector3 currentShakePosition;
    private Vector3 currentShakeRotation;

    private bool isShaking = false;

    private float noiseOffset;

    private void Start()
    {
        noiseOffset = Random.Range(0f, 1000f);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Remove last frame's shake before calculating
        // the actual follow camera position.
        RemovePreviousShake();

        ReadCameraInput();
        UpdateYawReturn();
        UpdateCamera();

        if (isShaking)
        {
            ApplySuspenseShake();
        }
    }

    // =====================================================
    // INPUT
    // =====================================================

    private void ReadCameraInput()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (Mathf.Abs(mouseDelta.x) > 0.01f)
        {
            yawOffset += mouseDelta.x * mouseSensitivity;

            yawOffset = Mathf.Clamp(
                yawOffset,
                -maxYawAngle,
                maxYawAngle
            );

            timeSinceMouseInput = 0f;
        }
        else
        {
            timeSinceMouseInput += Time.deltaTime;
        }
    }

    // =====================================================
    // AUTOMATIC RECENTERING
    // =====================================================

    private void UpdateYawReturn()
    {
        if (timeSinceMouseInput < 0.7f)
            return;

        float smoothing = 1f - Mathf.Exp(
            -yawReturnSpeed * Time.deltaTime
        );

        yawOffset = Mathf.Lerp(yawOffset, 0f, smoothing);
    }

    // =====================================================
    // CAMERA FOLLOW
    // =====================================================

    private void UpdateCamera()
    {
        Quaternion yawRotation = Quaternion.AngleAxis(
            yawOffset,
            Vector3.up
        );

        Vector3 cameraForward = yawRotation * target.forward;

        Vector3 lookPoint =
            target.position +
            target.forward * lookAhead +
            Vector3.up * lookHeight;

        Vector3 desiredPosition =
            target.position -
            cameraForward * distance +
            Vector3.up * height;

        // Camera collision
        Vector3 direction = desiredPosition - lookPoint;
        float desiredDistance = direction.magnitude;

        if (desiredDistance > 0.001f)
        {
            direction.Normalize();

            if (Physics.SphereCast(
                lookPoint,
                cameraCollisionRadius,
                direction,
                out RaycastHit hit,
                desiredDistance,
                cameraCollisionMask,
                QueryTriggerInteraction.Ignore))
            {
                float safeDistance = Mathf.Max(
                    hit.distance - collisionOffset,
                    minimumDistance
                );

                desiredPosition =
                    lookPoint + direction * safeDistance;
            }
        }

        // Smooth position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        // Look at target point
        Vector3 lookDirection = lookPoint - transform.position;

        if (lookDirection.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(
            lookDirection,
            Vector3.up
        );

        float rotationSmooth = 1f - Mathf.Exp(
            -rotationSmoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmooth
        );
    }

    // =====================================================
    // SUSPENSE SHAKE
    // =====================================================

    private void ApplySuspenseShake()
    {
        float danger = Mathf.Clamp01(dangerLevel);

        // Invert: low danger = strong shake.
        float intensity = 1f - danger;
        intensity *= intensity;

        float time = Time.time * shakeFrequency + noiseOffset;

        // Smooth Perlin noise, centered around zero.
        Vector3 noise = new Vector3(
            Mathf.PerlinNoise(time, 11.1f) - 0.5f,
            Mathf.PerlinNoise(22.2f, time) - 0.5f,
            Mathf.PerlinNoise(time, time * 0.37f) - 0.5f
        ) * 2f;

        Vector3 targetPositionShake =
            noise * maxPositionShake * intensity;

        Vector3 targetRotationShake = new Vector3(
            noise.y * maxRotationShake * intensity,
            noise.x * maxRotationShake * 0.35f * intensity,
            noise.x * maxRotationShake * intensity
        );

        float smoothing = 1f - Mathf.Exp(
            -shakeSmoothness * Time.deltaTime
        );

        currentShakePosition = Vector3.Lerp(
            currentShakePosition,
            targetPositionShake,
            smoothing
        );

        currentShakeRotation = Vector3.Lerp(
            currentShakeRotation,
            targetRotationShake,
            smoothing
        );

        // Apply shake AFTER the normal camera follow.
        previousShakePosition =
            transform.rotation * currentShakePosition;

        previousShakeRotation = Quaternion.Euler(
            currentShakeRotation
        );

        transform.position += previousShakePosition;
        transform.rotation =
            transform.rotation * previousShakeRotation;

        hasPreviousShake = true;
    }

    private void RemovePreviousShake()
    {
        if (!hasPreviousShake)
            return;

        transform.position -= previousShakePosition;

        transform.rotation =
            transform.rotation *
            Quaternion.Inverse(previousShakeRotation);

        hasPreviousShake = false;
    }

    // =====================================================
    // PUBLIC API
    // =====================================================

    public void SetDangerLevel(float value, bool shake)
    {
        dangerLevel = Mathf.Clamp01(value);
        isShaking = shake;
    }
}
