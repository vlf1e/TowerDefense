namespace TowerDefense.Data
{
    public enum ZoneType
    {
        PathA,         // Зона A: Путь движения врагов
        BuildableB,    // Зона B: Зона размещения башен (сверху и снизу от A)
        BaseC,         // Зона C: Контрольная точка (база)
        DeadZoneD      // Зона D: Зона бездействия / фон
    }

    public enum TowerType
    {
        Archer,        // Базовая скорострельная пулеметная/стрелковая башня
        Frost,         // Башня замедления (контроль темпа)
        Mortar         // Тяжелая мортира с уроном по площади (AoE)
    }

    public enum EnemyType
    {
        Grunt,         // Стандартный пехотинец
        Scout,         // Быстрый легкобронированный бегун
        Golem          // Тяжелый бронированный танк
    }

    public enum TargetPriority
    {
        First,         // Ближайший к контрольной точке C
        Closest,       // Ближайший к самой башне
        Strongest      // С наибольшим текущим здоровьем
    }

    public enum AttackType
    {
        SingleTarget,  // Точечный одиночный урон
        Slow,          // Точечный урон с наложением замедления
        AreaOfEffect   // Урон по площади (всплеск)
    }

    public enum GameState
    {
        Playing,
        Paused,
        Won,
        GameOver
    }
}
