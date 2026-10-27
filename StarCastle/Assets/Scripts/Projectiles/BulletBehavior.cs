using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private ContactRules _cr;
    private DestructionComponent _destruction;
    private HealthComponent _health;
    
    [SerializeField] private int _damage = 1;

    public float _time_left = 0;
    public bool _time_death = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _cr = GetComponent<ContactRules>();
        _destruction = GetComponent<DestructionComponent>();
        _health = GetComponent<HealthComponent>();
    }

    public void Initialize(float lifetime)
    {
        Initialize(Vector3.zero, lifetime, 0, _damage);
    }

    public void Initialize(Vector3 direction, float lifetime, float speed)
    {
        Initialize(direction, lifetime, speed, _damage);
    }

    public void Initialize(Vector3 direction, float lifetime, float speed, int damage)
    {
        _rb.linearVelocity = ((Vector2)direction).normalized * speed;
        if (lifetime > 0) 
        {
            _time_left = lifetime;
            _time_death = true;
        } else
        {
            _time_death = false;
        }

        _damage = damage;
    }

    private void FixedUpdate()
    {
        if (_time_death)
        {
            _time_left -= Time.deltaTime;
            if (_time_left <= 0)
            {
                _health.Kill();
            }
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_destruction.IsDestroying) return;

        Collider2D other = collision.collider;

        if (_cr.CanDamage(other))
        {
            HealthComponent target = other.GetComponentInParent<HealthComponent>();
            if (target != null) target.TryTakeDamage(_damage);
        }

        if (_cr.CostsDurability(other))
        {
            _health.TryTakeDamage(1);
        }
    }
}
