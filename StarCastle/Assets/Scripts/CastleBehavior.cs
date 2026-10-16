

using System.Collections;
using UnityEngine;

public class CastleBehavior : MonoBehaviour
{
    [HideInInspector] public HealthComponent _health;
    private AudioSource _aSource;
    [SerializeField] private AudioClip _missileLaunchClip;

    [SerializeField] public PlayerBehavior _enemyPlayer;
    private Rigidbody2D _target;

    // Shields
    [SerializeField] private ShieldCircleBehavior _shield;
    [SerializeField] private float _shield_spawn_time = 1.0f;
    [SerializeField] private int _shield_shape_sides = 10;
    [SerializeField] private float _shield_respawn_time = 2f;
    [SerializeField] private float _shield_speed_base = 160f;
    [SerializeField] private float _shield_speed_variation = 30f;
    [SerializeField] private float _shield_smallest_radius = 1f;
    [SerializeField] private float _shield_radius_gap = 1f;

    // Missiles
    [SerializeField] private MissileBehavior _missile;
    [SerializeField] private MissileBehavior _mine;

    // Bullets
    [SerializeField] private BulletBehavior _bullet;
    [SerializeField] private float _bullet_speed = 5f;
    [SerializeField] private float _bullet_lifetime = 5f;

    // Difficulty settings
    [SerializeField] private float _shield_amount_multiplier = 0.4f;
    [SerializeField] private int _shield_starting_count = 2;
    [SerializeField] private float _bullet_interval_multiplier = 0.3f;
    [SerializeField] private float _starting_bullet_interval = 5.0f;
    [SerializeField] private float _bullet_speed_increment = 0.2f;
    [SerializeField] private float _max_missiles_multiplier = 0.5f;
    [SerializeField] private int _starting_missiles_count = 1;
    [SerializeField] private float _mine_frequency_multiplier = 0.04f;
    [SerializeField] private float _starting_mine_frequency = 0.10f;
    [SerializeField] private float _mine_lifetime = 8f;

    private int current_shield_count;
    private float current_bullet_interval;
    private int max_missile_count;
    private int current_missile_count;
    private float current_mine_frequency;
    private int max_mine_count;

    private bool FiringBullet = false;
    private int _difficulty;
    

    public void Initialize(int difficulty)
    {
        _difficulty = difficulty;
        current_shield_count = Mathf.FloorToInt(_shield_starting_count + (difficulty * _shield_amount_multiplier));
        current_bullet_interval = Mathf.FloorToInt(_starting_bullet_interval - (difficulty * _bullet_interval_multiplier));
        max_missile_count = Mathf.FloorToInt(_starting_missiles_count + (difficulty * _max_missiles_multiplier));
        current_mine_frequency = _starting_mine_frequency + (difficulty * _mine_frequency_multiplier);
        max_mine_count = Mathf.FloorToInt(_shield_shape_sides * current_mine_frequency);
        _bullet_speed += _bullet_speed_increment * difficulty;
    }

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _aSource = GetComponent<AudioSource>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShieldSetup();
    }

    void ShieldSetup()
    {
        for(int i = 0; i < current_shield_count; i++)
        {
            SpawnShield((i * _shield_radius_gap) + _shield_smallest_radius);
        }
    }

    void FixedUpdate()
    {
        if(!FiringBullet) StartCoroutine(FireLoop(current_bullet_interval));
        if(current_missile_count < max_missile_count) StartCoroutine(MissileLoop(3));
    }

    void SpawnMissile(Rigidbody2D target)
    {
        _aSource.PlayOneShot(_missileLaunchClip, Random.Range(0.8f, 1f));

        MissileBehavior newMissile = Instantiate(_missile, transform.position, Quaternion.identity);
        newMissile.Initialize(target);
        newMissile.GetComponent<HealthComponent>().Died += DecrementMissileCount;
    }

    void SpawnMineAt(Vector3 position, Rigidbody2D target)
    {
        MissileBehavior newMissile = Instantiate(_mine, position, Quaternion.identity);
        newMissile.Initialize(target);
    }

    public void Retarget(PlayerBehavior newTarget)
    {
        _enemyPlayer = newTarget;
        _target = _enemyPlayer.GetComponent<Rigidbody2D>();

        MissileBehavior[] missiles = FindObjectsByType<MissileBehavior>(FindObjectsSortMode.None);

        foreach (MissileBehavior missile in missiles)
        {
            missile.Initialize(_target);
        }

        ShieldCircleBehavior[] shields = FindObjectsByType<ShieldCircleBehavior>(FindObjectsSortMode.None);

        foreach (ShieldCircleBehavior shield in shields)
        {
            shield._mineTarget = _target;
        }
    }

    void SpawnShield(float radius)
    {
        float speed = _shield_speed_base + Random.Range(-_shield_speed_variation, _shield_speed_variation);
        if (Random.Range(0, 2) == 0)
        {
            speed = -speed;
        }
        ShieldCircleBehavior currentShield = Instantiate(_shield, transform.position, Quaternion.identity);
        currentShield.RegenerationRequested += OnRegenerationRequested;
        currentShield.Initialize(_shield_shape_sides, radius, speed, _shield_spawn_time, _shield_respawn_time, _mine, current_mine_frequency, max_mine_count, _target, _mine_lifetime);
    }

    void OnRegenerationRequested (ShieldCircleBehavior oldShield, float oldRadius)
    {
        oldShield.RegenerationRequested -= OnRegenerationRequested;
        SpawnShield(oldRadius);
    }

    private IEnumerator FireLoop(float time)
    {
        FiringBullet = true;

        yield return new WaitForSeconds(time);

        if (_enemyPlayer == null)
        {
            FiringBullet = false;
            yield break;
        }
        
        BulletBehavior newBullet = Instantiate(_bullet, transform.position, Quaternion.identity);
        newBullet.Initialize(_enemyPlayer.GetComponent<Rigidbody2D>().position - (Vector2)transform.position, _bullet_lifetime, _bullet_speed);
        FiringBullet = false;
    }

    private IEnumerator MissileLoop(float time)
    {
        current_missile_count++;

        yield return new WaitForSeconds(time);

        SpawnMissile(_target);
    }

    void DecrementMissileCount(HealthComponent health)
    {
        current_missile_count--;
    }
}
