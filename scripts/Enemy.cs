using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Signal] public delegate void DiedEventHandler();
    [Export] public float MaxHealth = 30f;

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; } = false;

    protected EnemyState _currentState = EnemyState.Idle;
    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead)
            return;
        
        switch(_currentState)
        {
            case EnemyState.Idle:
                HandleIdle(delta);
                break;
            case EnemyState.Chase:
                HandleChase(delta);
                break;
            case EnemyState.Attack:
                HandleAttack(delta);
                break;
        }
        MoveAndSlide();
    }

    protected virtual void HandleIdle(double delta)
    {
        Velocity = Vector2.Zero;
    }

    protected virtual void HandleChase(double delta)
    {
        // To be overridden by child classes
    }

    protected virtual void HandleAttack(double delta)
    {
        // To be overridden by child classes
    }

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