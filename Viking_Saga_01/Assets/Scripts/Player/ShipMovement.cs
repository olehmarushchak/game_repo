using UnityEngine;
using UnityEngine.InputSystem;

public class ShipMovement : MonoBehaviour
{
    [Header("Engine")]
    [SerializeField] public float forwardAcceleration = 2.8f;
    [SerializeField] public float reverseAcceleration = 1.8f;

    [SerializeField] public float maxForwardSpeed = 8f;
    [SerializeField] public float maxReverseSpeed = 3f;

    [Header("Water Resistance")]
    [Tooltip("Опір руху вперед. Менше = більше інерції.")]
    [SerializeField] public float forwardDrag = 0.18f;

    [Tooltip("Опір боковому ковзанню. Менше = сильніший drift.")]
    [SerializeField] public float lateralDrag = 0.65f;

    [Header("Steering")]
    [SerializeField] public float steeringTorque = 7f;

    [Tooltip("Наскільки швидко гаситься обертання після відпускання керма.")]
    [SerializeField] public float angularDrag = 2f;

    [SerializeField] public float maxYawSpeed = 0.75f;

    private Rigidbody rb;

    private float throttleInput;
    private float steeringInput;

    public float SteeringInput => steeringInput;

    public float ForwardSpeed
    {
        get
        {
            Vector3 horizontalVelocity =
                Vector3.ProjectOnPlane(
                    rb.linearVelocity,
                    Vector3.up
                );

            return Vector3.Dot(
                horizontalVelocity,
                transform.forward
            );
        }
    }

    public float Speed01 =>
        Mathf.Clamp01(
            Mathf.Abs(ForwardSpeed) /
            maxForwardSpeed
        );
    
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {

        ReadInput();
        // Debug.Log("ForwardSpeed: " + Speed01);

    }

    private void FixedUpdate()
    {
        ApplyEngine();
        ApplyWaterResistance();
        ApplySteering();
        LimitSpeed();
    }

    private void ReadInput()
    {
        if (Keyboard.current == null)
            return;

        throttleInput = 0f;
        steeringInput = 0f;

        if (Keyboard.current.wKey.isPressed)
            throttleInput += 1f;

        if (Keyboard.current.sKey.isPressed)
            throttleInput -= 1f;

        if (Keyboard.current.aKey.isPressed)
            steeringInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            steeringInput += 1f;
    }

    private void ApplyEngine()
    {
        if (Mathf.Abs(throttleInput) < 0.01f)
            return;

        float acceleration =
            throttleInput > 0f
            ? forwardAcceleration
            : reverseAcceleration;

        Vector3 force =
            transform.forward *
            throttleInput *
            acceleration;

        rb.AddForce(
            force,
            ForceMode.Acceleration
        );
    }

    private void ApplyWaterResistance()
    {
        Vector3 horizontalVelocity =
            Vector3.ProjectOnPlane(
                rb.linearVelocity,
                Vector3.up
            );

        float forwardVelocity =
            Vector3.Dot(
                horizontalVelocity,
                transform.forward
            );

        float lateralVelocity =
            Vector3.Dot(
                horizontalVelocity,
                transform.right
            );

        // Невеликий опір вздовж корпусу.
        Vector3 forwardResistance =
            -transform.forward *
            forwardVelocity *
            forwardDrag;

        // Сильніший опір поперек корпусу.
        // Саме ця різниця дає корабельний drift.
        Vector3 lateralResistance =
            -transform.right *
            lateralVelocity *
            lateralDrag;

        rb.AddForce(
            forwardResistance + lateralResistance,
            ForceMode.Acceleration
        );
    }

    private void ApplySteering()
    {
        float forwardSpeed = ForwardSpeed;

        float speedFactor =
            Mathf.Clamp01(
                Mathf.Abs(forwardSpeed) /
                maxForwardSpeed
            );

        // На дуже малій швидкості кермо майже не працює.
        float steeringAuthority =
            Mathf.Lerp(
                0.08f,
                1f,
                speedFactor
            );

        float direction =
            Mathf.Abs(forwardSpeed) > 0.05f
            ? Mathf.Sign(forwardSpeed)
            : 1f;

        float torque =
            steeringInput *
            steeringTorque *
            steeringAuthority *
            direction;

        rb.AddTorque(
            Vector3.up * torque,
            ForceMode.Acceleration
        );

        // Коли кермо відпущене — вода поступово гасить yaw.
        float yaw =
            rb.angularVelocity.y;

        float damping =
            -yaw * angularDrag;

        rb.AddTorque(
            Vector3.up * damping,
            ForceMode.Acceleration
        );

        Vector3 angularVelocity =
            rb.angularVelocity;

        angularVelocity.y =
            Mathf.Clamp(
                angularVelocity.y,
                -maxYawSpeed,
                maxYawSpeed
            );

        rb.angularVelocity =
            angularVelocity;
    }

    private void LimitSpeed()
    {
        Vector3 velocity =
            rb.linearVelocity;

        Vector3 horizontal =
            Vector3.ProjectOnPlane(
                velocity,
                Vector3.up
            );

        float forward =
            Vector3.Dot(
                horizontal,
                transform.forward
            );

        // Ми clamp-имо тільки forward component.
        // Боковий drift від хвиль НЕ стираємо.
        if (forward > maxForwardSpeed)
        {
            velocity -=
                transform.forward *
                (forward - maxForwardSpeed);
        }

        if (forward < -maxReverseSpeed)
        {
            velocity -=
                transform.forward *
                (forward + maxReverseSpeed);
        }

        rb.linearVelocity = velocity;
    }
}