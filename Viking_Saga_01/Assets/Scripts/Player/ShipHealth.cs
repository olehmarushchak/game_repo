using System.Collections;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Ship Models")]
    [SerializeField] private GameObject ship100;
    [SerializeField] private GameObject ship50;
    [SerializeField] private GameObject ship20;

    [Header("Damage Transition")]
    [SerializeField] private ParticleSystem damageParticles;

    [Tooltip("Невелика затримка, щоб particles прикрили зміну моделі.")]
    [SerializeField] private float modelSwitchDelay = 0.08f;

    private int currentHealth;

    private bool isDead;
    private bool switchingModel;


    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;


    private void Awake()
    {
        currentHealth = maxHealth;

        UpdateShipModelImmediate();
    }


    // =====================================================
    // DAMAGE
    // =====================================================

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;


        currentHealth -= amount;

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maxHealth
            );


        Debug.Log(
            "SHIP DAMAGE | HP: " +
            currentHealth +
            "/" +
            maxHealth
        );


        if (currentHealth <= 0)
        {
            Die();
            return;
        }


        StartCoroutine(
            DamageTransition()
        );
    }


    // =====================================================
    // VISUAL TRANSITION
    // =====================================================

    private IEnumerator DamageTransition()
    {
        if (switchingModel)
            yield break;


        switchingModel = true;


        // -----------------------------
        // Damage FX
        // -----------------------------

        if (damageParticles != null)
        {
            damageParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            damageParticles.Play();
        }


        // particles починаються трохи раніше,
        // щоб прикрити "pop" між моделями

        yield return new WaitForSeconds(
            modelSwitchDelay
        );


        UpdateShipModelImmediate();


        switchingModel = false;
    }


    // =====================================================
    // MODEL STATE
    // =====================================================

    private void UpdateShipModelImmediate()
    {
        if (ship100 != null)
            ship100.SetActive(false);

        if (ship50 != null)
            ship50.SetActive(false);

        if (ship20 != null)
            ship20.SetActive(false);


        // 3 HP
        if (currentHealth >= 3)
        {
            if (ship100 != null)
                ship100.SetActive(true);

            return;
        }


        // 2 HP
        if (currentHealth == 2)
        {
            if (ship50 != null)
                ship50.SetActive(true);

            return;
        }


        // 1 HP
        if (currentHealth == 1)
        {
            if (ship20 != null)
                ship20.SetActive(true);

            return;
        }
    }


    // =====================================================
    // DEATH
    // =====================================================

    private void Die()
    {
        isDead = true;


        if (damageParticles != null)
        {
            damageParticles.Play();
        }


        Debug.Log("SHIP DESTROYED");


        /*
         * Пізніше тут буде:
         *
         * - disable controls
         * - sinking animation
         * - death VFX
         * - fade screen
         * - checkpoint respawn
         */
    }


    // =====================================================
    // LEVEL COMPLETE
    // =====================================================

    public void RestoreFullHealth()
    {
        isDead = false;

        currentHealth =
            maxHealth;


        UpdateShipModelImmediate();


        Debug.Log("SHIP REPAIRED");
    }
}