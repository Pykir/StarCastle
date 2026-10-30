using UnityEngine;
using UnityEngine.Audio;

public class ShrapnelComponent : MonoBehaviour
{
    [SerializeField] private BulletBehavior _shrapnel;
    [SerializeField] private int _shrapnel_count = 3;
    [SerializeField] private float _shrapnel_speed = 5f;
    [SerializeField] private float _shrapnel_lifetime = 1f;

    [SerializeField] private AudioResource _shrapnelExplosionSound;

    void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OneshotEmitter.Play(_shrapnelExplosionSound, transform.position, 1f, 1f);
        for (int i = 0; i < _shrapnel_count; i++)
        {
            BulletBehavior current_shrapnel = Instantiate(_shrapnel, transform.position, transform.rotation);
            current_shrapnel.Initialize(Random.onUnitCircle, _shrapnel_lifetime, _shrapnel_speed);
        }

        Destroy(gameObject);
    }
}
