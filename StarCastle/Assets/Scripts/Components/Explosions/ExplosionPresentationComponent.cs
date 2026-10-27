using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class ExplosionPresentationComponent : MonoBehaviour
{
    [SerializeField] ExplosionVisuals _visuals;
    ExplosionVisuals _current_visual;
    ExplosionComponent _explosion;
    void Awake()
    {
        _explosion = GetComponent<ExplosionComponent>();
        _current_visual = Instantiate(_visuals, transform.position, transform.rotation);
        _current_visual.SetRadius(_explosion.Radius);
    }

    void OnEnable()
    {
        _explosion.RadiusChanged += HandleRadiusChanged;
        _explosion.Detonating += HandleDetonation;
    }

    void OnDisable()
    {
        _explosion.RadiusChanged -= HandleRadiusChanged;
        _explosion.Detonating -= HandleDetonation;
    }

    void HandleRadiusChanged(float radius)
    {
        _current_visual.SetRadius(radius);
    }

    void HandleDetonation()
    {
        _current_visual.PlayDetonation();
    }
}
