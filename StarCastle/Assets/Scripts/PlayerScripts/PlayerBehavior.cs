using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBehavior : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction fireAction;
    private InputAction dashAction;

    private Rigidbody2D _rb;
    private ContactRules _cr;
    [SerializeField] private Transform _firing_transform;
    private PlayerStats _stats;

    [HideInInspector] public HealthComponent _health {get; set;}

    [SerializeField] private BulletBehavior _bullet;

    private int _current_dash_charges = 1;

    private bool throttle_on = false;
    public bool IsEngineFiring => throttle_on;
    private bool braking = false;
    public bool IsBraking => braking;

    private float _fireTimer = 0f;

    private bool _dashRequested = false;
    private bool dashing = false;
    public bool IsDashing => dashing;
    private float _dash_duration = 0f;
    private float _dash_acc_duration;
    private float _dash_cruise_duration;
    private float _dash_dec_duration;
    private Vector2 _dash_direction;
    private Vector2 _dash_lateral_direction;
    private float _dash_acceleration;
    private float _dash_deceleration;
    private float _dash_lateral_acceleration_limit;
    private float _dashTimer = 0f;
    
    public event Action ShotFired;
    public event Action DashStarted;
    public event Action DashEnded;
    
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        dashAction = InputSystem.actions.FindAction("Dash");

        _rb = GetComponent<Rigidbody2D>();
        _cr = GetComponent<ContactRules>();
        _health = GetComponent<HealthComponent>();
        _stats = GetComponent<PlayerStats>();
    }

    public void InitializeStats(PlayerStatsTracker runStats)
    {
        _stats.Initialize(runStats);
        _health.InitializeHealth(_stats.EffectiveMaxHealth);

        _current_dash_charges = _stats.EffectiveDashMaxCharges;
        _fireTimer = _stats.EffectiveFireCD;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameState != Utilities.GameState.Play) return;

        if (dashAction.WasPressedThisFrame())
        {
            _dashRequested = true;
        }
    }
    void FixedUpdate()
    {
        // dash code
        if (dashing && (_dashTimer >= _dash_duration || Mathf.Approximately(_dashTimer, _dash_duration))) {
            EndDash();
        }

        if (_dashRequested)
        {
            _dashRequested = false;
            if (!dashing)
            {
                StartDash();
            }
        }

        if (dashing) 
        {
            if (_dashTimer < _dash_acc_duration &&
                !Mathf.Approximately(_dashTimer, _dash_acc_duration))
            {
                ApplyDash(1);
            } else if (_dashTimer < (_dash_acc_duration + _dash_cruise_duration) &&
                !Mathf.Approximately(_dashTimer, _dash_acc_duration + _dash_cruise_duration))
            {
                ApplyDash(2);
            } else
            {                    
                ApplyDash(3);
            }
                
            _dashTimer += Time.fixedDeltaTime;
        } 
        else // when not dashing, normal movement, allow firing
        {
            Vector2 movement_input = moveAction.ReadValue<Vector2>();
            ApplyTurning(movement_input.x);
            ApplyMovement(movement_input.y);

            if(fireAction.IsPressed())
            {
                if (_fireTimer >= _stats.EffectiveFireCD)
                {
                    StartCoroutine(FireBullets(_stats.EffectiveBurstCount));
                    _fireTimer = 0;
                }
            }
        }
        _fireTimer += Time.fixedDeltaTime;
    }

    private void ApplyTurning(float turn_direction)
    {
        float maxTurnAcceleration = _stats.EffectiveMaxTurnSpeed;

        if (turn_direction == 0f)
        {
            maxTurnAcceleration /= Mathf.Max(_stats.EffectiveSecondsToStopTurning, 0.01f);
        } else
        {
            maxTurnAcceleration /= Mathf.Max(_stats.EffectiveSecondsToMaxTurnSpeed, 0.01f);
        }
        
        float remainingTurnAcceleration = ((-turn_direction * _stats.EffectiveMaxTurnSpeed) - _rb.angularVelocity) / Time.fixedDeltaTime;

        float turnTorque = Mathf.Clamp(remainingTurnAcceleration, -maxTurnAcceleration, maxTurnAcceleration) * Mathf.Deg2Rad * _rb.inertia;

        _rb.AddTorque(turnTorque, ForceMode2D.Force);
    }

    private void ApplyMovement(float throttle)
    {
        Vector2 accelerationToAdd = Vector2.zero;
        
        // acceleration code
        if (Math.Sign(throttle) > 0)
        {
            throttle_on = true;
            accelerationToAdd = (transform.up * CalculateForwardAcceleration()) + (transform.right * CalculateLateralFriction(transform.right));
        } else
        {
            throttle_on = false;
        }

        // active braking
        if (Math.Sign(throttle) < 0)
        {   
            braking = true;
            accelerationToAdd = -_rb.linearVelocity.normalized * CalculateBraking(_stats.EffectiveSecondsToStop);
        } else
        {
            braking = false;
        }

        // if no input, constant friction to slow down
        if (throttle == 0)
        {
            if (_rb.linearVelocity.magnitude > 0f)
            {
                accelerationToAdd = -_rb.linearVelocity.normalized * CalculateBraking(_stats.EffectiveCoastToStopTime);
            }    
        }
        
        _rb.AddForce((accelerationToAdd + CalculateOverspeedBraking()) * _rb.mass, ForceMode2D.Force);
    }

    private float CalculateForwardAcceleration()
    {
        float maxForwardAcceleration = _stats.EffectiveMaxSpeed / Mathf.Max(_stats.EffectiveSecondsToMaxSpeed, 0.01f);

        // computes how much speed is needed between current forward speed and max forward speed
        float remainingForwardAcceleration = Mathf.Max(0f, _stats.EffectiveMaxSpeed - Vector2.Dot(_rb.linearVelocity, transform.up)) / Time.fixedDeltaTime;

        // locks thrust behind speed limit - this only enforces maximum speed forward
        // The ship can be moving faster than limit due to drift and collisions
        return Mathf.Min(maxForwardAcceleration, remainingForwardAcceleration);
    }
    private float CalculateLateralFriction(Vector2 lateralDirection)
    {
        // this makes sure the sideways drift is slowed down during thrust still
        float maxLateralDeceleration = _stats.EffectiveMaxSpeed / Mathf.Max(_stats.EffectiveCoastToStopTime, 0.01f);

        return Mathf.Clamp(-Vector2.Dot(_rb.linearVelocity, lateralDirection) / Time.fixedDeltaTime, -maxLateralDeceleration, maxLateralDeceleration);
    }

    private float CalculateBraking(float stopping_time)
    {
        float deceleration = _stats.EffectiveMaxSpeed / Mathf.Max(stopping_time, 0.01f);
        return Mathf.Min(deceleration, _rb.linearVelocity.magnitude / Time.fixedDeltaTime);
    }

    private Vector2 CalculateOverspeedBraking()
    {
        float speed = _rb.linearVelocity.magnitude;
        float excessSpeed = speed - _stats.EffectiveMaxSpeed;

        if (excessSpeed <= 0f) return Vector2.zero;

        float maxDeceleration =
            _stats.EffectiveMaxSpeed / Mathf.Max(_stats.EffectiveOverspeedRecoveryTime, 0.01f);

        float deceleration = Mathf.Min(
            maxDeceleration,
            excessSpeed / Time.fixedDeltaTime);

        return -_rb.linearVelocity.normalized * deceleration;
    }

    private void StartDash()
    {
        if (_current_dash_charges <= 0) return;
        
        dashing = true;
        _dashTimer = 0f;
        float dash_distance = _stats.EffectiveDashDistance;
        _dash_acc_duration = _stats.EffectiveDashAccelerationDuration;
        _dash_cruise_duration = _stats.EffectiveDashCruiseDuration;
        _dash_dec_duration = _stats.EffectiveDashDecelerationDuration;
        _dash_duration = _dash_acc_duration + _dash_cruise_duration + _dash_dec_duration;

        _dash_direction = transform.up;
        _dash_lateral_direction = Vector2.Perpendicular(-_dash_direction);
        
        float sidewaysSpeed = Vector2.Dot(_rb.linearVelocity, _dash_lateral_direction);
        _dash_lateral_acceleration_limit = Mathf.Abs(sidewaysSpeed) / Mathf.Max((_dash_duration / 4), 0.01f);
        float _dash_exit_speed = _stats.EffectiveMaxSpeed * 2;
        
        if(_current_dash_charges >= _stats.EffectiveDashMaxCharges)
        {
            StartCoroutine(DashRecharge());
        }
        _current_dash_charges--;

        // calculating cruise speed here
        // since we are ramping up from start speed to cruise speed over the starting time
        // it is equivalent to moving the first half of the time at start speed and then jumping up to cruise speed for the second half
        // same for ending section
        float half_dash_acc_duration = _dash_acc_duration / 2;
        float half_dash_dec_duration = _dash_dec_duration / 2;
        float start_speed = Vector2.Dot(_rb.linearVelocity, _dash_direction);
        float start_speed_half_distance = start_speed * half_dash_acc_duration;
        float end_speed_half_distance = _dash_exit_speed * half_dash_dec_duration;
        float cruise_speed_time = Mathf.Max(_dash_cruise_duration + half_dash_acc_duration + half_dash_dec_duration, 0.01f);
        // if we subtract the distance covered by the slow halves of the time, and then divide it over the total time that we pretend to be moving at cruise speed
        // we get the cruise speed itself.
        float cruise_speed = (dash_distance - start_speed_half_distance - end_speed_half_distance) / cruise_speed_time;
        _dash_acceleration = (cruise_speed - start_speed) / Mathf.Max(_dash_acc_duration, 0.01f);
        _dash_deceleration = (cruise_speed - _dash_exit_speed) / Mathf.Max(_dash_dec_duration, 0.01f);

        DashStarted?.Invoke();
    }

    private void ApplyDash(int phase)
    {
        float acceleration = phase switch
        {
            <= 1 => _dash_acceleration,
            <= 2 => 0f,
            <= 3 => -_dash_deceleration,
            _ => 0f
        };

        _rb.AddForce(_dash_direction * acceleration * _rb.mass, ForceMode2D.Force);

        float sidewaysSpeed = Vector2.Dot(_rb.linearVelocity, _dash_lateral_direction);
        float lateralAcceleration = Mathf.Clamp(-sidewaysSpeed / Time.fixedDeltaTime, -_dash_lateral_acceleration_limit, _dash_lateral_acceleration_limit);
        _rb.AddForce(_dash_lateral_direction * lateralAcceleration * _rb.mass, ForceMode2D.Force);
    }

    private void EndDash()
    {
        if (!dashing) return;

        dashing = false;
        DashEnded?.Invoke();
    }

    private IEnumerator DashRecharge()
    {
        yield return new WaitForSeconds(_stats.EffectiveDashRechargeTime);
        
        _current_dash_charges++;
        if(_current_dash_charges < _stats.EffectiveDashMaxCharges)
        {
            StartCoroutine(DashRecharge());
        } else
        {
            
        }
    }

    private IEnumerator FireBullets(int number_of_bullets)
    {   
        for(int i = 0; i < number_of_bullets; i++)
        {
            ShotFired?.Invoke();

            BulletBehavior newBullet = Instantiate(_bullet, _firing_transform.position, transform.rotation);
            newBullet.Initialize(transform.up.normalized, _stats.EffectiveBulletLifetime, _stats.EffectiveBulletSpeed, _stats.EffectiveBulletDamage);
            yield return new WaitForSeconds(_stats.EffectiveBurstCD);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (_cr.CanDamage(other))
        {
            if (dashing)
            {
                HealthComponent target = other.GetComponentInParent<HealthComponent>();
                if (target != null) target.TryTakeDamage(1);
                EndDash();
            }
        }

        if (_cr.CostsDurability(other))
        {
            _health.TryTakeDamage(1);
        }
    }
}
