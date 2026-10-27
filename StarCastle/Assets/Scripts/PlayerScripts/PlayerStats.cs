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

    private float FiringMultiplier => Mathf.Max(0.01f, _runUpgrades.FiringMult * _tempEffects.FiringMult);
    public float EffectiveFireCD => _baseStats._fire_cd / FiringMultiplier;
    public float EffectiveBurstCD => _baseStats._bullet_cd / FiringMultiplier;
    public int EffectiveBurstCount => _baseStats._bullets_per_fire + _runUpgrades.AddBurstBullets;
    public int EffectiveBulletDamage => _baseStats._bullet_damage + _runUpgrades.AddBulletDamage;
    public float EffectiveBulletLifetime => _baseStats._bullet_lifetime;
    public float EffectiveBulletSpeed => _baseStats._bullet_speed + _runUpgrades.AddBulletSpeed;
}