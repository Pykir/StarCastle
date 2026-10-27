using UnityEngine;

public class PeewMissilePresentation : MonoBehaviour
{
    private PeewMissileBehavior _dart;
    private DrawTargetLine _line;
    private AudioSource _aSource;
    [SerializeField] private AudioClip _launchClip;
    [SerializeField] private AudioClip _beepClip;

    void Awake()
    {
        _dart = GetComponent<PeewMissileBehavior>();
        _line = GetComponent<DrawTargetLine>();
        _aSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _dart.TargetingStarted += HandleTargetingStarted;
        _dart.MissileLaunched += HandleMissileLaunch;
        _line.Flash += HandleBeep;
    }

    private void OnDisable()
    {
        _line.enabled = false;
        
        _dart.TargetingStarted -= HandleTargetingStarted;
        _dart.MissileLaunched -= HandleMissileLaunch;
        _line.Flash -= HandleBeep;
    }

    private void HandleTargetingStarted(float duration)
    {
        _line.Initialize(duration);
    }

    private void HandleMissileLaunch()
    {
        _aSource.PlayOneShot(_launchClip);
    }

    private void HandleBeep(float pitch)
    {
        //_aSource.PlayOneShot(_beepClip);
    }
}