using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Data
{
    [Serializable]
    public class WaveSubGroup
    {
        public EnemyType enemyType = EnemyType.Grunt;
        public int count = 5;
        public float spawnInterval = 1.2f;
        public float delayBeforeGroup = 0f;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Wave 1";
        public List<WaveSubGroup> groups = new List<WaveSubGroup>();
        public int bonusGoldReward = 50;
    }
}
