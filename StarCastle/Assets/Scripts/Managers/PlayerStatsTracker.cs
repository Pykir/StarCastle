using UnityEngine;

public class PlayerStatsTracker : MonoBehaviour
{
    [Header ("Player upgrades")]
    private int _additional_max_health = 0;
    public int AddMaxHealth => _additional_max_health;

    [Header ("Movement upgrades")]
    private float _additional_move_speed = 0;
    public float AddMoveSpeed => _additional_move_speed;
    private float _additional_dash_distance = 0;
    public float AddDashDistance => _additional_dash_distance;
    private float _recharge_rate_multiplier = 1;
    public float DashRechargeMult => _recharge_rate_multiplier;
    private int _additional_dash_charges = 0;
    public int AddDashCharges => _additional_dash_charges;

    [Header ("Bullet upgrades")]
    private float _fire_rate_multiplier = 1;
    public float FiringMult => _fire_rate_multiplier;
    private int _additional_burst_fire_bullets = 0;
    public int AddBurstBullets => _additional_burst_fire_bullets;
    private float _additional_bullet_speed = 0;
    public float AddBulletSpeed => _additional_bullet_speed;
    private int _additional_bullet_damage = 0;
    public int AddBulletDamage => _additional_bullet_damage;
}
