using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class PlayerPresentation : MonoBehaviour
{
    private PlayerBehavior _player;
    private HealthComponent _health;

    private float _thrust_flash_speed = 0.02f;
    [SerializeField] private SpriteRenderer _thrustSpriteL;
    [SerializeField] private SpriteRenderer _thrustSpriteR;
    [SerializeField] private SpriteRenderer _dashSprite;
    
    [SerializeField] private SpriteRenderer _shieldSprite;

    [SerializeField] private AudioResource _fireSound;
    [SerializeField] private AudioResource _hurtSound;
    [SerializeField] private AudioResource _shieldHitSound;
    [SerializeField] private AudioResource _dashSound;

    private float _thrust_elapsed = 0;
    private bool _thrustOn = false;
    [SerializeField] private bool FOR_MAIN_MENU = false;

    void Awake()
    {
        _player = GetComponent<PlayerBehavior>();
        _health = GetComponent<HealthComponent>();

        _shieldSprite.enabled = _health.ForcefieldCount > 0;
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
        if (FOR_MAIN_MENU)
        {
            ThrustFlashing();
            return;
        }
        _shieldSprite.enabled = _health.ForcefieldCount > 0;
        _dashSprite.enabled = _player.IsDashing;

        if (_player.IsEngineFiring && !_player.IsDashing)
        {
            ThrustFlashing();
            ThrustLoopSound();
        } else
        {
            _thrustOn = false;
            _thrustSpriteL.enabled = false;
            _thrustSpriteR.enabled = false;
        }
    }

    private void HandleShotFired()
    {
        OneshotEmitter.Play(_fireSound, transform.position, 1f, 1f);
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

    private void ThrustLoopSound()
    {
        // figure out engine looping sound
    }

    private void HandleDash()
    {
        OneshotEmitter.Play(_dashSound, transform.position, Random.Range(0.8f, 1f), Random.Range(0.9f, 1.1f));
    }

    private void HandleDashEnd()
    {
        // for stopping particles if they end up existing
    }

    private void HandleHitAccepted(HealthComponent source)
    {
        if (_hurtSound != null)
        {
            OneshotEmitter.Play(_hurtSound, transform.position, Random.Range(0.8f, 1f), Random.Range(0.8f, 1f));
        }
    }

    private void HandleShieldHit(HealthComponent source)
    {
        if (_shieldHitSound != null)
        {
            OneshotEmitter.Play(_shieldHitSound, transform.position, Random.Range(0.8f, 1f), Random.Range(0.8f, 1f));
        }
    }
}
