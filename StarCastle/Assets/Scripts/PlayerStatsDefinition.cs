using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsDefinition", menuName = "Scriptable Objects/PlayerStatsDefinition")]
public class PlayerStatsDefinition : ScriptableObject
{
    [Header ("Movement stats")]
    [SerializeField] public float _max_speed = 6.0f;
    [SerializeField, Min(0f)] public float _seconds_to_max_speed = 0.4f;
    [SerializeField, Min(0f)] public float _coast_to_stop_time = 2.0f;
    [SerializeField, Min(0f)] public float _seconds_to_stop = 0.5f;
    [SerializeField] public float _max_turn_speed = 180f;
    [SerializeField, Min(0f)] public float _seconds_to_max_turn_speed = 0.1f;
    [SerializeField, Min(0f)] public float _seconds_to_stop_turning = 0.1f;
    [SerializeField, Min(0f)] public float _overspeed_recovery_time = 0.4f;

    [Header ("Dash stats")]
    [SerializeField] public float _dash_distance = 5f;
    [SerializeField, Min(0.01f)] public float _dash_acceleration_duration = 0.06f;
    [SerializeField, Min(0f)] public float _dash_cruise_duration = 0.12f;
    [SerializeField, Min(0.01f)] public float _dash_deceleration_duration = 0.12f;
    [SerializeField, Min(0)] public int _dash_max_charges = 1;
    [SerializeField, Min(0f)] public float _dash_recharge_time = 3.0f;

    [Header ("Bullet stats")]
    [Min(0.01f)] public float _fire_cd;
    [Min(1)] public int _bullets_per_fire;
    [Min(0.01f)] public float _bullet_cd;
    public float _bullet_speed;
    [Min (0.01f)] public float _bullet_lifetime;
    public int _bullet_damage;
}
