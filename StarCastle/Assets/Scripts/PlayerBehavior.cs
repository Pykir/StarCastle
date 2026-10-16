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
    [SerializeField] private float _forward_acc = 10f;
    [SerializeField] private float _turnTorque = 50f;
    [SerializeField] private float _friction = 2.5f;

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

        //_rb.rotation -= turn_direction * _turnSpeedDegrees * Time.fixedDeltaTime;
        _rb.AddTorque(-turn_direction * _turnTorque);

        if (Math.Sign(throttle) > 0)
        {
                //_rb.linearVelocity += (Vector2)transform.up.normalized * _forward_acc * Time.fixedDeltaTime;
                float remainingSpeed = Mathf.Max(0f, _max_speed - Vector2.Dot(_rb.linearVelocity, transform.up)) / Time.fixedDeltaTime;

                // locks thrust behind speed limit - this only enforces maximum speed forward
                // The ship can be moving faster than limit due to drift and collisions
                float thrustForce = Mathf.Min(_forward_acc, _rb.mass * remainingSpeed);

                _rb.AddForce(transform.up * thrustForce, ForceMode2D.Force);
        } else
        {
            //_rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, Vector2.zero, _friction * Time.fixedDeltaTime);
            if (_rb.linearVelocity.magnitude > 0f)
            {
                float remainingFriction = Mathf.Min(_friction, _rb.mass * _rb.linearVelocity.magnitude / Time.fixedDeltaTime);
                _rb.AddForce(-_rb.linearVelocity.normalized * remainingFriction, ForceMode2D.Force);
            }    
        }

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
