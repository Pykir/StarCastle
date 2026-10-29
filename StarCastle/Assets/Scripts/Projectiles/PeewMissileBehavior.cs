using System;
using System.Collections;
using UnityEngine;

public class PeewMissileBehavior : MonoBehaviour
{
    private Camera _camera;

    [SerializeField] private float _tracking_time = 2.5f;
    [SerializeField] private float _locked_time = 0.2f;
    [SerializeField] private float _launchSpeed = 25f;
    MissileBehavior _missile;
    Rigidbody2D _rb;
    HealthComponent _health;

    public event Action MissileLaunched;
    public event Action<float> TargetingStarted;

    void Awake()
    {
        _camera = Camera.main;
        _health = GetComponent<HealthComponent>();
    }

    void Initialize(float waitTime)
    {
        _tracking_time = waitTime;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _missile = GetComponent<MissileBehavior>();
        _rb = GetComponent<Rigidbody2D>();

        _missile.moving = false;
        TargetingStarted?.Invoke(_tracking_time + _locked_time);

        StartCoroutine(ReleaseMissile(_tracking_time, _locked_time));
    }

    void FixedUpdate()
    {
        Vector3 viewport = _camera.WorldToViewportPoint(transform.position);

        bool outside =
            viewport.z <= 0f ||
            viewport.x < 0f || viewport.x > 1f ||
            viewport.y < 0f || viewport.y > 1f;

        if (outside)
        {
            _health.Remove();
            return;
        }
    }

    private IEnumerator ReleaseMissile(float time, float lockedTime)
    {
        yield return new WaitForSeconds(time);

        _missile.aiming = false;
        _missile.moving = false;

        _rb.angularVelocity = 0;
        _rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(lockedTime);

        Vector2 direction = transform.right;
        Vector2 desiredVelocity = direction * _launchSpeed;

        _rb.AddForce((desiredVelocity - _rb.linearVelocity) * _rb.mass, ForceMode2D.Impulse);

        MissileLaunched?.Invoke();
    }
}
