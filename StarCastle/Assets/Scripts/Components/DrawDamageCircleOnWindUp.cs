using UnityEngine;

public class DrawDamageCircleOnWindUp : MonoBehaviour
{
    [SerializeField] private LineRenderer _boundary;
    [SerializeField] private SpriteRenderer _progress;
    private HealthComponent _health;
    private MissileBehavior _missile;

    [SerializeField, Min(4)] private int _steps = 48;
    private Vector3[] _points;
    private float _maxScale;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _missile = GetComponent<MissileBehavior>();

        if (_boundary == null || _progress == null)
        {
            Debug.LogError("Assign both warning circle renderers.", this);
            enabled = false;
            return;
        }

        _maxScale = (2f * _missile.ExplosionRadius) / (_missile.transform.lossyScale.x * _progress.sprite.bounds.size.x);
        _points = new Vector3[_steps];

        _boundary.useWorldSpace = true;
        _boundary.loop = true;
        _boundary.positionCount = _steps;
        _boundary.enabled = false;
        _progress.enabled = false;
    }
    
    void LateUpdate()
    {
        _boundary.enabled = _health.windingUp;
        _progress.enabled = _health.windingUp;

        if (!_health.windingUp) return;

        DrawCircle(_boundary, _missile.ExplosionRadius);

        if (_progress.enabled)
            _progress.transform.localScale = Vector3.one * Mathf.Lerp(0f, _maxScale, _health._windUpProgress);
    }

    private void DrawCircle(LineRenderer line, float radius)
    {
        Vector3 center = transform.position;

        for (int i = 0; i < _steps; i++)
        {
            float angle = i * Mathf.PI * 2f / _steps;

            _points[i] = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        line.SetPositions(_points);
    }

    void OnDisable()
    {
        if (_boundary != null) _boundary.enabled = false;
        if (_progress != null) _progress.enabled = false;
    }
}
