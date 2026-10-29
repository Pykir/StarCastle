using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerPresentation : MonoBehaviour
{
    private PlayerBehavior _player;
    private HealthComponent _health;

    private float _thrust_flash_speed = 0.02f;
    [SerializeField] private SpriteRenderer _thrustSpriteL;
    [SerializeField] private SpriteRenderer _thrustSpriteR;
    [SerializeField] private SpriteRenderer _dashSprite;
    
    private AudioSource _aSource;
    [SerializeField] private AudioClip _fireClip;
    [SerializeField] private AudioClip _hurtClip;
    [SerializeField] private AudioClip _shieldHitClip;

    private float _thrust_elapsed = 0;
    private bool _thrustOn = false;

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

    private void Update()
    {
        if (_player.IsEngineFiring)
        {
            ThrustFlashing();
        } else
        {
            _thrustOn = false;
            _thrustSpriteL.enabled = false;
            _thrustSpriteR.enabled = false;
        }
    }

    private void FixedUpdate()
    {

        if (_player.IsDashing)
        {
            _dashSprite.enabled = true;
            _thrustSpriteL.enabled = false;
            _thrustSpriteR.enabled = false;
        } else
        {
            _dashSprite.enabled = false;
        }
    }

    private void HandleShotFired()
    {
        _aSource.PlayOneShot(_fireClip, Random.Range(0.6f, 0.8f));
    }

    private void ThrustFlashing()
    {
        if (_thrust_elapsed >= _thrust_flash_speed)
        {
            if (_thrustOn)
            {
                _thrustOn = false;
                _thrustSpriteL.enabled = false;
                _thrustSpriteR.enabled = false;
            } else
            {
                _thrustOn = true;
                _thrustSpriteL.enabled = true;
                _thrustSpriteR.enabled = true;
            }
            _thrust_elapsed -= _thrust_flash_speed;
        }
        _thrust_elapsed += Time.deltaTime;
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