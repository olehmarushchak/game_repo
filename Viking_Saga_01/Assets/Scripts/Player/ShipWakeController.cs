using UnityEngine;

public class ShipWakeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private OceanWaveSampler ocean;
    [SerializeField] private ShipShift shipShift;

    [SerializeField] private Transform leftWakePoint;
    [SerializeField] private Transform rightWakePoint;

    [SerializeField] private TrailRenderer leftTrail;
    [SerializeField] private TrailRenderer rightTrail;


    [Header("Activation")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 8f;


    [Header("Normal Wake")]
    [SerializeField] private float minWidth = 0.2f;
    [SerializeField] private float maxWidth = 0.8f;

    [SerializeField] private float minTime = 0.4f;
    [SerializeField] private float maxTime = 2.2f;


    [Header("Boost Wake")]
    [Tooltip("Наскільки ширшим стає слід під час boost.")]
    [SerializeField] private float boostWidthMultiplier = 1.6f;

    [Tooltip("Наскільки довше піна залишається після boost.")]
    [SerializeField] private float boostTimeMultiplier = 1.3f;

    [Tooltip("Швидкість плавного переходу між normal і boost wake.")]
    [SerializeField] private float boostSmoothSpeed = 5f;


    [Header("Water")]
    [SerializeField] private float waterOffset = 0.03f;


    private float currentBoostIntensity;


    private void LateUpdate()
    {
        if (rb == null || ocean == null)
            return;


        FollowWater(leftWakePoint);
        FollowWater(rightWakePoint);

        UpdateBoostIntensity();

        UpdateWake();
    }


    private void FollowWater(Transform point)
    {
        if (point == null)
            return;


        Vector3 pos =
            point.position;


        float waterHeight =
            ocean.GetWaveHeight(pos);


        pos.y =
            waterHeight +
            waterOffset;


        point.position =
            pos;
    }


    private void UpdateBoostIntensity()
    {
        float targetBoost =
            shipShift != null &&
            shipShift.IsBoosting
            ? 1f
            : 0f;


        float smooth =
            1f - Mathf.Exp(
                -boostSmoothSpeed *
                Time.deltaTime
            );


        currentBoostIntensity =
            Mathf.Lerp(
                currentBoostIntensity,
                targetBoost,
                smooth
            );
    }


    private void UpdateWake()
    {
        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                rb.linearVelocity,
                Vector3.up
            );


        float speed =
            horizontalVelocity.magnitude;


        float speed01 =
            Mathf.InverseLerp(
                minSpeed,
                maxSpeed,
                speed
            );


        bool active =
            speed > minSpeed;


        UpdateTrail(
            leftTrail,
            active,
            speed01
        );


        UpdateTrail(
            rightTrail,
            active,
            speed01
        );
    }


    private void UpdateTrail(
        TrailRenderer trail,
        bool active,
        float speed01
    )
    {
        if (trail == null)
            return;


        trail.emitting =
            active;


        // ==========================================
        // NORMAL WAKE
        // ==========================================

        float normalWidth =
            Mathf.Lerp(
                minWidth,
                maxWidth,
                speed01
            );


        float normalTime =
            Mathf.Lerp(
                minTime,
                maxTime,
                speed01
            );


        // ==========================================
        // BOOST WAKE
        // ==========================================

        float widthMultiplier =
            Mathf.Lerp(
                1f,
                boostWidthMultiplier,
                currentBoostIntensity
            );


        float timeMultiplier =
            Mathf.Lerp(
                1f,
                boostTimeMultiplier,
                currentBoostIntensity
            );


        trail.widthMultiplier =
            normalWidth *
            widthMultiplier;


        trail.time =
            normalTime *
            timeMultiplier;
    }
}