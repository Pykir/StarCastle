using System;
using UnityEngine;

public class MissileBehavior : MonoBehaviour
{
    private BulletBehavior _bullet;
    private Rigidbody2D _rb;
    private HealthComponent _health;
    public Rigidbody2D _target_rb {get; set;}

    [SerializeField] private float _time_before_death_trigger = 0.5f;
    [SerializeField, Min(0.01f)] private float _explosionRadius = 0.6f;
    public float ExplosionRadius => _explosionRadius;
    [SerializeField] private float _max_speed = 9f;
    [SerializeField] private float _forward_acc = 300f;
    [SerializeField] private float _torque = 20f;
    [SerializeField] private float _max_angular_speed = 180;
    [SerializeField] private float heading_gain = 4f;
    [SerializeField] private float steering_gain = 0.2f;
    [SerializeField, Min(0.01f)] private float _responseTime = 0.25f;
    [SerializeField] private bool _isMine = false;
    

    private bool attached = false;
    public bool aiming {get; set;} = true;
    public bool moving {get; set;} = true;

    void Awake()
    {
        _bullet = GetComponent<BulletBehavior>();
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<HealthComponent>();

        if (_isMine)
        {
            attached = true;
            _rb.simulated = false;
            GetComponent<Screenwrap>().enabled = false;
            GetComponent<ProximityFuse>().enabled = false;
        } 
    }

    public void Initialize(Rigidbody2D target)
    {
        _target_rb = target;
    }

    public void ActivateMine()
    {
        if (!attached) return;

        attached = false;
        _rb.simulated = true;
        GetComponent<Screenwrap>().enabled = true;
        GetComponent<ProximityFuse>().enabled = true;
    }

    void FixedUpdate()
    {
        if (attached || _target_rb == null) return;

        // this is what would send the missile straight at target
        Vector2 requestedVelocity = (_target_rb.position - _rb.position).normalized * _max_speed;

        // this is the acceleration needed to push the missile to that velocity over a certain time
        Vector2 requestedAcceleration = (requestedVelocity - _rb.linearVelocity) / Mathf.Max(_responseTime, 0.01f);

        if (aiming)
        {
            // this is how fast it wants to turn
            float requestAngle = Mathf.Clamp(Vector2.SignedAngle(transform.right, requestedAcceleration) * heading_gain, -_max_angular_speed, _max_angular_speed);
            
            // this is how much force to push it towards that speed
            float finalTorque = Mathf.Clamp((requestAngle - _rb.angularVelocity) * steering_gain, -_torque, _torque);

            _rb.AddTorque(finalTorque, ForceMode2D.Force);
        }

        if (moving)
        {
            // this is the actual possible acceleration due to the direction of the missile
            // projects the wanted acceleration vector onto the current heading, and only thrusts by the amount that is in both directions
            float possibleAcceleration = Vector2.Dot(requestedAcceleration, (Vector2)transform.right);
            float thrustForce = Mathf.Clamp(possibleAcceleration * _rb.mass, 0f, _forward_acc);

            _rb.AddForce(transform.right * thrustForce, ForceMode2D.Force);
        }
    }

    void OnEnable()
    {
        _health.WindUpStarted += HandleWindUp;

        if (_health.windingUp)
            HandleWindUp(_health);
    }

    void OnDisable()
    {
        _health.WindUpStarted -= HandleWindUp;
    }

    void HandleWindUp(HealthComponent source)
    {
        if (TryGetComponent<ProximityFuse>(out var fuse)) {
            fuse.enabled = false;
        }

        moving = true;
        aiming = true;
    }
}
