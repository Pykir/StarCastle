using System;
using Unity.VisualScripting;
using UnityEngine;

public class FlashOnWindUp : MonoBehaviour
{
    private HealthComponent _health;
    private SpriteRenderer _renderer;

    [SerializeField] private float _starting_flash_interval = 0.05f;
    [SerializeField] private float _ending_flash_interval = 0.01f;
    [SerializeField] private Color _colorTint = Color.white;
    [SerializeField] private float _minOpacity = 0.3f;

    private float _elapsed_time;
    private float _current_interval;
    private bool flashTint;
    private Color _baseColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _health = GetComponent<HealthComponent>();
        _renderer = GetComponent<SpriteRenderer>();
        _baseColor = _renderer.color;

        _elapsed_time = 0f;
        _current_interval = _starting_flash_interval;
        flashTint = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_health.windingUp)
        {
            _elapsed_time += Time.deltaTime;

            _current_interval = Mathf.Lerp(_starting_flash_interval, _ending_flash_interval, _health._windUpProgress);

            Color color = flashTint ? _colorTint : _baseColor;
            color.a = _baseColor.a;
            _renderer.color = color;

            if (flashTint)
            {
                if (_elapsed_time >= _current_interval)
                {
                    flashTint = false;
                    _elapsed_time -= _current_interval;
                }
            } else
            {
                if (_elapsed_time >= _current_interval)
                {
                    flashTint = true;
                    _elapsed_time -= _current_interval;
                }
            }
        }
    }
}
