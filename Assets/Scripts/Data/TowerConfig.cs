using System;
using UnityEngine;

namespace TowerDefense.Data
{
    [Serializable]
    public class TowerConfig
    {
        public TowerType type;
        public string towerName;
        [TextArea] public string description;

        [Header("Основные характеристики (баланс)")]
        public int cost = 100;
        public float damage = 12f;
        public float attackCooldown = 0.6f;
        public float range = 3.2f;

        [Header("Тип атаки и спецспособности")]
        public AttackType attackType = AttackType.SingleTarget;
        [Range(0.1f, 0.9f)] public float slowFactor = 0.5f;     // Снижение скорости (0.5 = на 50%)
        public float slowDuration = 2.5f;                       // Длительность замедления
        public float splashRadius = 1.6f;                       // Радиус поражения для AoE
        public float projectileSpeed = 10f;

        [Header("Прокачка и продажа")]
        public int upgradeCost = 80;
        public float sellRefundRatio = 0.7f;                    // Возврат 70% от стоимости

        public float DPS => attackCooldown > 0.001f ? (damage / attackCooldown) : 0f;

        public TowerConfig Clone()
        {
            return (TowerConfig)MemberwiseClone();
        }

        public void CopyFrom(TowerConfig source)
        {
            type = source.type;
            towerName = source.towerName;
            description = source.description;
            cost = source.cost;
            damage = source.damage;
            attackCooldown = source.attackCooldown;
            range = source.range;
            attackType = source.attackType;
            slowFactor = source.slowFactor;
            slowDuration = source.slowDuration;
            splashRadius = source.splashRadius;
            projectileSpeed = source.projectileSpeed;
            upgradeCost = source.upgradeCost;
            sellRefundRatio = source.sellRefundRatio;
        }
    }
}
