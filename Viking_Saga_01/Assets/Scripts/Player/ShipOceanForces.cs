using UnityEngine;

public class ShipOceanForces : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private OceanWaveSampler ocean;

    [Header("Wave Push")]
    [Tooltip("Основна сила хвилі по горизонталі.")]
    [SerializeField] private float wavePushForce = 1.2f;

    [Header("Crest Impact")]
    [Tooltip("Додатковий поштовх, коли під кораблем проходить гребінь.")]
    [SerializeField] private float crestImpactForce = 1.5f;

    [Range(0f, 1f)]
    [SerializeField] private float crestThreshold = 0.65f;

    [Header("Passive Drift")]
    [Tooltip("Постійний слабкий дрейф у напрямку хвиль.")]
    [SerializeField] private float passiveDriftForce = 0.20f;

    [Header("Limits")]
    [Tooltip("Максимальна швидкість, яку океан може накопичити вздовж хвилі.")]
    [SerializeField] private float maxOceanAddedSpeed = 2.5f;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb == null || ocean == null)
            return;

        ApplyOceanForces();
    }

    private void ApplyOceanForces()
    {
        OceanWaveSampler.WaveSample sample =
            ocean.SampleWave(rb.worldCenterOfMass);

        Vector3 direction =
            Vector3.ProjectOnPlane(
                sample.direction,
                Vector3.up
            );

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        // 1. Постійний слабкий дрейф.
        Vector3 passiveForce =
            direction *
            passiveDriftForce;

        // 2. Сила залежить від амплітуди хвиль.
        Vector3 waveForce =
            direction *
            sample.intensity *
            wavePushForce;

        // 3. Додатковий поштовх на гребені.
        float crestFactor =
            Mathf.InverseLerp(
                crestThreshold,
                1f,
                sample.crest
            );

        crestFactor =
            Mathf.Clamp01(crestFactor);

        Vector3 crestForce =
            direction *
            crestFactor *
            crestImpactForce;

        Vector3 totalForce =
            passiveForce +
            waveForce +
            crestForce;

        // Не дозволяємо океану нескінченно розганяти корабель.
        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                rb.linearVelocity,
                Vector3.up
            );

        float velocityAlongWave =
            Vector3.Dot(
                horizontalVelocity,
                direction
            );

        if (velocityAlongWave < maxOceanAddedSpeed)
        {
            rb.AddForce(
                totalForce,
                ForceMode.Acceleration
            );
        }
    }
}