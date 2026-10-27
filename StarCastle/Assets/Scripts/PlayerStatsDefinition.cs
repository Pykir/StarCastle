using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsDefinition", menuName = "Scriptable Objects/PlayerStatsDefinition")]
public class PlayerStatsDefinition : ScriptableObject
{
    [Header("Health stats")]
    [Min(1)] public int _max_health = 5;

    [Header ("Movement stats")]
    public float _max_speed = 6.0f;
    [Min(0f)] public float _seconds_to_max_speed = 0.4f;
    [Min(0f)] public float _coast_to_stop_time = 2.0f;
    [Min(0f)] public float _seconds_to_stop = 0.5f;
    public float _max_turn_speed = 180f;
    [Min(0f)] public float _seconds_to_max_turn_speed = 0.1f;
    [Min(0f)] public float _seconds_to_stop_turning = 0.1f;
    [Min(0f)] public float _overspeed_recovery_time = 0.4f;

    [Header ("Dash stats")]
    public float _dash_distance = 5f;
    [Min(0.01f)] public float _dash_acceleration_duration = 0.06f;
    [Min(0f)] public float _dash_cruise_duration = 0.12f;
    [Min(0.01f)] public float _dash_deceleration_duration = 0.12f;
    [Min(0)] public int _dash_max_charges = 1;
    [Min(0f)] public float _dash_recharge_time = 3.0f;

    [Header ("Bullet stats")]
    [Min(0.01f)] public float _fire_cd;
    [Min(1)] public int _bullets_per_fire;
    [Min(0.01f)] public float _bullet_cd;
    public float _bullet_speed;
    [Min (0.01f)] public float _bullet_lifetime;
    public int _bullet_damage;
}
