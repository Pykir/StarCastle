using UnityEngine;

public class ProximityFuse : MonoBehaviour
{
    private Rigidbody2D _rb;
    private ContactRules _cr;
    private HealthComponent _health;

   [SerializeField] private float _triggerRadius = 1f;
   private bool _triggered = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cr = GetComponent<ContactRules>();
        _health = GetComponent<HealthComponent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_triggered) return;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, _triggerRadius);

        foreach (Collider2D other in nearby)
        {
            if (other.attachedRigidbody == _rb) continue;
            if (!_cr.CanDamage(other)) continue;

            _triggered = true;
            _health.Kill();
            return;
        }
    }
}
