using UnityEngine;

public class StatMonitor
{
    private PlayerController pc;

    // ¼ÇÂ¼ÉÏ´Î×´Ì¬
    private float lastMaxHealth;
    private float lastMaxEnergy;
    private float lastHealth;
    private float lastEnergy;
    private float lastDamage;
    private float lastAttackDelay;
    private float lastMovingSpeed;
    private float lastAttackRadius;
    private float lastInteractSpeed;
    private float lastRecoverSpeed;

    private float lastVisionRadius;
    private int lastLevel;

    public StatMonitor(PlayerController player)
    {
        pc = player;
        CacheCurrentValues();
    }

    private void CacheCurrentValues()
    {
        lastHealth = pc.health;
        lastEnergy = pc.energy;
        lastMaxHealth = pc.attribute.maxhealth;
        lastMaxEnergy = pc.attribute.maxEnergy;
        lastDamage = pc.attribute.damage;
        lastAttackDelay = pc.attribute.attackDelay;
        lastMovingSpeed = pc.attribute.movingSpeed;
        lastAttackRadius = pc.attribute.attackRange != null ? pc.attribute.attackRange.radius : -1f;
        lastInteractSpeed = pc.attribute.interactspeed;
        lastRecoverSpeed = pc.attribute.recoveredspeed;

        lastVisionRadius = pc.visionRadius;
        lastLevel = pc.level;
    }

    public void CheckForChanges()
    {
        if (pc == null) return;

        bool changed = false;
        if (!Mathf.Approximately(pc.health, lastHealth))
        {
            lastHealth = pc.health;
            changed = true;
        }

        if (!Mathf.Approximately(pc.energy, lastEnergy))
        {
            lastEnergy = pc.energy;
            changed = true;
        }
        if (!Mathf.Approximately(pc.attribute.maxhealth, lastMaxHealth)) { lastMaxHealth = pc.attribute.maxhealth; changed = true; }
        if (!Mathf.Approximately(pc.attribute.maxEnergy, lastMaxEnergy)) { lastMaxEnergy = pc.attribute.maxEnergy; changed = true; }
        if (pc.attribute.damage != lastDamage) { lastDamage = pc.attribute.damage; changed = true; }
        if (!Mathf.Approximately(pc.attribute.attackDelay, lastAttackDelay)) { lastAttackDelay = pc.attribute.attackDelay; changed = true; }
        if (!Mathf.Approximately(pc.attribute.movingSpeed, lastMovingSpeed)) { lastMovingSpeed = pc.attribute.movingSpeed; changed = true; }
        float currentRadius = pc.attribute.attackRange != null ? pc.attribute.attackRange.radius : -1f;
        if (!Mathf.Approximately(currentRadius, lastAttackRadius)) { lastAttackRadius = currentRadius; changed = true; }
        if (!Mathf.Approximately(pc.attribute.interactspeed, lastInteractSpeed)) { lastInteractSpeed = pc.attribute.interactspeed; changed = true; }
        if (!Mathf.Approximately(pc.attribute.recoveredspeed, lastRecoverSpeed)) { lastRecoverSpeed = pc.attribute.recoveredspeed; changed = true; }

        if (!Mathf.Approximately(pc.visionRadius, lastVisionRadius)) { lastVisionRadius = pc.visionRadius; changed = true; }
        if (pc.level != lastLevel) { lastLevel = pc.level; changed = true; }

        if (changed)
        {
            pc.TriggerStatsChanged();
        }
    }
}
