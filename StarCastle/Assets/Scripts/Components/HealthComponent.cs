using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] public int health = 2;
    [SerializeField] private int _max_forcefields = 0;
    private int _forcefield_count = 0;
    public int ForcefieldCount => _forcefield_count;

    [SerializeField] private float _invulnTime = 0f;
    [SerializeField] private bool _startWithInvuln = false;
    [SerializeField] private bool _hasWindUp = false;
    [SerializeField] private float _windUpTime = 0.2f;

    public bool windingUp {get; set;} = false;
    public float _windUpProgress {get; set;} = 0.0f;
    private float _elapsed_time = 0f;


    private bool isDead = false;
    private float _invulnerableUntil;
    public bool isInvulnerable => Time.time < _invulnerableUntil;

    public event Action<HealthComponent> Died;
    public event Action<HealthComponent> Damaged;
    public event Action<HealthComponent> ForceFieldAbsorbed;
    public event Action<HealthComponent> WindUpStarted;
    public event Action<HealthComponent> HitAccepted;

    private void Awake()
    {
        if (_startWithInvuln)
        {
            _invulnerableUntil = Time.time + _invulnTime;
        }
    }

    void FixedUpdate()
    {
        if (windingUp)
        {
            if (_elapsed_time >= _windUpTime) 
            {
                _windUpProgress = 1.0f;
                windingUp = false;
                Died?.Invoke(this);
            } else
            {
                _windUpProgress = _elapsed_time / _windUpTime;
                _elapsed_time += Time.fixedDeltaTime;
            }
        }
    }

    public void TryTakeDamage(int damage)
    {
        if ((isDead && !windingUp) || isInvulnerable || (damage <= 0))
            return;

        if (_forcefield_count > 0)
        {
            _forcefield_count--;
            _invulnerableUntil = Time.time + _invulnTime;
            ForceFieldAbsorbed?.Invoke(this);
            return;
        }

        HitAccepted?.Invoke(this);

        health -= damage;
        _invulnerableUntil = Time.time + _invulnTime;

        if (health <= 0)
        {
            Kill();
            if (TryGetComponent<Rigidbody2D>(out Rigidbody2D body))
                {
                    body.simulated = false;
                }
        } else
        {
            Damaged?.Invoke(this);
        }
    }

    public void Kill()
    {
        if (isDead) return;
        isDead = true;

        if (_hasWindUp)
        {
            _windUpProgress = 0f;
            _elapsed_time = 0f;
            windingUp = true;

            WindUpStarted?.Invoke(this);
        } else
        {
            Died?.Invoke(this);
        }
    }

    public void AddForceFieldCharge (int amount)
    {
        _forcefield_count += amount;
        if (_forcefield_count > _max_forcefields) _forcefield_count = _max_forcefields;
    }
}
