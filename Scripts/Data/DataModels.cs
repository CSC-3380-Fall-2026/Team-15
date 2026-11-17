using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeyondTheLastGoodbye.Data;

// Typed versions of the files in data/. Property names map to the JSON's
// snake_case keys automatically (see DataLoader.JsonOptions).

// ---------- classes.json ----------
public class ClassesFile { public List<ClassData> Classes { get; set; } = new(); }

public class ClassData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Role { get; set; } = "";
    public ClassStats Stats { get; set; } = new();
    public WeaponData Weapon { get; set; } = new();
    [JsonPropertyName("ability_1")]
    public AbilityData Ability1 { get; set; } = new();
    [JsonPropertyName("ability_2")]
    public AbilityData Ability2 { get; set; } = new();
    [JsonPropertyName("ability_3")]
    public AbilityData Ability3 { get; set; } = new();
    public PassiveData Passive { get; set; } = new();
}

public class ClassStats
{
    public int MaxHealth { get; set; }
    public float MoveSpeedPxPerSecond { get; set; }
    public float DamageReductionPercent { get; set; }
}

public class WeaponData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string AttackType { get; set; } = "";
    public int Damage { get; set; }
    public float AttackCooldownSeconds { get; set; }
    public float RangePx { get; set; }
    public float ProjectileSpeedPxPerSecond { get; set; }
}

public class AbilityData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public float CooldownSeconds { get; set; }
}

public class PassiveData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
}

// ---------- enemies.json ----------
public class EnemiesFile { public List<EnemyData> Enemies { get; set; } = new(); }

public class EnemyData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Type { get; set; } = "";          // "regular" or "boss"
    public int MaxHealth { get; set; }
    public float MoveSpeedPxPerSecond { get; set; }
    public float DetectionRangePx { get; set; }
    public BasicAttackData? BasicAttack { get; set; } // regular enemies
    public BossData? Boss { get; set; }               // bosses
    public bool IsBoss => Type == "boss";
}

public class BasicAttackData
{
    public string AttackType { get; set; } = "";    // "melee" or "projectile"
    public int Damage { get; set; }
    public float AttackRangePx { get; set; }
    public float AttackCooldownSeconds { get; set; }
    public float KeepDistancePx { get; set; }
    public float ProjectileSpeedPxPerSecond { get; set; }
}

public class BossData
{
    [JsonPropertyName("phase_2_health_percent")]
    public float Phase2HealthPercent { get; set; }
    [JsonPropertyName("attack_wait_seconds_phase_1")]
    public float AttackWaitSecondsPhase1 { get; set; }
    [JsonPropertyName("attack_wait_seconds_phase_2")]
    public float AttackWaitSecondsPhase2 { get; set; }
    public List<BossAttackData> Attacks { get; set; } = new();
}

public class BossAttackData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Damage { get; set; }
}

// ---------- items.json ----------
public class ItemsFile { public List<ItemData> Items { get; set; } = new(); }

public class ItemData
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public int MaxStack { get; set; }
    public ConsumableData? Consumable { get; set; }
}

public class ConsumableData
{
    public string EffectType { get; set; } = "";
    public int Amount { get; set; }
}

// ---------- levels.json ----------
public class LevelsFile { public List<LevelData> Levels { get; set; } = new(); }

public class LevelData
{
    public string Id { get; set; } = "";
    public int LevelNumber { get; set; }
    public string DisplayName { get; set; } = "";
    public int RoomWidthTiles { get; set; }
    public int RoomHeightTiles { get; set; }
    public int TileSizePx { get; set; }
    public string StartRoomId { get; set; } = "";
    public List<RoomData> Rooms { get; set; } = new();
}

public class RoomData
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";          // combat, treasure, rest, boss
    public List<string> NextRoomIds { get; set; } = new();
    public List<ObstacleData> Obstacles { get; set; } = new();
    public Dictionary<string, EnemySlot>? Combat { get; set; } // "enemy_slot_1" -> enemy
    public TreasureData? Treasure { get; set; }
    public BossRoomData? Boss { get; set; }
}

public class ObstacleData
{
    [JsonPropertyName("x_tile")]
    public int XTile { get; set; }
    [JsonPropertyName("y_tile")]
    public int YTile { get; set; }
    public int WidthTiles { get; set; }
    public int HeightTiles { get; set; }
}

public class EnemySlot { public string EnemyId { get; set; } = ""; }
public class TreasureData { public string ItemId { get; set; } = ""; }
public class BossRoomData { public string BossId { get; set; } = ""; }
