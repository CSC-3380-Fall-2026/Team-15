using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Signal] public delegate void DiedEventHandler();
    [Export] public float MaxHealth = 30f;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; } = false;

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;}

    public void TakeDamage(float amount)
    {
       if (IsDead)
            return;

        if (amount <= 0f)
            return;

        CurrentHealth -= amount;

        if (CurrentHealth < 0f)
            CurrentHealth = 0f;

        if (CurrentHealth <= 0f)
            Die();}

    private void Die()
    {
        IsDead = true;
        EmitSignal(SignalName.Died);
        QueueFree();}
}