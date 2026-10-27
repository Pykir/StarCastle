using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatsDefinition _baseStats;
    private PlayerEffects _tempEffects;
    private PlayerStatsTracker _runUpgrades; 

    void Awake()
    {
        _tempEffects = GetComponent<PlayerEffects>();
    }

    public void Initialize(PlayerStatsTracker runStats)
    {
        _runUpgrades = runStats;
    }

    public int EffectiveMaxHealth => Mathf.Max(1, _baseStats._max_health + _runUpgrades.AddMaxHealth);

    public float EffectiveMaxSpeed => _baseStats._max_speed + _runUpgrades.AddMoveSpeed;
    public float EffectiveSecondsToMaxSpeed => _baseStats._seconds_to_max_speed;
    public float EffectiveCoastToStopTime => _baseStats._coast_to_stop_time;
    public float EffectiveSecondsToStop => _baseStats._seconds_to_stop;
    public float EffectiveMaxTurnSpeed => _baseStats._max_turn_speed;
    public float EffectiveSecondsToMaxTurnSpeed => _baseStats._seconds_to_max_turn_speed;
    public float EffectiveSecondsToStopTurning => _baseStats._seconds_to_stop_turning;
    public float EffectiveOverspeedRecoveryTime => _baseStats._overspeed_recovery_time;

    public float EffectiveDashDistance => _baseStats._dash_distance + _runUpgrades.AddDashDistance;
    public float EffectiveDashAccelerationDuration => _baseStats._dash_acceleration_duration;
    public float EffectiveDashCruiseDuration => _baseStats._dash_cruise_duration;
    public float EffectiveDashDecelerationDuration => _baseStats._dash_deceleration_duration;
    public int EffectiveDashMaxCharges => Mathf.Max(0, _baseStats._dash_max_charges + _runUpgrades.AddDashCharges);
    public float EffectiveDashRechargeTime => _baseStats._dash_recharge_time / Mathf.Max(0.01f, _runUpgrades.DashRechargeMult);

    private float FiringMultiplier => Mathf.Max(0.01f, _runUpgrades.FiringMult * _tempEffects.FiringMult);
    public float EffectiveFireCD => _baseStats._fire_cd / FiringMultiplier;
    public float EffectiveBurstCD => _baseStats._bullet_cd / FiringMultiplier;
    public int EffectiveBurstCount => _baseStats._bullets_per_fire + _runUpgrades.AddBurstBullets;
    public int EffectiveBulletDamage => _baseStats._bullet_damage + _runUpgrades.AddBulletDamage;
    public float EffectiveBulletLifetime => _baseStats._bullet_lifetime;
    public float EffectiveBulletSpeed => _baseStats._bullet_speed + _runUpgrades.AddBulletSpeed;
}
