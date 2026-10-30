using UnityEngine;
using UnityEngine.Audio;

public class PeewMissilePresentation : MonoBehaviour
{
    private PeewMissileBehavior _dart;
    private DrawTargetLine _line;

    [SerializeField] private AudioResource _launchSound;
    [SerializeField] private AudioResource _beepSound;

    void Awake()
    {
        _dart = GetComponent<PeewMissileBehavior>();
        _line = GetComponent<DrawTargetLine>();
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
        OneshotEmitter.Play(_launchSound, transform.position, 1f, Random.Range(0.9f, 1.1f));
    }

    private void HandleBeep(float pitch)
    {
        OneshotEmitter.Play(_beepSound, transform.position, 0.3f, pitch);
    }
}