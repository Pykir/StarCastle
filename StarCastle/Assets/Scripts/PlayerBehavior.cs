using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBehavior : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction fireAction;

    private Rigidbody2D _rb;
    private ContactRules _cr;
    private AudioSource _aSource;
    [SerializeField] private AudioClip _shootClip;

    [HideInInspector] public HealthComponent _health;

    [SerializeField] private float _max_speed = 4.0f;
    [SerializeField] private float _seconds_to_max_speed = 0.4f;
    [SerializeField] private float _coast_to_stop_time = 2.0f;
    [SerializeField] private float _seconds_to_stop = 1.0f;
    [SerializeField] private float _max_turn_speed = 180f;
    [SerializeField] private float _seconds_to_max_turn_speed = 0.1f;
    [SerializeField] private float _seconds_to_stop_turning = 0.1f;

    [SerializeField] private BulletBehavior _bullet;
    [SerializeField] private int _bullets_per_fire = 3;
    [SerializeField] private float _fire_cd = 2.0f;

    [SerializeField] private float _bullet_cd = 0.05f;
    [SerializeField] private float _bullet_lifetime = 3.0f;
    [SerializeField] private float _bullet_speed = 8.0f;

    private bool player_can_shoot = true;
    
    void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        fireAction = InputSystem.actions.FindAction("Attack");

        _rb = GetComponent<Rigidbody2D>();
        _cr = GetComponent<ContactRules>();
        _health = GetComponent<HealthComponent>();
        _aSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float throttle = moveAction.ReadValue<Vector2>().y;
        float turn_direction = moveAction.ReadValue<Vector2>().x;

        // turning
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

        Vector2 accelerationToAdd = Vector2.zero;
        // acceleration code
        if (Math.Sign(throttle) > 0)
        {
            float maxForwardAcceleration = _max_speed / Mathf.Max(_seconds_to_max_speed, 0.01f);

            // computes how much speed is needed between current forward speed and max forward speed
            float remainingForwardAcceleration = Mathf.Max(0f, _max_speed - Vector2.Dot(_rb.linearVelocity, transform.up)) / Time.fixedDeltaTime;

            // locks thrust behind speed limit - this only enforces maximum speed forward
            // The ship can be moving faster than limit due to drift and collisions
            float finalForwardAcceleration = Mathf.Min(maxForwardAcceleration, remainingForwardAcceleration);

            // this makes sure the sideways drift is slowed down during thrust still
            float maxLateralDeceleration = _max_speed / Mathf.Max(_coast_to_stop_time, 0.01f);

            float remainingLateralDeceleration = Mathf.Clamp(-Vector2.Dot(_rb.linearVelocity, transform.right) / Time.fixedDeltaTime, -maxLateralDeceleration, maxLateralDeceleration);

            accelerationToAdd = (transform.up * finalForwardAcceleration) + (transform.right * remainingLateralDeceleration);
        }

        // active braking
        if (Math.Sign(throttle) < 0)
        {
            float deceleration = _max_speed / Mathf.Max(_seconds_to_stop, 0.01f);
            float remainingBraking = Mathf.Min(deceleration, _rb.linearVelocity.magnitude / Time.fixedDeltaTime);
            accelerationToAdd = -_rb.linearVelocity.normalized * remainingBraking;
        }

        // if no input, constant friction to slow down
        if (throttle == 0)
        {
            if (_rb.linearVelocity.magnitude > 0f)
            {
                float deceleration = _max_speed / Mathf.Max(_coast_to_stop_time, 0.01f);
                float remainingFriction = Mathf.Min(deceleration, _rb.linearVelocity.magnitude / Time.fixedDeltaTime);
                accelerationToAdd = -_rb.linearVelocity.normalized * remainingFriction;
            }    
        }
        
        _rb.AddForce(accelerationToAdd * _rb.mass, ForceMode2D.Force);

        // firing
        if(fireAction.IsPressed())
        {
            if (player_can_shoot)
            {
                StartCoroutine(FireBullets(_bullets_per_fire));
                StartCoroutine(FireCooldown());
            }
        }
    }

    private IEnumerator FireCooldown()
    {
        player_can_shoot = false;

        yield return new WaitForSeconds(_fire_cd);

        player_can_shoot = true;
    }

    private IEnumerator FireBullets(int number_of_bullets)
    {   
        for(int i = 0; i < number_of_bullets; i++)
        {
            _aSource.PlayOneShot(_shootClip, Random.Range(0.6f, 0.8f));

            BulletBehavior newBullet = Instantiate(_bullet, transform.GetChild(0).position, transform.rotation);
            newBullet.Initialize(transform.up.normalized, _bullet_lifetime, _bullet_speed);
            yield return new WaitForSeconds(_bullet_cd);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (_cr.CanDamage(other))
        {
            
        }

        if (_cr.CostsDurability(other))
        {
            _health.TryTakeDamage(1);
        }
    }
}
