using System.Collections;
using UnityEngine;

public class JumperMissilePresentation : MonoBehaviour
{
    private JumperMissileBehavior _jumper;
    private DrawTargetLine _line;
    [SerializeField] private SpriteRenderer _jumpL;
    [SerializeField] private SpriteRenderer _jumpR;
    private HealthComponent _health;
    
    private AudioSource _aSource;
    [SerializeField] private AudioClip _launchClip;
    [SerializeField] private AudioClip _beepClip;

    private bool _jumping = false;
    private float _targeting_time;
    private float _elapsed_time;
    private Color _start_color;
    [SerializeField] private Color _end_color;

    void Awake()
    {
        _jumper = GetComponent<JumperMissileBehavior>();
        _line = GetComponent<DrawTargetLine>();
        _health = GetComponent<HealthComponent>();
        _aSource = GetComponent<AudioSource>();

        _start_color = _jumpL.color;

        _line.enabled = false;
    }

    private void OnEnable()
    {
        _jumper.TargetingStarted += HandleTargetingStarted;
        _jumper.MissileLaunched += HandleMissileLaunch;
        _line.Flash += HandleBeep;

        _health.WindUpStarted += HandleWindUp;

        if (_health.windingUp)
            HandleWindUp(_health);
    }

    private void OnDisable()
    {
        _line.enabled = false;
        
        _jumper.TargetingStarted -= HandleTargetingStarted;
        _jumper.MissileLaunched -= HandleMissileLaunch;
        _line.Flash -= HandleBeep;

        _health.WindUpStarted -= HandleWindUp;
    }

    void Update()
    {
        if (_jumping)
        {
            Color current_color = Color.Lerp(_start_color, _end_color, Mathf.Min(1.2f * _elapsed_time / Mathf.Max(_targeting_time, 0.01f), 1f));
            _jumpL.color = current_color;
            _jumpR.color = current_color;
            _elapsed_time += Time.deltaTime;
        } else
        {
            _jumpL.color = _start_color;
            _jumpR.color = _start_color;
        }
    }

    private void HandleTargetingStarted(float duration, Rigidbody2D target)
    {
        _line.enabled = true;
        _line.Initialize(duration, target);
        _jumping = true;
        _targeting_time = duration;
        _elapsed_time = 0;
    }

    private void HandleMissileLaunch()
    {
        _jumping = false;
        _aSource.PlayOneShot(_launchClip);
        _line.enabled = false;
        _line.Reset();
    }

    private void HandleBeep(float pitch)
    {
        //_aSource.PlayOneShot(_beepClip);
    }

    void HandleWindUp(HealthComponent source)
    {
        _jumping = false;
        _line.enabled = false;
        _line.Reset();
    }
}