using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class OneshotEmitter : MonoBehaviour
{
    public static void Play(AudioResource resource, Vector3 position, float volume, float pitch)
    {
        if (resource == null) return;

        var obj = new GameObject($"SFX: {resource.name}");
        obj.transform.position = position;
        DontDestroyOnLoad(obj);

        var source = obj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = false;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        source.resource = resource;

        var emitter = obj.AddComponent<OneshotEmitter>();

        source.Play();
        emitter.StartCoroutine(emitter.Cleanup(source));
    }

    private IEnumerator Cleanup(AudioSource source)
    {
        yield return null;

        while (AudioListener.pause || source.isPlaying) yield return null;

        Destroy(gameObject);
    }
}