using Godot;
using System.Collections.Generic;
namespace BeyondTheGoodbye;
public partial class Projectile : Node2D
{
	public Main Game; public Vector2 Direction; public float Speed; public int Damage, Status; public bool Hostile, Piercing; public Color Tint;
	private float _life = 3.5f; private readonly HashSet<ulong> _hit = new();
	public override void _PhysicsProcess(double delta)
	{
		if (!Game.Active)
			return;
		float dt = (float)delta;
		_life -= dt;
		Vector2 old = Position;
		Position += Direction * Speed * dt;
		var query = PhysicsRayQueryParameters2D.Create(Game.World.ToGlobal(old), GlobalPosition, 1);
		if (_life <= 0 || Position.X < 16 || Position.X > 624 || Position.Y < 16 || Position.Y > 496 || GetWorld2D().DirectSpaceState.IntersectRay(query).Count > 0)
		{
			QueueFree();
			return;
		}
		if (Hostile)
		{
			if (Geometry2D.GetClosestPointToSegment(Game.Hero.Position, old, Position).DistanceTo(Game.Hero.Position) < 13)
			{
				Game.Hero.Hurt(Damage, false, Status);
				QueueFree();
			}
		}
		else
			foreach (var enemy in Game.Enemies)
			{
				if (!IsInstanceValid(enemy) || enemy.Dead || _hit.Contains(enemy.GetInstanceId()))
					continue;
				if (Geometry2D.GetClosestPointToSegment(enemy.Position, old, Position).DistanceTo(enemy.Position) < (enemy.IsBoss ? 26 : 15))
				{
					enemy.Hit(Damage, Status);
					_hit.Add(enemy.GetInstanceId());
					if (!Piercing)
					{
						QueueFree();
						break;
					}
				}
			}
		QueueRedraw();
	}
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, Hostile ? 8 : 6, new Color(Tint, .13f));
		DrawLine(-Direction * 12, Direction * 4, Tint, Hostile ? 4 : 3);
		DrawCircle(Vector2.Zero, 2, Colors.White);
	}
}
public partial class DangerZone : Node2D
{
	public Main Game; public float Radius, Delay; public int Damage, Status; private float _age; private bool _fired;
	public override void _Process(double delta)
	{
		if (!Game.Active)
			return;
		_age += (float)delta;
		if (!_fired && _age >= Delay)
		{
			_fired = true;
			if (Position.DistanceTo(Game.Hero.Position) < Radius + 9)
				Game.Hero.Hurt(Damage, false, Status);
			Game.Particles(Position, new Color("e0a480"), 12);
		}
		if (_age > Delay + .25f)
			QueueFree();
		QueueRedraw();
	}
	public override void _Draw()
	{
		DrawCircle(Vector2.Zero, Radius, new Color(.9f, .42f, .3f, _fired ? .27f : .09f));
		DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 48, new Color("d6977a"), 2);
		DrawArc(Vector2.Zero, Radius * Mathf.Clamp(_age / Delay, 0, 1), 0, Mathf.Tau, 48, new Color(1, .7f, .4f, .65f), 2);
		DrawLine(new Vector2(-5, 0), new Vector2(5, 0), new Color("e9c4a2"), 2);
		DrawLine(new Vector2(0, -5), new Vector2(0, 5), new Color("e9c4a2"), 2);
	}
}
public partial class VisualEffect : Node2D
{
	public string Kind = "ring", Text = ""; public Color Tint; public float Radius = 50, Life = .5f; public Vector2 Direction; public int Count = 8;
	private float _age;
	public override void _Process(double delta)
	{
		_age += (float)delta;
		if (_age >= Life)
			QueueFree();
		QueueRedraw();
	}
	public override void _Draw()
	{
		float t = Mathf.Clamp(_age / Life, 0, 1);
		Color c = new(Tint, 1 - t);
		if (Kind == "text")
		{
			DrawString(ThemeDB.FallbackFont, new Vector2(-12, -25 - t * 24), Text, HorizontalAlignment.Left, -1, 16, c);
			return;
		}
		if (Kind == "slash")
		{
			DrawArc(Vector2.Zero, Radius * (.8f + t * .2f), Direction.Angle() - 1.1f, Direction.Angle() + 1.1f, 24, c, 4);
			return;
		}
		if (Kind == "particles")
		{
			for (int i = 0; i < Count; i++)
			{
				Vector2 at = Vector2.FromAngle(i * Mathf.Tau / Count) * t * Radius;
				DrawRect(new Rect2(at, Vector2.One * (3 - t * 2)), c);
			}
			return;
		}
		DrawArc(Vector2.Zero, Radius * t, 0, Mathf.Tau, 48, c, 2);
	}
}
