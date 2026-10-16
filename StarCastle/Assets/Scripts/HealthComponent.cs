using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] public int health = 2;
    [SerializeField] private float _invulnTime = 0f;
    [SerializeField] private bool _startWithInvuln = false;
    private bool isDead = false;
    private float _invulnerableUntil;
    public bool isInvulnerable => Time.time < _invulnerableUntil;

    public event Action<HealthComponent> Died;
    public event Action<HealthComponent> Damaged;

    private void Awake()
    {
        if (_startWithInvuln)
        {
            _invulnerableUntil = Time.time + _invulnTime;
        }
        
    }

    public void TryTakeDamage(int damage)
    {
        if (isDead || isInvulnerable || (damage <= 0))
            return;

        health -= damage;
        _invulnerableUntil = Time.time + _invulnTime;

        if (health <= 0)
        {
            isDead = true;
            Died?.Invoke(this);
        } else
        {
            Damaged?.Invoke(this);
        }
    }

    public void Kill()
    {
        if (isDead) return;

        isDead = true;
        Died?.Invoke(this);
    }
}
