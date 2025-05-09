using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ChargedShot", menuName = "SlotEffects/ChargedShot")]
public class ChargedShot : SlotEffect
{
    [Header("每次攻击减少的冷却时间")]
    public float cooldownReducePerAttack = 0.3f;

    [Header("最低冷却限制")]
    public float minCooldown = 2f;

    [Header("技能重置为的基础冷却")]
    public float baseCooldown = 8f;

    private Dictionary<string, System.Action> normalAttackHandlers = new Dictionary<string, System.Action>();
    private Dictionary<string, System.Action<int>> skillUsedHandlers = new Dictionary<string, System.Action<int>>();

    private int skillIndex = 0; // 步枪速射默认为技能1，index = 0；OnSkillUsed 会传入 index+1

    public override void ApplyEffect(PlayerController player)
    {
        // 检查是否是 Rifleman
        if (!(player is Rifleman rifleman))
        {
            Debug.LogWarning($"[ChargedShot] 插槽仅适用于 Rifleman，但当前是 {player.symbol.unitName}，忽略应用");
            return;
        }

        string id = player.symbol.unitID;

        // 普通攻击事件
        System.Action normalAttackHandler = () =>
        {
            if (player.isUsingSkill) return;

            float oldCooldown = rifleman.rapidFireCooldown;
            rifleman.rapidFireCooldown = Mathf.Max(minCooldown, rifleman.rapidFireCooldown - cooldownReducePerAttack);

            Debug.Log($"[ChargedShot] {rifleman.symbol.unitName} 普攻触发，速射冷却从 {oldCooldown:F2}s 降低至 {rifleman.rapidFireCooldown:F2}s");
        };
        normalAttackHandlers[id] = normalAttackHandler;
        player.OnNormalAttack += normalAttackHandler;

        // 技能使用事件（index = 1 为步枪速射）
        System.Action<int> skillUsedHandler = (usedIndex) =>
        {
            if (usedIndex == skillIndex + 1)
            {
                float oldCooldown = rifleman.rapidFireCooldown;
                rifleman.rapidFireCooldown = baseCooldown;

                Debug.Log($"[ChargedShot] {rifleman.symbol.unitName} 使用步枪速射，冷却重置为 {baseCooldown}s（原为 {oldCooldown}s）");
            }
        };
        skillUsedHandlers[id] = skillUsedHandler;
        player.OnSkillUsed += skillUsedHandler;
    }

    public override void RemoveEffect(PlayerController player)
    {
        string id = player.symbol.unitID;

        if (normalAttackHandlers.TryGetValue(id, out var handler1))
        {
            player.OnNormalAttack -= handler1;
            normalAttackHandlers.Remove(id);
        }

        if (skillUsedHandlers.TryGetValue(id, out var handler2))
        {
            player.OnSkillUsed -= handler2;
            skillUsedHandlers.Remove(id);
        }
    }
}
