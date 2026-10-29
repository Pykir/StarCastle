using UnityEngine;

public class DestructionComponent : MonoBehaviour
{
    private HealthComponent _health;
    [SerializeField] private GameObject _destructionEffect;

    public bool IsDestroying { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        IsDestroying = false;
    }

    private void OnEnable()
    {
        if (_health != null)
            _health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.Died -= HandleDeath;
    }

    private void HandleDeath(HealthComponent source, bool instantRemove)
    {
        if (instantRemove)
        {
            StartRemoval();
        } else
        {
           StartDestruction(); 
        }
    }

    public void StartDestruction()
    {
        if (IsDestroying) return;

        IsDestroying = true;

        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }

        if (_destructionEffect) {
            GameObject effect = Instantiate(_destructionEffect, transform.position, transform.rotation);

            if (TryGetComponent<MissileBehavior>(out var missile) && effect.TryGetComponent<ExplosionComponent>(out var explosion))
            {
                explosion.Initialize(missile.ExplosionRadius);
            }
        }
        
        Destroy(gameObject);
    }

    public void StartRemoval()
    {
        if (IsDestroying) return;

        IsDestroying = true;

        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }

        Destroy(gameObject);
    }
}
