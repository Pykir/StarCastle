using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class ExplosionVisuals : MonoBehaviour
{
    [SerializeField] private AudioResource _explosionSound;
    

    public void SetRadius (float radius)
    {
        transform.localScale = Vector3.one * radius * 2f;
    }

    public void PlayDetonation()
    {
        if (_explosionSound != null)
        {
            OneshotEmitter.Play(_explosionSound, transform.position, Random.Range(0.8f, 1.0f), Random.Range(0.9f, 1.1f));
        }

        Destroy(gameObject, 1f);
    }
}
