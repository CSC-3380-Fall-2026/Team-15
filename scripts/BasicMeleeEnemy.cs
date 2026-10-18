using Godot;
using System;

public partial class BasicMeleeEnemy : Enemy
{
	[ExportGroup("Melee Enemy Stats")]
	[Export] public float Speed = 100.0f;
	[Export] public int AttackDamage = 10;
	[Export] public float AttackRange = 50.0f;
	[Export] public double AttackCooldown = 1.0;

	private Player _player;
	private double _timeSinceLastAttack = 1.0;

	public override void _Ready()
	{
		base._Ready();

		_player = GetTree().GetFirstNodeInGroup("Player") as Player;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead || _player == null || !IsInstanceValid(_player))
			return;

		_timeSinceLastAttack += delta;
		float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

		if (distanceToPlayer > AttackRange)
		{
			Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
			Velocity = direction * Speed;

		}
		else
		{
			Velocity = Vector2.Zero;


			if (_timeSinceLastAttack >= AttackCooldown)
			{
				Attack();
				_timeSinceLastAttack = 0.0;
			}
		}
        MoveAndSlide();
    }
	public void Attack()
	{
		_player.TakeDamage(AttackDamage);
	}
}

