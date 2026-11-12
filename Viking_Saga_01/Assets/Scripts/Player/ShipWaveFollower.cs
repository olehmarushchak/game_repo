using UnityEngine;

public class ShipWaveFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OceanWaveSampler ocean;
    [SerializeField] private Transform shipRoot;

    [Header("Sample Points")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform backPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Height")]
    [SerializeField] private float waterOffset = -0.7f;
    [SerializeField] private float heightFollowSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationFollowSpeed = 4f;
    [SerializeField] private float pitchMultiplier = 1f;
    [SerializeField] private float rollMultiplier = 1f;

    [Header("Limits")]
    [SerializeField] private float maxPitch = 10f;
    [SerializeField] private float maxRoll = 12f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (
            ocean == null ||
            shipRoot == null ||
            centerPoint == null ||
            frontPoint == null ||
            backPoint == null ||
            leftPoint == null ||
            rightPoint == null
        )
        {
            return;
        }

        UpdateHeight();
        // UpdateRotation();
    }

    private void UpdateHeight()
    {
        float centerHeight =
            ocean.GetWaveHeight(centerPoint.position);

        float targetLocalY =
            centerHeight
            - shipRoot.position.y
            + waterOffset;

        Vector3 localPosition = transform.localPosition;

        float smoothing =
            1f - Mathf.Exp(
                -heightFollowSpeed * Time.deltaTime
            );

        localPosition.y =
            Mathf.Lerp(
                localPosition.y,
                startLocalPosition.y + targetLocalY,
                smoothing
            );

        transform.localPosition = localPosition;
    }

    private void UpdateRotation()
    {
        float frontHeight =
            ocean.GetWaveHeight(frontPoint.position);

        float backHeight =
            ocean.GetWaveHeight(backPoint.position);

        float leftHeight =
            ocean.GetWaveHeight(leftPoint.position);

        float rightHeight =
            ocean.GetWaveHeight(rightPoint.position);

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

        float pitch =
            Mathf.Atan2(
                frontHeight - backHeight,
                forwardDistance
            ) * Mathf.Rad2Deg;

        float roll =
            Mathf.Atan2(
                rightHeight - leftHeight,
                sideDistance
            ) * Mathf.Rad2Deg;

        pitch *= pitchMultiplier;
        roll *= rollMultiplier;

        pitch = Mathf.Clamp(
            pitch,
            -maxPitch,
            maxPitch
        );

        roll = Mathf.Clamp(
            roll,
            -maxRoll,
            maxRoll
        );

        Quaternion waveRotation =
            Quaternion.Euler(
                -pitch,
                0f,
                roll
            );

        Quaternion targetRotation =
            startLocalRotation * waveRotation;

        float smoothing =
            1f - Mathf.Exp(
                -rotationFollowSpeed * Time.deltaTime
            );

        transform.localRotation =
            Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                smoothing
            );
    }
}