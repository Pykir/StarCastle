

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
    private CastlePresentation _presentation;
    [SerializeField] private Transform _firePoint;

    [SerializeField] private CastleDefinition _definition;

    [System.Serializable] public class MissileSpawnOption
    {
        public string name;
        public MissileBehavior prefab;

        [Min(0)] public int weight = 1;
        [Min(0.1f)] public float lifetime = 10f;
        [Min(0)] public int minimumDifficulty = 0;
    }

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


    private bool FiringBullet = false;
    private bool FiringMissile = false;
    private bool FiringBarrage = false;
    private int _difficulty;
    
    public Action MissileFired;
    public Action<float> CannonCharging;
    public Action CannonFired;

    void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _movement = GetComponent<CastleMovement>();
        _presentation = GetComponent<CastlePresentation>();
    }

    public void Initialize(int difficulty)
    {
        _difficulty = difficulty;
        _movement?.Initialize(_definition);

        current_shield_count = Mathf.Min(Mathf.FloorToInt(_definition._shield_starting_count + (difficulty * _definition._shield_amount_multiplier)), _definition._shield_max_count);
        current_bullet_interval = Mathf.Max(_definition._starting_bullet_interval - (difficulty * _definition._bullet_interval_decrement), _definition._bullet_minimum_interval);
        max_missile_count = Mathf.FloorToInt(_definition._starting_missiles_count + (difficulty * _definition._max_missiles_multiplier));
        current_missile_interval = Mathf.Max(_definition._starting_missile_interval - (difficulty * _definition._missile_interval_decrement), _definition._missile_minimum_interval);
        current_mine_frequency = _definition._starting_mine_frequency + (difficulty * _definition._mine_frequency_multiplier);
        max_mine_count = Mathf.FloorToInt(_definition._shield_shape_sides * current_mine_frequency);
        current_bullet_speed = _definition._starting_bullet_speed + (_definition._bullet_speed_increment * difficulty);
        current_barrage_count = Mathf.FloorToInt(_definition._starting_barrage_count + ((difficulty - _definition._barrage_starting_difficulty) * _definition._barrage_count_multiplier));
        current_barrage_interval = Mathf.Max(_definition._starting_barrage_interval - ((difficulty - _definition._barrage_starting_difficulty) * _definition._barrage_interval_decrement), _definition._barrage_minimum_interval);
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
            SpawnShield((i * _definition._shield_radius_gap) + _definition._shield_smallest_radius);
        }
    }

    void FixedUpdate()
    {
        if(!FiringBullet) StartCoroutine(FireLoop(current_bullet_interval));
        if((current_missile_count < max_missile_count) && !FiringMissile) StartCoroutine(MissileLoop(current_missile_interval + Random.Range(-current_missile_interval/3, current_missile_interval/3)));
        if(!FiringBarrage && (_difficulty >= _definition._barrage_starting_difficulty)) StartCoroutine(BarrageLoop(current_barrage_interval));
    }

    
    void SpawnMineAt(Vector3 position, Rigidbody2D target)
    {
        MissileBehavior newMissile = Instantiate(_definition._mine, position, Quaternion.identity);
        newMissile.Initialize(target);
    }

    public void Retarget(PlayerBehavior newTarget)
    {
        _enemyPlayer = newTarget;
        _target = _enemyPlayer.GetComponent<Rigidbody2D>();

        _presentation._target = _target;

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
        float speed = _definition._shield_speed_base + Random.Range(-_definition._shield_speed_variation, _definition._shield_speed_variation);
        if (Random.Range(0, 2) == 0)
        {
            speed = -speed;
        }
        ShieldCircleBehavior currentShield = Instantiate(_definition._shield, transform, false);
        currentShield.RegenerationRequested += OnRegenerationRequested;
        currentShield.Initialize(_definition._shield_shape_sides, radius, speed, _definition._shield_spawn_time, _definition._shield_respawn_time, _definition._mine, current_mine_frequency, max_mine_count, _target, _definition._mine_lifetime);
    }

    void OnRegenerationRequested (ShieldCircleBehavior oldShield, float oldRadius)
    {
        oldShield.RegenerationRequested -= OnRegenerationRequested;
        SpawnShield(oldRadius);
    }

    private IEnumerator FireLoop(float time)
    {
        FiringBullet = true;

        CannonCharging?.Invoke(time);

        yield return new WaitForSeconds(time);

        if (_enemyPlayer == null)
        {
            FiringBullet = false;
            yield break;
        }
        
        CannonFired?.Invoke();

        BulletBehavior newBullet = Instantiate(_definition._bullet, _firePoint.position, Quaternion.identity);
        newBullet.Initialize(_firePoint.right, _definition._bullet_lifetime, current_bullet_speed);
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

    void DecrementMissileCount(HealthComponent health, bool instantRemove)
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
            StartCoroutine(FireBarrage(_target, i * _definition._time_between_barrages));
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
        if (target == null) return;

        TargetedBarrageBehavior barrage = Instantiate(_definition._barrage, target.position, Quaternion.identity);
        barrage.Initialize(_definition._barrage_fuse_time, _definition._barrage_radius, _definition._barrage_damage);
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
        if (_definition._missileOptions == null) return null;

        int totalWeight = 0;

        foreach (var option in _definition._missileOptions)
        {
            if (CanSpawn(option))
                totalWeight += option.weight;
        }

        if (totalWeight == 0) return null;

        int roll = Random.Range(0, totalWeight);

        foreach (var option in _definition._missileOptions)
        {
            if (!CanSpawn(option)) continue;

            if (roll < option.weight)
                return option;

            roll -= option.weight;
        }

        return null;
    }
}
