using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerPresentation : MonoBehaviour
{
    private PlayerBehavior _player;
    private HealthComponent _health;
    
    private AudioSource _aSource;
    [SerializeField] private AudioClip _fireClip;
    [SerializeField] private AudioClip _hurtClip;
    [SerializeField] private AudioClip _shieldHitClip;

    void Awake()
    {
        _player = GetComponent<PlayerBehavior>();
        _health = GetComponent<HealthComponent>();

        _aSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _player.ShotFired += HandleShotFired;
        _player.DashStarted += HandleDash;
        _player.DashEnded += HandleDashEnd;
        
        _health.HitAccepted += HandleHitAccepted;
        _health.ForceFieldAbsorbed += HandleShieldHit;
    }

    private void OnDisable()
    {
        _player.ShotFired -= HandleShotFired;
        _player.DashStarted -= HandleDash;
        _player.DashEnded -= HandleDashEnd;

        _health.HitAccepted -= HandleHitAccepted;
        _health.ForceFieldAbsorbed -= HandleShieldHit;
    }

    private void FixedUpdate()
    {
        if (_player.IsDashing)
        {
            // future dashing visuals here
        }
    }

    private void HandleShotFired()
    {
        _aSource.PlayOneShot(_fireClip, Random.Range(0.6f, 0.8f));
    }

    private void HandleDash()
    {
        // future dash sound here
    }

    private void HandleDashEnd()
    {
        // for stopping particles if they end up existing
    }

    private void HandleHitAccepted(HealthComponent source)
    {
        if (_hurtClip != null)
        {
            _aSource.PlayOneShot(_hurtClip, Random.Range(0.8f, 1f));
        }
    }

    private void HandleShieldHit(HealthComponent source)
    {
        if (_shieldHitClip != null)
        {
            _aSource.PlayOneShot(_shieldHitClip, Random.Range(0.8f, 1f));
        }
    }
}