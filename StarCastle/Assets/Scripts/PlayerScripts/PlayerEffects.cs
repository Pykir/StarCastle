using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    private HealthComponent _health;

    private float _firing_multiplier = 1;
    public float FiringMult => _firing_multiplier;

    private float _temp_fire_mult = 1;

    private float _double_fire_time = 0;
    public float RapidFireTimeRemaining => _double_fire_time;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_double_fire_time > 0)
        {
            _temp_fire_mult = 2;
            _double_fire_time -= Time.deltaTime;
        } else
        {
            _temp_fire_mult = 1;
            _double_fire_time = 0;
        }

        _firing_multiplier = 1 * _temp_fire_mult;
    }

    public void AddShield(int amount)
    {
        _health.AddForceFieldCharge(amount);
    }

    public void AddDoubleFireTime(float time)
    {
        _double_fire_time += time;
    }

    public void SetDoubleFireTime(float time)
    {
        _double_fire_time = time;
    }
}
