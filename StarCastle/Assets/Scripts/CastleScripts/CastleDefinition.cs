using UnityEngine;

[CreateAssetMenu(fileName = "CastleDefinition", menuName = "Star Castle/Castle Definition")]
public class CastleDefinition : ScriptableObject
{
    [Header("Shields")]
    public ShieldCircleBehavior _shield;
    public float _shield_spawn_time = 2f;
    public int _shield_shape_sides = 12;
    public float _shield_respawn_time = 1f;
    public float _shield_speed_base = 160f;
    public float _shield_speed_variation = 30f;
    public float _shield_smallest_radius = 1f;
    public float _shield_radius_gap = 1f;
    public int _shield_starting_count = 2;
    public float _shield_amount_multiplier = 0.4f;
    public int _shield_max_count = 5;

    [Header("Missiles")]
    public CastleBehavior.MissileSpawnOption[] _missileOptions;
    public float _max_missiles_multiplier = 0.5f;
    public int _starting_missiles_count = 1;
    public float _starting_missile_interval = 6f;
    public float _missile_interval_decrement = 0.5f;
    public float _missile_minimum_interval = 1.5f;

    [Header("Mines")]
    public MissileBehavior _mine;
    public float _mine_frequency_multiplier = 0.03f;
    public float _starting_mine_frequency = 0f;
    public float _mine_lifetime = 8f;

    [Header("Cannon")]
    public BulletBehavior _bullet;
    public float _bullet_lifetime = 5f;
    public float _starting_bullet_interval = 5f;
    public float _bullet_interval_decrement = 0.3f;
    public float _bullet_minimum_interval = 1f;
    public float _starting_bullet_speed = 5f;
    public float _bullet_speed_increment = 0.2f;

    [Header("Barrage")]
    public TargetedBarrageBehavior _barrage;
    public float _barrage_fuse_time = 1f;
    public float _barrage_radius = 1f;
    public int _barrage_damage = 2;
    public int _barrage_starting_difficulty = 3;
    public int _starting_barrage_count = 1;
    public float _barrage_count_multiplier = 0.4f;
    public float _starting_barrage_interval = 7f;
    public float _barrage_interval_decrement = 0.4f;
    public float _barrage_minimum_interval = 2f;
    public float _time_between_barrages = 0.3f;

    [Header("Movement")]
    public float _bounds_padding = 2f;
    public float _acceleration = 5f;
    public float _correction_acceleration = 8f;
    public float _move_time = 5f;
    public float _wait_time = 5f;
}
