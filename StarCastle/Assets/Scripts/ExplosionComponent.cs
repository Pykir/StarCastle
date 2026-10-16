using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ExplosionComponent : MonoBehaviour
{
    [SerializeField] private ContactRules _cr;

    [SerializeField] private int _damage = 1;
    [SerializeField] private float _radius = 1;
    [SerializeField] private float _explosionForce = 5;
    [SerializeField] private int explosionType = 0;
    
    private bool _exploding = false;

    public void Awake()
    {
        _cr = GetComponent<ContactRules>();
        GetComponentInChildren<SpriteRenderer>().transform.localScale = Vector3.one * _radius * 2f;
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

        if (explosionType == 0) DetonateCircle(_radius);

        StartCoroutine(ScheduleDestruction(1));
    }

    private IEnumerator ScheduleDestruction(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    public void DetonateCircle(float radius)
    {
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
