using UnityEngine;

public class ShrapnelComponent : MonoBehaviour
{
    [SerializeField] private BulletBehavior _shrapnel;
    [SerializeField] private int _shrapnel_count = 3;
    [SerializeField] private float _shrapnel_speed = 5f;
    [SerializeField] private float _shrapnel_lifetime = 1f;

    void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _shrapnel_count; i++)
        {
            BulletBehavior current_shrapnel = Instantiate(_shrapnel, transform.position, transform.rotation);
            current_shrapnel.Initialize(Random.onUnitCircle, _shrapnel_lifetime, _shrapnel_speed);
        }

        Destroy(gameObject);
    }
}
