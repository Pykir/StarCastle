using System.Collections;
using UnityEngine;

public class TargetedBarrageBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _destructionEffect;
    private float _radius = 1f;
    private int _damage = 1;

    public event System.Action<float, float> TargetingStarted;

    public void Initialize(float fuse_time, float attack_radius, int damage)
    {
        _radius = attack_radius;
        _damage = damage;

        TargetingStarted?.Invoke(fuse_time, attack_radius);
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
