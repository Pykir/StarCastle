using System.Collections;
using UnityEngine;

public class DamageFeedbackComponent : MonoBehaviour
{
    private HealthComponent _health;
    private SpriteRenderer _renderer;

    [SerializeField] private float _blinkInterval = 0.1f;
    [SerializeField] private Color _hitTint = Color.white;
    [SerializeField] private float _minOpacity = 0.3f;

    private Color _baseColor;
    private float _lastHitTime;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _renderer = GetComponent<SpriteRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _baseColor = _renderer.color;
        _health.Damaged += OnDamage;
    }

    // Update is called once per frame
    void Update()
    {
        Color color = _baseColor;
        float elapsed = Time.time - _lastHitTime;

        if (_health.isInvulnerable)
        {
            color = _hitTint;
            // Finds what part of the wave it should be in
            float phase = elapsed / _blinkInterval;

            // cos period is 2pi, so have to scale phase (0-1)
            // goes between minOpacity and 1
            float wave = _minOpacity + ((1-_minOpacity) * Mathf.Cos(phase * Mathf.PI * 2f));

            color.a = _baseColor.a * Mathf.Lerp(_minOpacity, 1f, wave);
        }

        _renderer.color = color;
    }

    void OnDamage(HealthComponent health)
    {
        _lastHitTime = Time.time;
    }

}
