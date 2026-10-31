using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    [Signal] public delegate void ManaChangedEventHandler(int currentMana, int maxMana);
    [Signal] public delegate void CooldownChangedEventHandler(float currentCooldown, float maxCooldown);

    [ExportGroup("Health & Mana Stats")]
    [Export] public int MaxHealth = 100;
    [Export] public int CurrentHealth = 100;
    [Export] public int MaxMana = 100;
    [Export] public int CurrentMana = 100;
    [Export] public float CurrentCooldown = 1.0f;
    [Export] public float MaxCooldown = 1.0f;

    [ExportGroup("Movement & Dodge Stats")]
	[Export] public float MoveSpeed = 200f;
    [Export] public float DodgeSpeed = 600f;
    [Export] public float DodgeDuration = 0.15f;
    [Export] public float DodgeCooldown = 1.0f;

    [ExportGroup("Input Action Names")]
    [Export] public string MoveLeftAction = "move_left";
    [Export] public string MoveRightAction = "move_right";
    [Export] public string MoveUpAction = "move_up";
    [Export] public string MoveDownAction = "move_down";
    [Export] public string DodgeAction = "dodge";



    private bool _isDodging = false;
    private float _dodgeTimer = 0f;
    private float _cooldownTimer = 0f;
    private Vector2 _dodgeDirection = Vector2.Zero;


    public override void _EnterTree()
    {
        AddToGroup("Player");
    }
    public override void _Ready()
    {

        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
        EmitSignal(SignalName.ManaChanged, CurrentMana, MaxMana);
        EmitSignal(SignalName.CooldownChanged, CurrentCooldown, MaxCooldown);

    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        if(_isDodging)
        {
            _dodgeTimer -= dt;
            Velocity = _dodgeDirection * DodgeSpeed;
            MoveAndSlide();

            if (_dodgeTimer <= 0f)
                _isDodging = false;

            return;
        }

        Vector2 inputDir = Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);

        if(Input.IsActionJustPressed(DodgeAction) && _cooldownTimer <= 0f && inputDir != Vector2.Zero)
        {
            _isDodging = true;
            _dodgeTimer = DodgeDuration;
            _cooldownTimer = DodgeCooldown;
            _dodgeDirection = inputDir.Normalized();
            return;
        }

        Velocity = inputDir * MoveSpeed;
        MoveAndSlide();
    }
    public void TakeDamage(int damageAmount)
    {
        if(_isDodging)
        {
            return;
        }

        CurrentHealth -= damageAmount;
        GD.Print($"Player took {damageAmount} damage. Health: {CurrentHealth}");

        EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        QueueFree();
    }
   
}
