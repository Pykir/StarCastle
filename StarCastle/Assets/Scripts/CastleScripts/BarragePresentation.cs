using UnityEngine;

public class BarragePresentation : MonoBehaviour
{
    private TargetedBarrageBehavior _barrage;
    private DrawTargetCircle _draw;

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
        _draw.StartDrawCircle(duration, radius);
    }
}