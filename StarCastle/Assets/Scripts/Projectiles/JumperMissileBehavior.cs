using System.Collections;
using UnityEngine;

public class JumperMissileBehavior : MonoBehaviour
{
    [SerializeField] private float _tracking_time;
    [SerializeField] private float _cooldown_time;
    [SerializeField] private float _launchSpeed = 10f;
    [SerializeField] private float _jump_distance = 1f;
    MissileBehavior _missile;
    DrawTargetLine _line;
    Rigidbody2D _rb;
    ProximityFuse _prox;
    HealthComponent _health;

    private bool LockedOn = false;

    void Awake()
    {
        _missile = GetComponent<MissileBehavior>();
        _line = GetComponent<DrawTargetLine>();
        _line.enabled = false;
        _rb = GetComponent<Rigidbody2D>();
        _prox = GetComponent<ProximityFuse>();
        _health = GetComponent<HealthComponent>();
    }

    void Initialize(float waitTime)
    {
        _tracking_time = waitTime;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _missile.moving = true;
        _missile.aiming = true;
        LockedOn = false;
    }

    void FixedUpdate()
    {
        if(Vector2.Distance(_rb.position, _missile._target_rb.position) < _jump_distance)
        {
            if (!LockedOn)
            {
                StartCoroutine(ReleaseMissile(_tracking_time, _cooldown_time));
            }   
        }
    }

    private IEnumerator ReleaseMissile(float time, float cooldownTime)
    {
        LockedOn = true;
        _prox.enabled = false;
        _missile.moving = false; 
        _line.enabled = true;
        _line.Initialize(_tracking_time, _missile._target_rb);

        yield return new WaitForSeconds(time);

        _line.enabled = false;
        _line.Reset();

        _prox.enabled = true;

        _missile.aiming = false;

        _rb.angularVelocity = 0;
        _rb.linearVelocity = Vector2.zero;

        Vector2 direction = transform.right;
        Vector2 desiredVelocity = direction * _launchSpeed;

        _rb.AddForce((desiredVelocity - _rb.linearVelocity) * _rb.mass, ForceMode2D.Impulse);

        yield return new WaitForSeconds(cooldownTime);

        _missile.aiming = true;
        _missile.moving = true;
        LockedOn = false;
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
        StopAllCoroutines();

        _line.enabled = false;
        _line.Reset();
        _prox.enabled = false;

        enabled = false;
    }
}
