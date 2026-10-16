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
    [SerializeField] private int damage = 1;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _cr = GetComponent<ContactRules>();
        _destruction = GetComponent<DestructionComponent>();
        _health = GetComponent<HealthComponent>();
    }

    public void Initialize(Vector3 direction, float lifetime, float speed)
    {
        _rb.linearVelocity = ((Vector2)direction).normalized * speed;
        if (lifetime > 0) StartCoroutine(Lifetime(lifetime));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_destruction.IsDestroying) return;

        Collider2D other = collision.collider;

        if (_cr.CanDamage(other))
        {
            HealthComponent target = other.GetComponentInParent<HealthComponent>();
            if (target != null) target.TryTakeDamage(damage);
        }

        if (_cr.CostsDurability(other))
        {
            _health.TryTakeDamage(1);
        }
    }

    private IEnumerator Lifetime(float time)
    {
        yield return new WaitForSeconds(time);
        _destruction.StartDestruction();
    }
}
