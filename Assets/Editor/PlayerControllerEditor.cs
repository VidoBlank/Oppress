using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : Editor
{
    private bool showAttribute = false;
    private bool showSlotEffectAdditions = false;
    private bool showFinalAttributes = true;

    public override void OnInspectorGUI()
    {
        PlayerController player = (PlayerController)target;
        serializedObject.Update();

        // === 等级字段 ===
        EditorGUILayout.PropertyField(serializedObject.FindProperty("level"));

        // === 最终属性显示（只读）===
        EditorGUILayout.Space(10);
        showFinalAttributes = EditorGUILayout.Foldout(showFinalAttributes, "最终属性（TotalXXX）");
        if (showFinalAttributes)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("总生命值", $"{player.health:F1} / {player.TotalMaxHealth:F1}");
            EditorGUILayout.LabelField("总能量值", $"{player.energy:F1} / {player.TotalMaxEnergy:F1}");
            EditorGUILayout.LabelField("倒地生命值", player.TotalKnockdownHealth.ToString("F2"));
            EditorGUILayout.LabelField("攻击伤害", player.TotalDamage.ToString());
            EditorGUILayout.LabelField("攻击范围", player.TotalAttackRange.ToString("F2"));
            EditorGUILayout.LabelField("攻击间隔", player.TotalAttackDelay.ToString("F2"));
            EditorGUILayout.LabelField("移动速度", player.TotalMovingSpeed.ToString("F2"));
            EditorGUILayout.LabelField("交互距离", player.TotalInteractionRange.ToString("F2"));
            EditorGUILayout.LabelField("交互速度", player.TotalInteractSpeed.ToString("F2"));
            EditorGUILayout.LabelField("恢复速度", player.TotalRecoverSpeed.ToString("F2"));
            EditorGUILayout.LabelField("子弹速度", player.TotalBulletSpeed.ToString("F2"));
            EditorGUILayout.LabelField("技能冷却缩减", player.TotalSkillCooldownRate.ToString("F2"));
            EditorGUILayout.LabelField("伤害倍率", player.TotalDamageTakenMultiplier.ToString("F2"));
            EditorGUILayout.LabelField("固定减伤", player.TotalFlatDamageReduction.ToString("F2"));
            EditorGUILayout.LabelField("每次攻击消耗能量", player.TotalAttackEnergyCost.ToString("F2"));
            EditorGUILayout.LabelField("视野半径", player.TotalVisionRadius.ToString("F2"));
            EditorGUI.indentLevel--;
        }

        // === health / energy 可调 ===
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("energy"));

        // === attribute 折叠 ===
        EditorGUILayout.Space(10);
        showAttribute = EditorGUILayout.Foldout(showAttribute, "基础属性 attribute");
        if (showAttribute)
        {
            SerializedProperty attrProp = serializedObject.FindProperty("attribute");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(attrProp, true);
            EditorGUI.indentLevel--;

            // ✅ 手动添加 attackRange
            EditorGUILayout.ObjectField("攻击范围碰撞体",
                ((PlayerController)target).attribute.attackRange, typeof(CircleCollider2D), true);
        }

        // === 插槽加成字段 ===
        EditorGUILayout.Space(10);
        showSlotEffectAdditions = EditorGUILayout.Foldout(showSlotEffectAdditions, "插槽加成字段");
        if (showSlotEffectAdditions)
        {
            string[] slotFieldNames = new string[]
            {
                "additionalMaxHealthFromSlotEffect",
                "additionalMaxEnergyFromSlotEffect",
                "additionalKnockdownHealthFromSlotEffect",
                "additionalAttackRangeFromSlotEffect",
                "additionalInteractSpeedFromSlotEffect",
                "additionalRecoverSpeedFromSlotEffect",
                "additionalMovingSpeedFromSlotEffect",
                "additionalInteractionRangeFromSlotEffect",
                "additionalDamageFromSlotEffect",
                "additionalAttackDelayFromSlotEffect",
                "additionalBulletSpeedFromSlotEffect",
                "additionalDamageTakenMultiplierFromSlotEffect",
                "additionalFlatDamageReductionFromSlotEffect",
                "additionalSkillCooldownRateFromSlotEffect",
                "additionalAttackEnergyCostFromSlotEffect",
                "additionalVisionRadiusFromSlotEffect"
            };

            EditorGUI.indentLevel++;
            foreach (string field in slotFieldNames)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
            }
            EditorGUI.indentLevel--;
        }

        // === 当前装备技能（安全访问）===
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("当前装备技能", EditorStyles.boldLabel);

        if (Application.isPlaying && player.equippedSkill != null)
        {
            try
            {
                EditorGUILayout.LabelField("技能名", player.equippedSkill.SkillName);
                EditorGUILayout.LabelField("能量消耗", player.equippedSkill.NeedEnergy.ToString("F1"));
                EditorGUILayout.LabelField("冷却时间", player.equippedSkill.Cooldown.ToString("F1"));
                EditorGUILayout.LabelField("瞬发技能", player.equippedSkill.IsInstantCast ? "是" : "否");
                EditorGUILayout.LabelField("持续技能", player.equippedSkill.IsSustained ? "是" : "否");
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox("⚠️ 访问技能数据失败：" + ex.Message, MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("未装备技能或未在运行模式下。", MessageType.Info);
        }



        // === 插槽效果列表 ===
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("已应用插槽", EditorStyles.boldLabel);
        if (player.slots != null && player.slots.Count > 0)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < player.slots.Count; i++)
            {
                var slot = player.slots[i];
                if (slot != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"插槽 {i + 1}", GUILayout.Width(80));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(slot.slotEffect, typeof(ScriptableObject), false);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField($"插槽 {i + 1}", "（空）");
                }
            }
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUILayout.HelpBox("无已应用插槽", MessageType.Info);
        }

        // === 其他未处理字段自动绘制 ===
        EditorGUILayout.Space(10);
        DrawPropertiesExcluding(serializedObject, new string[] {
            "attribute",
            "level",
            "health",
            "energy",
            "equippedSkill",
            "slots",
            "additionalMaxHealthFromSlotEffect",
            "additionalMaxEnergyFromSlotEffect",
            "additionalKnockdownHealthFromSlotEffect",
            "additionalAttackRangeFromSlotEffect",
            "additionalInteractSpeedFromSlotEffect",
            "additionalRecoverSpeedFromSlotEffect",
            "additionalMovingSpeedFromSlotEffect",
            "additionalInteractionRangeFromSlotEffect",
            "additionalDamageFromSlotEffect",
            "additionalAttackDelayFromSlotEffect",
            "additionalBulletSpeedFromSlotEffect",
            "additionalDamageTakenMultiplierFromSlotEffect",
            "additionalFlatDamageReductionFromSlotEffect",
            "additionalSkillCooldownRateFromSlotEffect",
            "additionalAttackEnergyCostFromSlotEffect",
            "additionalVisionRadiusFromSlotEffect"
        });

        serializedObject.ApplyModifiedProperties();
    }
}
