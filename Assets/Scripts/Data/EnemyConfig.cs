using System;
using UnityEngine;

namespace TowerDefense.Data
{
    [Serializable]
    public class EnemyConfig
    {
        public EnemyType type;
        public string enemyName;
        [TextArea] public string description;

        [Header("Основные характеристики (баланс)")]
        public float maxHealth = 80f;
        public float moveSpeed = 1.5f;
        public int goldReward = 20;
        public int baseDamage = 1;

        [Header("Визуал")]
        public Color tintColor = Color.white;
        public float visualScale = 1.0f;

        public EnemyConfig Clone()
        {
            return (EnemyConfig)MemberwiseClone();
        }

        public void CopyFrom(EnemyConfig source)
        {
            type = source.type;
            enemyName = source.enemyName;
            description = source.description;
            maxHealth = source.maxHealth;
            moveSpeed = source.moveSpeed;
            goldReward = source.goldReward;
            baseDamage = source.baseDamage;
            tintColor = source.tintColor;
            visualScale = source.visualScale;
        }
    }
}
