using System.Collections;
using UnityEngine;

public class TargetedBarrageBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _destructionEffect;
    private DrawTargetCircle _draw;
    private float _radius = 1f;
    private int _damage = 1;

    void Awake()
    {
        _draw = GetComponent<DrawTargetCircle>();
    }

    public void Initialize(float fuse_time, float attack_radius, int damage)
    {
        _draw.StartDrawCircle(fuse_time, attack_radius);

        _radius = attack_radius;
        _damage = damage;
        StartCoroutine(Attack(fuse_time));
    }

    private IEnumerator Attack(float delay_time)
    {
        yield return new WaitForSeconds(delay_time);

        if (_destructionEffect) {
            GameObject effect = Instantiate(_destructionEffect, transform.position, transform.rotation);
            effect.GetComponent<ExplosionComponent>().Initialize(_radius, _damage);
        }
        
        Destroy(gameObject);
    }
}
