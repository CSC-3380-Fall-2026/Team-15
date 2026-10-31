using Godot;
using System;

public partial class Hud : CanvasLayer
{
	private ProgressBar _healthBar;
    private ProgressBar _manaBar;
    private ProgressBar _cooldownBar;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/HealthBar");
        _manaBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/ManaBar");
        _cooldownBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/CooldownBar");

        Player player = GetTree().GetFirstNodeInGroup("Player") as Player;

        if(player != null)
        {
            player.HealthChanged += OnPlayerHealthChanged;
            player.ManaChanged += OnPlayerManaChanged;
            player.CooldownChanged += OnPlayerCooldownChanged;

            OnPlayerHealthChanged(player.CurrentHealth, player.MaxHealth);
            OnPlayerManaChanged(player.CurrentMana, player.MaxMana);
            OnPlayerCooldownChanged(player.CurrentCooldown, player.MaxCooldown);

        }

    }

    public void OnPlayerHealthChanged(int currentHealth, int maxHealth)
    {
        _healthBar.MaxValue = maxHealth;
        _healthBar.Value = currentHealth;
    }
    public void OnPlayerManaChanged(int currentMana, int maxMana)
    {
        _manaBar.MaxValue = maxMana;
        _manaBar.Value = currentMana;
    }

    public void OnPlayerCooldownChanged(float currentCooldown, float maxCooldown)
    {
        _cooldownBar.MaxValue = maxCooldown;
        _cooldownBar.Value = currentCooldown;
    }


}
