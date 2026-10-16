using UnityEngine;

public class ContactRules : MonoBehaviour
{
    [SerializeField] private LayerMask damageTargets;
    [SerializeField] private LayerMask durabilityCostTargets;

    public bool CanDamage(Collider2D target)
    {
        return Contains(damageTargets, target.gameObject.layer);
    }

    public bool CostsDurability(Collider2D target)
    {
        return Contains(durabilityCostTargets, target.gameObject.layer);
    }

    private static bool Contains(LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}