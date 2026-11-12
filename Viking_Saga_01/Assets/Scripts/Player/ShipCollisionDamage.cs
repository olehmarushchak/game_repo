using UnityEngine;

public class ShipCollisionDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth shipHealth;


    [Header("Impact")]
    [Tooltip("Мінімальна швидкість удару для втрати 1 HP.")]
    [SerializeField] private float minImpactSpeed = 3.5f;


    [Header("Damage Protection")]
    [Tooltip("Не дозволяє одному удару зняти всі HP через кілька контактів.")]
    [SerializeField] private float damageCooldown = 1f;


    [Header("Damage Layers")]
    [SerializeField] private LayerMask damageLayers;


    private float nextDamageTime;


    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth =
                GetComponent<ShipHealth>();
        }
    }


    private void OnCollisionEnter(
        Collision collision
    )
    {
        if (shipHealth == null)
            return;

        if (shipHealth.IsDead)
            return;


        // =================================================
        // CHECK LAYER
        // =================================================

        int collisionLayer =
            collision.gameObject.layer;


        bool isDamageLayer =
            (
                damageLayers.value &
                (1 << collisionLayer)
            ) != 0;


        if (!isDamageLayer)
            return;


        // =================================================
        // COOLDOWN
        // =================================================

        if (Time.time < nextDamageTime)
            return;


        // =================================================
        // CONTACT
        // =================================================

        if (collision.contactCount == 0)
            return;


        ContactPoint contact =
            collision.GetContact(0);


        // =================================================
        // IMPACT SPEED
        //
        // Беремо не всю швидкість корабля,
        // а саме швидкість У НАПРЯМКУ поверхні.
        //
        // Це означає:
        //
        // сильний прямий удар = damage
        //
        // легке ковзання бортом =
        // може не дати damage
        // =================================================

        float impactSpeed =
            Mathf.Abs(
                Vector3.Dot(
                    collision.relativeVelocity,
                    contact.normal
                )
            );


        Debug.Log(
            "Collision impact: " +
            impactSpeed.ToString("F2")
        );


        // =================================================
        // DAMAGE
        // =================================================

        if (impactSpeed < minImpactSpeed)
            return;


        shipHealth.TakeDamage(1);


        nextDamageTime =
            Time.time +
            damageCooldown;
    }
}