using Unity.VisualScripting;
using UnityEngine;

public class CastlePresentation : MonoBehaviour
{
    private CastleBehavior _castle;
    private AudioSource _aSource;

    [SerializeField] AudioClip missileLaunchClip;
    [SerializeField] AudioClip cannonChargeClip;
    [SerializeField] AudioClip cannonFireClip;

    void Awake()
    {
        _castle = GetComponent<CastleBehavior>();
        _aSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        _castle.CannonCharging += HandleCannonCharging;
        _castle.CannonFired += HandleCannonFired;
        _castle.MissileFired += HandleMissileLaunch;
    }

    void OnDisable()
    {
        _castle.CannonCharging -= HandleCannonCharging;
        _castle.CannonFired -= HandleCannonFired;
        _castle.MissileFired -= HandleMissileLaunch;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HandleMissileLaunch()
    {
        _aSource.PlayOneShot(missileLaunchClip);
    }

    void HandleCannonCharging()
    {
        //_aSource.PlayOneShot(cannonChargeClip);
    }

    void HandleCannonFired()
    {
        _aSource.PlayOneShot(cannonFireClip);
    }
}
