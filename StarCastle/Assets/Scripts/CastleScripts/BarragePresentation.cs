using UnityEngine;
using UnityEngine.Audio;

public class BarragePresentation : MonoBehaviour
{
    private TargetedBarrageBehavior _barrage;
    private DrawTargetCircle _draw;

    [SerializeField] private AudioResource _targetingSound;

    private void Awake()
    {
        _barrage = GetComponent<TargetedBarrageBehavior>();
        _draw = GetComponent<DrawTargetCircle>();
    }

    private void OnEnable()
    {
        _barrage.TargetingStarted += HandleTargetingStarted;
    }

    private void OnDisable()
    {
        _barrage.TargetingStarted -= HandleTargetingStarted;
        _draw.StopDrawing();
    }

    private void HandleTargetingStarted(float duration, float radius)
    {
        if (_targetingSound != null)
        {
            OneshotEmitter.Play(_targetingSound, transform.position, 1f, 1f);
        }
        
        _draw.StartDrawCircle(duration, radius);
    }
}