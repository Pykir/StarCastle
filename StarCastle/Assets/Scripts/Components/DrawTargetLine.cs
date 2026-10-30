using System;
using UnityEngine;

public class DrawTargetLine : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;

    [SerializeField] private float _total_flashing_time;
    [SerializeField] private float _starting_solid_time;
    [SerializeField] private float _starting_blink_time;
    [SerializeField] private float _ending_solid_time;
    [SerializeField] private float _ending_blink_time;
    [SerializeField] private float _length = 100f;
    
    private float _current_solid_time;
    private float _current_blink_time;
    private float _elapsed_time;
    private float _total_time;
    private bool flashOn;

    public Action<float> Flash;

    public void Initialize(float time)
    {
        _total_flashing_time = time;
        StartDrawLine(_length);
    }

    public void Initialize(float time, Rigidbody2D target)
    {
        _total_flashing_time = time;
        _length = Vector2.Distance(transform.position, target.transform.position);
        StartDrawLine(_length);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Reset();
    }

    public void Reset()
    {
        _elapsed_time = 0;
        _total_time = 0;
        flashOn = true;
        _current_solid_time = _starting_solid_time;
        _current_blink_time = _starting_blink_time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_total_time >= _total_flashing_time)
        {
            flashOn = false;
            _line.enabled = false;
            return;
        } 

        _elapsed_time += Time.deltaTime;
        _total_time += Time.deltaTime;

        if (flashOn)
        {
            _line.enabled = true;
            if (_elapsed_time >= _current_solid_time)
            {
                flashOn = false;
                _elapsed_time -= _current_solid_time;
            }
        } else
        {
            _line.enabled = false;
            if (_elapsed_time >= _current_blink_time)
            {
                float progress = Mathf.Clamp01(_total_time / Mathf.Max(_total_flashing_time, 0.001f));
                float pitch = Mathf.Lerp(1f, 1.5f, progress);
                Flash?.Invoke(pitch);
                flashOn = true;
                _elapsed_time -= _current_blink_time;
            }
        }
        
        _current_solid_time = Mathf.Lerp(_starting_solid_time, _ending_solid_time, _total_time / _total_flashing_time);
        _current_blink_time = Mathf.Lerp(_starting_blink_time, _ending_blink_time, _total_time / _total_flashing_time);

        StartDrawLine(_length);
        
    }

    void StartDrawLine(float length)
    {
        _line.positionCount = 2;
        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, transform.position + transform.right * length);
    }

    void OnDisable()
    {
        _line.enabled = false;
    }

    void OnEnable()
    {
        _line.enabled = true;
    }
}
