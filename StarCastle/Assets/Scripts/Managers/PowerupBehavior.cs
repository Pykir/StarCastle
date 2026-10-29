using UnityEngine;

public class PowerupBehavior : MonoBehaviour
{
    [SerializeField] private int _powerup_type;

    private float _lifetime = 3f;
    private float _elapsed = 0;
    private bool consumed = false;

    public void Initialize(float lifetime)
    {
        _lifetime = lifetime;
        _elapsed = 0;
        consumed = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (consumed) Destroy(gameObject);

        if (_elapsed >= _lifetime) Destroy(gameObject);

        _elapsed += Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        if (other.gameObject.TryGetComponent<PlayerEffects>(out PlayerEffects player))
        {
            consumed = true;
            if (_powerup_type == 0)
            {
                player.AddShield(1);
            }
            if (_powerup_type == 1)
            {
                player.SetDoubleFireTime(5f);
            }
        }
    }
}
