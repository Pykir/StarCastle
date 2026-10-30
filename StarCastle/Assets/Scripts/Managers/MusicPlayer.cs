using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private AudioSource musicAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.ignoreListenerPause = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
