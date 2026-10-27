using UnityEngine;

public class JumperMissilePresentation : MonoBehaviour
{
    private JumperMissileBehavior _jumper;
    private DrawTargetLine _line;
    private HealthComponent _health;
    
    private AudioSource _aSource;
    [SerializeField] private AudioClip _launchClip;
    [SerializeField] private AudioClip _beepClip;

    void Awake()
    {
        _jumper = GetComponent<JumperMissileBehavior>();
        _line = GetComponent<DrawTargetLine>();
        _health = GetComponent<HealthComponent>();
        _aSource = GetComponent<AudioSource>();

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

    private void HandleTargetingStarted(float duration, Rigidbody2D target)
    {
        _line.enabled = true;
        _line.Initialize(duration, target);
    }

    private void HandleMissileLaunch()
    {
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
        _line.enabled = false;
        _line.Reset();
    }
}