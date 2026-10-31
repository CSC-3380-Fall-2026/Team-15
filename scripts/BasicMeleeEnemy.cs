using Godot;
using System;

public partial class BasicMeleeEnemy : Enemy
{
	[ExportGroup("Melee Enemy Stats")]
	[Export] public float Speed = 100.0f;
	[Export] public int AttackDamage = 10;
	[Export] public float AttackRange = 50.0f;
	[Export] public double AttackCooldown = 1.0;
	[Export] public float DetectionRange = 300.0f;

    private Player _player;
	private double _timeSinceLastAttack = 1.0;

	public override void _Ready()
	{
		base._Ready();

		_player = GetTree().GetFirstNodeInGroup("Player") as Player;
    }

    protected override void HandleIdle(double delta)
    {
		Velocity = Vector2.Zero;

		if (_player != null && IsInstanceValid(_player))
		{
			float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

			if (distanceToPlayer <= DetectionRange)
			{ 
			_currentState = EnemyState.Chase;
			}
		}
    }

	protected override void HandleChase(double delta)
	{
		if (_player == null || !IsInstanceValid(_player))
		{
			_currentState = EnemyState.Idle;
			return;
		}

		float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        if (distanceToPlayer > DetectionRange)
        {
            _currentState = EnemyState.Idle;
			return;
        }

        if (distanceToPlayer <= AttackRange)
		{
			_currentState = EnemyState.Attack;
		}
		else
		{
			Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * Speed;
		}
	}

    protected override void HandleAttack(double delta)
    {
		Velocity = Vector2.Zero;
		_timeSinceLastAttack += delta;

		if (_player == null || !IsInstanceValid(_player) || GlobalPosition.DistanceTo(_player.GlobalPosition) > AttackRange)
		{
			_currentState = EnemyState.Chase;
			return;
		}

        if(_timeSinceLastAttack >= AttackCooldown)

            {
            Attack();
            _timeSinceLastAttack = 0.0;
        }
    }

	public void Attack()
	{
		_player.TakeDamage(AttackDamage);
	}
}

