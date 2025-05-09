using UnityEngine;

[CreateAssetMenu(fileName = "BulletEnhancement", menuName = "SlotEffects/BulletEnhancement")]
public class BulletEnhancement : SlotEffect
{
    private void OnEnable()
    {
        onlyApplyInBattle = false;
    }

    [Header("增加的伤害值")]
    public int damageIncrease = 1;

    public override void ApplyEffect(PlayerController player)
    {
        var state = player.GetSlotEffectState(this);
        if (state.hasUsedThisBattle) return;

        player.additionalDamageFromSlotEffect += damageIncrease;
        state.hasUsedThisBattle = true;
    }

    public override void RemoveEffect(PlayerController player)
    {
        player.additionalDamageFromSlotEffect -= damageIncrease;
    }

}
