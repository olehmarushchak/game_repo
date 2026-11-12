using UnityEngine;
using UnityEngine.InputSystem;

public class DredgeStyleCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Distance")]
    [SerializeField] private float distance = 11f;
    [SerializeField] private float minDistance = 6f;
    [SerializeField] private float maxDistance = 16f;

    [Header("Orbit")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 32f;

    [SerializeField] private float minPitch = 18f;
    [SerializeField] private float maxPitch = 55f;

    [Header("Sensitivity")]
    [SerializeField] private float yawSensitivity = 0.12f;
    [SerializeField] private float pitchSensitivity = 0.10f;

    [Header("Target Offset")]
    [SerializeField] private float targetHeight = 1.5f;
    [SerializeField] private float lookAhead = 2f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 12f;

    [Header("Auto Recenter")]
    [SerializeField] private bool autoRecenter = false;
    [SerializeField] private float recenterDelay = 2f;
    [SerializeField] private float recenterSpeed = 2f;

    [Header("Collision")]
    [SerializeField] private bool useCollision = true;
    [SerializeField] private LayerMask collisionMask;

    [SerializeField] private float collisionRadius = 0.35f;
    [SerializeField] private float collisionPadding = 0.2f;

    [Header("Optional Zoom")]
    [SerializeField] private bool allowZoom = false;
    [SerializeField] private float zoomSpeed = 2f;

    private Vector3 positionVelocity;

    private float timeSinceManualInput;

    private void Start()
    {
        if (target == null)
            return;

        Vector3 direction =
            transform.position -
            target.position;

        if (direction.sqrMagnitude > 0.01f)
        {
            distance =
                Mathf.Clamp(
                    direction.magnitude,
                    minDistance,
                    maxDistance
                );
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        ReadInput();

        UpdateRecentering();

        UpdateCamera();
    }

    // =========================================================
    // INPUT
    // =========================================================

    private void ReadInput()
    {
        bool receivedInput = false;

        if (Mouse.current != null)
        {
            Vector2 delta =
                Mouse.current.delta.ReadValue();

            if (
                Mathf.Abs(delta.x) > 0.01f ||
                Mathf.Abs(delta.y) > 0.01f
            )
            {
                yaw +=
                    delta.x *
                    yawSensitivity;

                pitch -=
                    delta.y *
                    pitchSensitivity;

                receivedInput = true;
            }

            // Middle mouse = recenter behind target
            if (
                Mouse.current.middleButton
                    .wasPressedThisFrame
            )
            {
                RecenterBehindTarget();
            }

            if (allowZoom)
            {
                float scroll =
                    Mouse.current.scroll
                        .ReadValue().y;

                if (Mathf.Abs(scroll) > 0.01f)
                {
                    distance -=
                        scroll *
                        0.01f *
                        zoomSpeed;

                    receivedInput = true;
                }
            }
        }

        pitch =
            Mathf.Clamp(
                pitch,
                minPitch,
                maxPitch
            );

        distance =
            Mathf.Clamp(
                distance,
                minDistance,
                maxDistance
            );

        if (receivedInput)
        {
            timeSinceManualInput = 0f;
        }
        else
        {
            timeSinceManualInput +=
                Time.deltaTime;
        }
    }

    // =========================================================
    // RECENTER
    // =========================================================

    private void UpdateRecentering()
    {
        if (!autoRecenter)
            return;

        if (
            timeSinceManualInput <
            recenterDelay
        )
            return;

        float targetYaw =
            target.eulerAngles.y;

        yaw =
            Mathf.LerpAngle(
                yaw,
                targetYaw,
                1f -
                Mathf.Exp(
                    -recenterSpeed *
                    Time.deltaTime
                )
            );
    }

    public void RecenterBehindTarget()
    {
        if (target == null)
            return;

        yaw =
            target.eulerAngles.y;

        timeSinceManualInput = 0f;
    }

    // =========================================================
    // CAMERA UPDATE
    // =========================================================

    private void UpdateCamera()
    {
        Quaternion orbitRotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 targetForward =
            Vector3.ProjectOnPlane(
                target.forward,
                Vector3.up
            ).normalized;

        Vector3 focusPoint =
            target.position
            +
            Vector3.up *
            targetHeight
            +
            targetForward *
            lookAhead;

        Vector3 offset =
            orbitRotation *
            new Vector3(
                0f,
                0f,
                -distance
            );

        Vector3 desiredPosition =
            focusPoint +
            offset;

        if (useCollision)
        {
            desiredPosition =
                ResolveCollision(
                    focusPoint,
                    desiredPosition
                );
        }

        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );

        Vector3 lookDirection =
            focusPoint -
            transform.position;

        if (
            lookDirection.sqrMagnitude <
            0.001f
        )
            return;

        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookDirection,
                Vector3.up
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f -
                Mathf.Exp(
                    -rotationSmoothSpeed *
                    Time.deltaTime
                )
            );
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private Vector3 ResolveCollision(
        Vector3 focusPoint,
        Vector3 desiredPosition
    )
    {
        Vector3 direction =
            desiredPosition -
            focusPoint;

        float length =
            direction.magnitude;

        if (length <= 0.001f)
            return desiredPosition;

        direction.Normalize();

        if (
            Physics.SphereCast(
                focusPoint,
                collisionRadius,
                direction,
                out RaycastHit hit,
                length,
                collisionMask,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            float safeDistance =
                Mathf.Max(
                    0f,
                    hit.distance -
                    collisionPadding
                );

            return
                focusPoint +
                direction *
                safeDistance;
        }

        return desiredPosition;
    }
}