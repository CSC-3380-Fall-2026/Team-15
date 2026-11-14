using Godot;
using System;
using static Enemy;

public partial class BasicRangedEnemy : Enemy
{
    [Export] public PackedScene ProjectileScene { get; set; }

    [ExportGroup("Ranged Stats")]
    [Export] public float MoveSpeed = 100f;
    [Export] public float AttackRange = 250f;
    [Export] public float ChaseRange = 400f;
    [Export] public float FireRate = 1.5f;

    private Player _player;
    private float _shootTimer = 0f;

    public override void _Ready()
    {
        base._Ready(); 
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsDead || !IsInstanceValid(_player))
        {
            return;
        }

        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        // Determine State based on distance
        if (distanceToPlayer <= AttackRange)
        {
            _currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= ChaseRange)
        {
            _currentState = EnemyState.Chase;
        }
        else
        {
            _currentState = EnemyState.Idle;
        }

        // Execute the base class logic (which runs the switch statement and MoveAndSlide)
        base._PhysicsProcess(delta);
    }

    protected override void HandleIdle(double delta)
    {
        base.HandleIdle(delta); // Sets Velocity to Zero

        // Keep timer topped off so it doesn't instantly fire the moment the player enters range
        _shootTimer = FireRate;
    }

    protected override void HandleChase(double delta)
    {
        Vector2 direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
        Velocity = direction * MoveSpeed;

        _shootTimer = FireRate;
    }

    protected override void HandleAttack(double delta)
    {
        Velocity = Vector2.Zero;

        _shootTimer -= (float)delta;

        if (_shootTimer <= 0f)
        {
            Shoot();
            _shootTimer = FireRate;
        }
    }

    private void Shoot()
    {
        if (ProjectileScene == null)
        {
            GD.PushError("ProjectileScene is not assigned on RangedEnemy!");
            return;
        }

        Projectile projectile = ProjectileScene.Instantiate<Projectile>();

        // Spawn the projectile at the enemy's location
        projectile.GlobalPosition = GlobalPosition;

        // Calculate the direction toward the player
        projectile.Direction = GlobalPosition.DirectionTo(_player.GlobalPosition);

        // Add the projectile to the root scene tree so it moves independently
        GetTree().CurrentScene.AddChild(projectile);
    }
}
