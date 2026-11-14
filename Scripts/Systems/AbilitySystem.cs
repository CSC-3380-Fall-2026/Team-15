#nullable enable
using System;
using System.Collections.Generic;
using BeyondTheLastGoodbye.Data;

namespace BeyondTheLastGoodbye.Systems;

/// <summary>
/// Tracks mana, mana regen and per-ability cooldowns for one player.
/// Plain C# (no Godot types) so it can be unit tested.
///
/// Usage (in Player.cs):
///   var abilities = new AbilitySystem(maxMana: 100, manaRegenPerSecond: 8);
///   abilities.AddAbility(classData.Ability1, manaCost: 20);
///   // every frame:
///   abilities.Tick((float)delta);
///   // on input:
///   if (abilities.TryUse(classData.Ability1.Id)) { /* do the ability */ }
/// </summary>
public class AbilitySystem
{
    private class Slot
    {
        public float CooldownSeconds;
        public float ManaCost;
        public float Remaining;
    }

    private readonly Dictionary<string, Slot> _abilities = new();

    public float MaxMana { get; }
    public float Mana { get; private set; }
    public float ManaRegenPerSecond { get; set; }

    /// <summary>Fires after an ability is used successfully (ability id).</summary>
    public event Action<string>? AbilityUsed;

    public AbilitySystem(float maxMana, float manaRegenPerSecond)
    {
        if (maxMana < 0) throw new ArgumentOutOfRangeException(nameof(maxMana));
        MaxMana = maxMana;
        Mana = maxMana;
        ManaRegenPerSecond = manaRegenPerSecond;
    }

    /// <summary>Register an ability by id with its cooldown and mana cost.</summary>
    public void AddAbility(string id, float cooldownSeconds, float manaCost)
    {
        _abilities[id] = new Slot { CooldownSeconds = cooldownSeconds, ManaCost = manaCost };
    }

    /// <summary>Register an ability straight from classes.json data.</summary>
    public void AddAbility(AbilityData ability, float manaCost)
        => AddAbility(ability.Id, ability.CooldownSeconds, manaCost);

    /// <summary>Call once per frame with the frame's delta time in seconds.</summary>
    public void Tick(float deltaSeconds)
    {
        if (deltaSeconds <= 0) return;

        Mana = Math.Min(MaxMana, Mana + ManaRegenPerSecond * deltaSeconds);

        foreach (Slot slot in _abilities.Values)
            slot.Remaining = Math.Max(0, slot.Remaining - deltaSeconds);
    }

    /// <summary>True if the ability is off cooldown and there's enough mana.</summary>
    public bool CanUse(string id)
        => _abilities.TryGetValue(id, out Slot? slot) && slot.Remaining <= 0 && Mana >= slot.ManaCost;

    /// <summary>Spends mana and starts the cooldown. Returns false (and does nothing) if it can't be used.</summary>
    public bool TryUse(string id)
    {
        if (!CanUse(id)) return false;

        Slot slot = _abilities[id];
        Mana -= slot.ManaCost;
        slot.Remaining = slot.CooldownSeconds;
        AbilityUsed?.Invoke(id);
        return true;
    }

    /// <summary>Seconds left on the cooldown (0 = ready). For the HUD.</summary>
    public float GetCooldownRemaining(string id)
        => _abilities.TryGetValue(id, out Slot? slot) ? slot.Remaining : 0;

    /// <summary>0..1 cooldown progress (1 = just used, 0 = ready). For HUD cooldown overlays.</summary>
    public float GetCooldownPercent(string id)
        => _abilities.TryGetValue(id, out Slot? slot) && slot.CooldownSeconds > 0
            ? slot.Remaining / slot.CooldownSeconds
            : 0;

    /// <summary>Refill mana (e.g. the rest room).</summary>
    public void RestoreMana() => Mana = MaxMana;
}
