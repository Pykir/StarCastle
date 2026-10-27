using Unity.VisualScripting;
using UnityEngine;

public class ExplosionVisuals : MonoBehaviour
{
    
    [SerializeField] GameObject visual;
    Transform visualTransform;

    private AudioSource _aSource;
    [SerializeField] private AudioClip _explosionClip;

    void Awake()
    {
        visualTransform = visual.transform;
        _aSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRadius (float radius)
    {
        visualTransform.localScale = Vector3.one * radius * 2f;
    }

    public void PlayDetonation()
    {
        _aSource.pitch = Random.Range(0.9f, 1.1f);
        _aSource.volume = Random.Range(0.8f, 1.0f);
        _aSource.clip = _explosionClip;
        _aSource.Play();

        Destroy(gameObject, 1f);
    }
}
