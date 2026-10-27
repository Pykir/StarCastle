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
    private PlayerEffects _player_effectors;

    private AudioSource _aSource;
    [SerializeField] private AudioClip _shootClip;

    [HideInInspector] public HealthComponent _health {get; set;}

    [SerializeField] private float _max_speed = 4.0f;
    [SerializeField, Min(0f)] private float _seconds_to_max_speed = 0.4f;
    [SerializeField, Min(0f)] private float _coast_to_stop_time = 2.0f;
    [SerializeField, Min(0f)] private float _seconds_to_stop = 1.0f;
    [SerializeField] private float _max_turn_speed = 180f;
    [SerializeField, Min(0f)] private float _seconds_to_max_turn_speed = 0.1f;
    [SerializeField, Min(0f)] private float _seconds_to_stop_turning = 0.1f;
    [SerializeField, Min(0f)] private float _overspeed_recovery_time = 0.4f;

    [SerializeField] private BulletBehavior _bullet;
    [SerializeField, Min(0)] private int _bullets_per_fire = 3;
    [SerializeField, Min(0f)] private float _fire_cd = 2.0f;

    [SerializeField, Min(0f)] private float _bullet_cd = 0.05f;
    [SerializeField, Min(0.01f)] private float _bullet_lifetime = 3.0f;
    [SerializeField] private float _bullet_speed = 8.0f;

    [SerializeField] private float _dash_distance = 5f;
    [SerializeField, Min(0.01f)] private float _dash_acceleration_duration = 0.06f;
    [SerializeField, Min(0f)] private float _dash_cruise_duration = 0.12f;
    [SerializeField, Min(0.01f)] private float _dash_deceleration_duration = 0.12f;
    [SerializeField, Min(0)] private int _dash_max_charges = 1;
    [SerializeField, Min(0f)] private float _dash_recharge_time = 3.0f;
    private int _current_dash_charges = 1;

    private bool player_can_shoot = true;
    private float _fireTimer = 0f;

    private bool player_can_dash = true;
    private bool _dashRequested = false;
    private bool dashing = false;
    private float _dash_duration = 0f;
    private Vector2 _dash_direction;
    private Vector2 _dash_lateral_direction;
    private float _dash_acceleration;
    private float _dash_deceleration;
    private float _dash_lateral_acceleration_limit;
    private float _dashTimer = 0f;
    
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");
        dashAction = InputSystem.actions.FindAction("Dash");

        _rb = GetComponent<Rigidbody2D>();
        _cr = GetComponent<ContactRules>();
        _health = GetComponent<HealthComponent>();
        _aSource = GetComponent<AudioSource>();
        _player_effectors = GetComponent<PlayerEffects>();
        _current_dash_charges = _dash_max_charges;
        _fireTimer = _fire_cd;
    }

    // Update is called once per frame
    void Update()
    {
        if (dashAction.WasPressedThisFrame())
        {
            _dashRequested = true;
        }
    }
    void FixedUpdate()
    {
        // dash code
        if (dashing && (_dashTimer >= _dash_duration || Mathf.Approximately(_dashTimer, _dash_duration))) {
            dashing = false;
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
            if (_dashTimer < _dash_acceleration_duration && !Mathf.Approximately(_dashTimer, _dash_acceleration_duration)) 
            {
                ApplyDash(1);
            } else if (_dashTimer < (_dash_acceleration_duration + _dash_cruise_duration) && !Mathf.Approximately(_dashTimer, _dash_acceleration_duration + _dash_cruise_duration))                {
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
                if (_fireTimer >= _fire_cd / _player_effectors.FiringMult)
                {
                    StartCoroutine(FireBullets(_bullets_per_fire));
                    _fireTimer = 0;
                }
            }
        }
        _fireTimer += Time.fixedDeltaTime;
    }

    private void ApplyTurning(float turn_direction)
    {
        float maxTurnAcceleration = _max_turn_speed;

        if (turn_direction == 0f)
        {
            maxTurnAcceleration /= Mathf.Max(_seconds_to_stop_turning, 0.01f);
        } else
        {
            maxTurnAcceleration /= Mathf.Max(_seconds_to_max_turn_speed, 0.01f);
        }
        
        float remainingTurnAcceleration = ((-turn_direction * _max_turn_speed) - _rb.angularVelocity) / Time.fixedDeltaTime;

        float turnTorque = Mathf.Clamp(remainingTurnAcceleration, -maxTurnAcceleration, maxTurnAcceleration) * Mathf.Deg2Rad * _rb.inertia;

        _rb.AddTorque(turnTorque, ForceMode2D.Force);
    }

    private void ApplyMovement(float throttle)
    {
        Vector2 accelerationToAdd = Vector2.zero;
        
        // acceleration code
        if (Math.Sign(throttle) > 0)
        {
            accelerationToAdd = (transform.up * CalculateForwardAcceleration()) + (transform.right * CalculateLateralFriction(transform.right));
        }

        // active braking
        if (Math.Sign(throttle) < 0)
        {   
            accelerationToAdd = -_rb.linearVelocity.normalized * CalculateBraking(_seconds_to_stop);
        }

        // if no input, constant friction to slow down
        if (throttle == 0)
        {
            if (_rb.linearVelocity.magnitude > 0f)
            {
                accelerationToAdd = -_rb.linearVelocity.normalized * CalculateBraking(_coast_to_stop_time);
            }    
        }
        
        _rb.AddForce((accelerationToAdd + CalculateOverspeedBraking()) * _rb.mass, ForceMode2D.Force);
    }

    private float CalculateForwardAcceleration()
    {
        float maxForwardAcceleration = _max_speed / Mathf.Max(_seconds_to_max_speed, 0.01f);

        // computes how much speed is needed between current forward speed and max forward speed
        float remainingForwardAcceleration = Mathf.Max(0f, _max_speed - Vector2.Dot(_rb.linearVelocity, transform.up)) / Time.fixedDeltaTime;

        // locks thrust behind speed limit - this only enforces maximum speed forward
        // The ship can be moving faster than limit due to drift and collisions
        return Mathf.Min(maxForwardAcceleration, remainingForwardAcceleration);
    }
    private float CalculateLateralFriction(Vector2 lateralDirection)
    {
        // this makes sure the sideways drift is slowed down during thrust still
        float maxLateralDeceleration = _max_speed / Mathf.Max(_coast_to_stop_time, 0.01f);

        return Mathf.Clamp(-Vector2.Dot(_rb.linearVelocity, lateralDirection) / Time.fixedDeltaTime, -maxLateralDeceleration, maxLateralDeceleration);
    }

    private float CalculateBraking(float stopping_time)
    {
        float deceleration = _max_speed / Mathf.Max(stopping_time, 0.01f);
        return Mathf.Min(deceleration, _rb.linearVelocity.magnitude / Time.fixedDeltaTime);
    }

    private Vector2 CalculateOverspeedBraking()
    {
        float speed = _rb.linearVelocity.magnitude;
        float excessSpeed = speed - _max_speed;

        if (excessSpeed <= 0f) return Vector2.zero;

        float maxDeceleration =
            _max_speed / Mathf.Max(_overspeed_recovery_time, 0.01f);

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
        _dash_duration = _dash_acceleration_duration + _dash_cruise_duration + _dash_deceleration_duration;

        _dash_direction = transform.up;
        _dash_lateral_direction = Vector2.Perpendicular(-_dash_direction);
        
        float sidewaysSpeed = Vector2.Dot(_rb.linearVelocity, _dash_lateral_direction);
        _dash_lateral_acceleration_limit = Mathf.Abs(sidewaysSpeed) / Mathf.Max((_dash_duration / 4), 0.01f);
        float _dash_exit_speed = _max_speed * 2;
        
        if(_current_dash_charges >= _dash_max_charges)
        {
            StartCoroutine(DashRecharge());
        }
        _current_dash_charges--;

        float start_speed = Vector2.Dot(_rb.linearVelocity, _dash_direction);
        float peak_speed = (_dash_distance - (0.5f * start_speed * _dash_acceleration_duration) - (0.5f * _dash_exit_speed * _dash_deceleration_duration)) / Mathf.Max((_dash_cruise_duration + (0.5f * (_dash_acceleration_duration + _dash_deceleration_duration))), 0.01f);
        _dash_acceleration = (peak_speed - start_speed) / Mathf.Max(_dash_acceleration_duration, 0.01f);
        _dash_deceleration = (peak_speed - _dash_exit_speed) / Mathf.Max(_dash_deceleration_duration, 0.01f);
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

    private IEnumerator DashRecharge()
    {
        yield return new WaitForSeconds(_dash_recharge_time);
        
        _current_dash_charges++;
        if(_current_dash_charges < _dash_max_charges)
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
            _aSource.PlayOneShot(_shootClip, Random.Range(0.6f, 0.8f));

            BulletBehavior newBullet = Instantiate(_bullet, _firing_transform.position, transform.rotation);
            newBullet.Initialize(transform.up.normalized, _bullet_lifetime, _bullet_speed);
            yield return new WaitForSeconds(_bullet_cd / _player_effectors.FiringMult);
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
                dashing = false;
            }
        }

        if (_cr.CostsDurability(other))
        {
            _health.TryTakeDamage(1);
        }
    }
}
