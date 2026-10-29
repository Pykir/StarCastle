using UnityEngine;

public class BulletGrowth : MonoBehaviour
{

    [SerializeField, Min(0.01f)] private float _duration = 0.12f;

    private Vector3 _fullVisualScale;
    private float _elapsed;

    void Awake()
    {
        _fullVisualScale = transform.localScale;

        transform.localScale = Vector3.zero;
    }

    void FixedUpdate()
    {
        _elapsed += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        transform.localScale = Vector3.Lerp(Vector3.zero, _fullVisualScale, t);

        if (t >= 1f) enabled = false;
    }
}