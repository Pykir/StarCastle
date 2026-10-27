using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ExplosionComponent : MonoBehaviour
{
    private ContactRules _cr;
    

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _radius = 1;
    public float Radius => _radius;
    [SerializeField] private float _explosionForce = 5;
    [SerializeField] private int _explosionType = 0;
    
    private bool _exploding = false;

    public event System.Action<float>RadiusChanged;
    public event System.Action Detonating;

    public void Awake()
    {
        _cr = GetComponent<ContactRules>();
    }

    public void Initialize(float radius)
    {
        Initialize(radius, _damage, _explosionForce, _explosionType);
    }

    public void Initialize(float radius, int damage)
    {
        Initialize(radius, damage, _explosionForce, _explosionType);
    }

    public void Initialize(float radius, int damage, float force, int type)
    {
        _radius = radius;
        _damage = damage;
        _explosionForce = force;
        _explosionType = type;
        RadiusChanged?.Invoke(_radius);
    }

    public void Start()
    {
        _exploding = false;
    }

    private void FixedUpdate()
    {
        TryExplode();
    }

    private void TryExplode()
    {
        if (GameManager.Instance.GameState != Utilities.GameState.Play) return;
        if(_exploding) return;
        
        _exploding = true;

        if (_explosionType == 0) DetonateCircle(_radius);

        StartCoroutine(ScheduleDestruction(1));
    }

    private IEnumerator ScheduleDestruction(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public void DetonateCircle(float radius)
    {
        Detonating?.Invoke();

        

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, radius);
        HashSet<HealthComponent> uniqueHits = new HashSet<HealthComponent>();
        foreach (Collider2D other in nearby)
        {
            if (_cr.CanDamage(other))
            {
                HealthComponent target = other.GetComponentInParent<HealthComponent>();
                if ((target != null) && uniqueHits.Add(target)) {
                    target.TryTakeDamage(_damage);
                    Rigidbody2D target_rb = other.GetComponent<Rigidbody2D>();
                    if (target_rb != null)
                    {
                        target_rb.AddForce((target_rb.transform.position - transform.position) * _explosionForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

}
