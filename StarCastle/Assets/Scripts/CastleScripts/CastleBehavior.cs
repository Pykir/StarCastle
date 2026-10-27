

using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CastleBehavior : MonoBehaviour
{
    [HideInInspector] public HealthComponent _health;

    [SerializeField] public PlayerBehavior _enemyPlayer;
    private Rigidbody2D _target;
    private CastleMovement _movement;

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
    [System.Serializable] public class MissileSpawnOption
    {
        public string name;
        public MissileBehavior prefab;

        [Min(0)] public int weight = 1;
        [Min(0.1f)] public float lifetime = 10f;
        [Min(0)] public int minimumDifficulty = 0;
    }
    [SerializeField] private MissileSpawnOption[] _missileOptions;
    [SerializeField] private MissileBehavior _mine;

    // Bullets
    [SerializeField] private BulletBehavior _bullet;
    [SerializeField] private float _bullet_lifetime = 5f;

    [SerializeField] private TargetedBarrageBehavior _barrage;
    [SerializeField] private float _barrage_fuse_time = 2f;
    [SerializeField] private float _barrage_radius = 1f;
    [SerializeField] private int _barrage_damage = 2;

    // Difficulty settings
    [Header("Difficulty Settings")]
    [SerializeField] private int _shield_starting_count = 2;
    [SerializeField] private float _shield_amount_multiplier = 0.4f;
    [SerializeField] private float _starting_bullet_interval = 5.0f;
    [SerializeField] private float _bullet_interval_decrement = 0.3f;
    [SerializeField] private float _bullet_minimum_interval = 1f;
    [SerializeField] private float _starting_bullet_speed = 5f;
    [SerializeField] private float _bullet_speed_increment = 0.2f;
    [SerializeField] private float _max_missiles_multiplier = 0.5f;
    [SerializeField] private int _starting_missiles_count = 1;
    [SerializeField] private float _starting_missile_interval = 6f;
    [SerializeField] private float _missile_interval_decrement = 0.5f;
    [SerializeField] private float _missile_minimum_interval = 1.5f;
    [SerializeField] private float _mine_frequency_multiplier = 0.04f;
    [SerializeField] private float _starting_mine_frequency = 0.10f;
    [SerializeField] private float _mine_lifetime = 8f;
    [SerializeField] private int _barrage_starting_difficulty = 3;
    [SerializeField] private int _starting_barrage_count = 1;
    [SerializeField] private float _barrage_count_multiplier = 0.4f;
    [SerializeField] private float _starting_barrage_interval = 7.0f;
    [SerializeField] private float _barrage_interval_decrement = 0.4f;
    [SerializeField] private float _barrage_minimum_interval = 2.0f;

    private int current_shield_count;
    private float current_bullet_interval;
    private int max_missile_count;
    private int current_missile_count;
    private float current_missile_interval;
    private float current_mine_frequency;
    private int max_mine_count;
    private float current_bullet_speed;
    private int current_barrage_count;
    private float current_barrage_interval;
   [SerializeField] private float current_time_between_barrages;

    private bool FiringBullet = false;
    private bool FiringMissile = false;
    private bool FiringBarrage = false;
    private int _difficulty;
    
    public Action MissileFired;
    public Action CannonCharging;
    public Action CannonFired;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _movement = GetComponent<CastleMovement>();
    }

    public void Initialize(int difficulty)
    {
        _difficulty = difficulty;

        current_shield_count = Mathf.FloorToInt(_shield_starting_count + (difficulty * _shield_amount_multiplier));
        current_bullet_interval = Mathf.Max(_starting_bullet_interval - (difficulty * _bullet_interval_decrement), _bullet_minimum_interval);
        max_missile_count = Mathf.FloorToInt(_starting_missiles_count + (difficulty * _max_missiles_multiplier));
        current_missile_interval = Mathf.Max(_starting_missile_interval - (difficulty * _missile_interval_decrement), _missile_minimum_interval);
        current_mine_frequency = _starting_mine_frequency + (difficulty * _mine_frequency_multiplier);
        max_mine_count = Mathf.FloorToInt(_shield_shape_sides * current_mine_frequency);
        current_bullet_speed = Mathf.FloorToInt(_starting_bullet_speed + (_bullet_speed_increment * difficulty));
        current_barrage_count = Mathf.FloorToInt(_starting_barrage_count + ((difficulty - _barrage_starting_difficulty) * _barrage_count_multiplier));
        current_barrage_interval = Mathf.Max(Mathf.FloorToInt(_starting_barrage_interval - ((difficulty - _barrage_starting_difficulty) * _barrage_interval_decrement)), _barrage_minimum_interval);
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
        if((current_missile_count < max_missile_count) && !FiringMissile) StartCoroutine(MissileLoop(current_missile_interval + Random.Range(-current_missile_interval/3, current_missile_interval/3)));
        if(!FiringBarrage && (_difficulty >= _barrage_starting_difficulty)) StartCoroutine(BarrageLoop(current_barrage_interval));
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
        ShieldCircleBehavior currentShield = Instantiate(_shield, transform, false);
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

        CannonCharging?.Invoke();

        yield return new WaitForSeconds(time);

        if (_enemyPlayer == null)
        {
            FiringBullet = false;
            yield break;
        }
        
        CannonFired?.Invoke();

        BulletBehavior newBullet = Instantiate(_bullet, transform.position, Quaternion.identity);
        newBullet.Initialize(_enemyPlayer.GetComponent<Rigidbody2D>().position - (Vector2)transform.position, _bullet_lifetime, current_bullet_speed);
        FiringBullet = false;
    }

    private IEnumerator MissileLoop(float time)
    {
        FiringMissile = true; 

        yield return new WaitForSeconds(time);

        MissileSpawnOption selected = ChooseMissile();

        if (selected != null && _target != null)
        {
            current_missile_count++;
            SpawnMissile(_target, selected.prefab, selected.lifetime);
        }

        FiringMissile = false;
    }

    void DecrementMissileCount(HealthComponent health)
    {
        current_missile_count--;
    }

    void SpawnMissile(Rigidbody2D target, MissileBehavior missile, float lifetime)
    {
        MissileFired?.Invoke();

        Vector3 spawnPosition = transform.position + (Vector3)Random.onUnitCircle;
        Vector2 direction = target.position - (Vector2)spawnPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        MissileBehavior newMissile = Instantiate(missile, spawnPosition, Quaternion.Euler(0f, 0f, angle));
        newMissile.Initialize(target);
        newMissile.GetComponent<BulletBehavior>().Initialize(lifetime);
        newMissile.GetComponent<HealthComponent>().Died += DecrementMissileCount;
    }

    private IEnumerator BarrageLoop(float time)
    {
        FiringBarrage = true; 

        yield return new WaitForSeconds(time);

        for (int i = 0; i < current_barrage_count; i++)
        {
            StartCoroutine(FireBarrage(_target, i * current_time_between_barrages));
        }

        FiringBarrage = false;
    }

    private IEnumerator FireBarrage(Rigidbody2D target, float time)
    {
        yield return new WaitForSeconds(time);

        SpawnBarrageOnceAt(target);
    }

    private void SpawnBarrageOnceAt(Rigidbody2D target)
    {
        TargetedBarrageBehavior barrage = Instantiate(_barrage, target.position, Quaternion.identity);
        barrage.Initialize(_barrage_fuse_time, _barrage_radius, _barrage_damage);
    }

    private bool CanSpawn(MissileSpawnOption option)
    {
        return option != null
            && option.prefab != null
            && option.weight > 0
            && _difficulty >= option.minimumDifficulty;
    }

    private MissileSpawnOption ChooseMissile()
    {
        if (_missileOptions == null) return null;

        int totalWeight = 0;

        foreach (var option in _missileOptions)
        {
            if (CanSpawn(option))
                totalWeight += option.weight;
        }

        if (totalWeight == 0) return null;

        int roll = Random.Range(0, totalWeight);

        foreach (var option in _missileOptions)
        {
            if (!CanSpawn(option)) continue;

            if (roll < option.weight)
                return option;

            roll -= option.weight;
        }

        return null;
    }
}
