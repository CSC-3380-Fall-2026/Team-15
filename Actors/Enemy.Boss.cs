using Godot;
using System;
using System.Collections.Generic;
namespace BeyondTheGoodbye;

public partial class Enemy
{
	private readonly List<(float Time, Action Action)> _bossEvents = new();
	private float _bossClock, _bossBusy, _phaseTransition;
	public int PhaseTransitions
	{
		get; private set;
	}
	public int[] BossAttackCounts { get; } = new int[3];
	public bool Reviving => _phaseTransition > 0 && !PhaseTwo;
	public bool Transforming => _phaseTransition > 0;
	private Color BossColor => Game.Run.Level switch
	{
		0 => new Color("d5bce6"),
		1 => new Color("ff9956"),
		2 => new Color("83d5ea"),
		_ => new Color("e3c582")
	};
	private void Later(float delay, Action action) => _bossEvents.Add((_bossClock + delay, action));
	public void ClearBossThreats()
	{
		_bossEvents.Clear();
		foreach (var child in Game.World.GetChildren())
			if (child is BossHazard || child is Projectile shot && shot.Hostile)
				child.QueueFree();
	}
	private void StartSecondPhase()
	{
		_phaseTransition = 2.4f;
		Burn = Poison = Slow = 0;
		_charge = 0;
		_bossBusy = 0;
		_wait = AttackWait;
		ClearBossThreats();
		Game.Toast("The first form falls... it is rising again!", 3);
		Game.FlashRing(Position, 155, BossColor);
		Game.Sound("skill");
		Sprite.Play("death");
	}
	private void TickBoss(float dt)
	{
		_bossClock += dt;
		if (_phaseTransition > 0)
		{
			_phaseTransition -= dt;
			Velocity = Vector2.Zero;
			Sprite.Modulate = new Color(BossColor, .45f + .55f * (1 - _phaseTransition / 2.4f));
			if (_phaseTransition < 1.1f)
				Sprite.Play("phase2");
			if (_phaseTransition <= 0)
			{
				PhaseTwo = true;
				PhaseTransitions++;
				Health = MaxHealth;
				_wait = AttackWait;
				Sprite.Scale = Vector2.One * 2.05f;
				Sprite.Play("idle");
				Game.FlashRing(Position, 170, BossColor);
				Game.Toast(Boss.Name + " / REBORN / PHASE II", 3);
				Game.Sound("clear");
			}
			QueueRedraw();
			return;
		}
		// Remove due callbacks before invoking: callbacks may schedule the next part.
		for (int i = _bossEvents.Count - 1; i >= 0; i--)
		{
			if (_bossEvents[i].Time > _bossClock)
				continue;
			var action = _bossEvents[i].Action;
			_bossEvents.RemoveAt(i);
			action();
		}
		var direction = (Game.Hero.Position - Position).Normalized();
		if (_charge > 0)
		{
			_charge -= dt;
			Velocity = _aim * 330;
			MoveAndSlide();
			if (Position.DistanceTo(Game.Hero.Position) < 36)
				Game.Hero.Hurt(Boss.Damage);
		}
		else if (_bossBusy > 0)
		{
			_bossBusy -= dt;
			Velocity = Vector2.Zero;
		}
		else
		{
			_wait -= dt;
			Velocity = Position.DistanceTo(Game.Hero.Position) > 165 ? direction * 43 : Vector2.Zero;
			MoveAndSlide();
			if (_wait <= 0)
				StartBossAttack();
		}
		Sprite.FlipH = direction.X < 0;
		Sprite.Modulate = _flash > 0 ? new Color(1.8f, 1.6f, 1.3f) : PhaseTwo ? BossColor.Lightened(.18f) : Colors.White;
		if (_animation <= 0)
			Sprite.Play(_charge > 0 ? "charge" : _bossBusy > 0 ? "cast" : Velocity.Length() > 3 ? "move" : "idle");
		QueueRedraw();
	}
	private BossHazard Zone(string shape, Vector2 from, Vector2 to, float delay = .9f, float duration = .25f,
		float radius = 48, float width = 24, int status = 0)
	{
		var hazard = new BossHazard
		{
			Game = Game,
			Shape = shape,
			From = from,
			To = to,
			Delay = delay,
			Duration = duration,
			Radius = radius,
			Width = width,
			Damage = Boss.Damage,
			Status = status,
			Tint = BossColor
		};
		Game.World.AddChild(hazard);
		return hazard;
	}
	private void Fan(Vector2 origin, Vector2 aim, int count, float spread)
	{
		for (int i = 0; i < count; i++)
			Game.Shoot(origin, aim.Rotated((i - (count - 1) / 2f) * spread), 170, Boss.Damage, true, 0, BossColor);
		Game.Sound("attack");
	}
	private void Sweep(Vector2 origin, Vector2 aim, float delay, float radius, bool reverse = false)
	{
		var zone = Zone("arc", origin, origin, delay, .3f, radius);
		zone.Angle = aim.Angle() + (reverse ? Mathf.Pi : 0);
		zone.Arc = 1.15f;
	}
	private void StartBossAttack()
	{
		int attack = _pattern % 3;
		_pattern++;
		BossAttackCounts[attack]++;
		_aim = (Game.Hero.Position - Position).Normalized();
		Vector2 origin = Position, target = Game.Hero.Position, aim = _aim;
		_animation = .8f;
		Sprite.Play(attack == 0 ? "attack" : "attack" + (attack + 1));
		_wait = AttackWait;
		_bossBusy = 1.5f;
		Game.Toast(Boss.Attacks[attack], 2);
		switch (Game.Run.Level)
		{
			case 0:
				if (attack == 0)
				{
					Later(.8f, () => Fan(origin, aim, 5, .24f));
					if (PhaseTwo)
						Later(1.35f, () => Fan(origin, aim.Rotated(.12f), 5, .24f));
				}
				else if (attack == 1)
				{
					Zone("circle", target, target, .9f, .3f, 42);
					Zone("circle", new(155, 260), Vector2.Zero, 1.15f, .3f, 42);
					if (PhaseTwo)
						Zone("circle", new(Mathf.Clamp(target.X + 95, 75, 565), Mathf.Clamp(target.Y - 90, 75, 437)), Vector2.Zero, 1.5f, .3f, 42);
					_bossBusy = 1.8f;
				}
				else
				{
					Sweep(origin, aim, .8f, 130);
					if (PhaseTwo)
						Sweep(origin, aim, 1.55f, 105, true);
					_bossBusy = 1.9f;
				}
				break;
			case 1:
				if (attack == 0)
				{
					Sweep(origin, aim, .8f, 155);
					if (PhaseTwo)
						Sweep(origin, aim, 1.6f, 145, true);
					_bossBusy = 2;
				}
				else if (attack == 1)
				{
					Zone("line", new(target.X, 30), new(target.X, 482), 1, .55f, width: 46, status: 1);
					if (PhaseTwo)
					{
						float x = target.X > 320 ? target.X - 100 : target.X + 100;
						Zone("line", new(x, 30), new(x, 482), 1.65f, .55f, width: 46, status: 1);
					}
					_bossBusy = 2.3f;
				}
				else
					BeginBossRush(origin, aim, false);
				break;
			case 2:
				if (attack == 0)
				{
					var wave = Zone("wave", new(24, 50), new(616, 462), 1, 2.8f, width: 26, status: 2);
					wave.GapX = 270;
					wave.GapWidth = 150;
					if (PhaseTwo)
					{
						var second = Zone("wave", new(24, 50), new(616, 462), 3.9f, 2.8f, width: 26, status: 2);
						second.GapX = 385;
						second.GapWidth = 150;
					}
					_bossBusy = PhaseTwo ? 6.8f : 3.9f;
				}
				else if (attack == 1)
				{
					int count = PhaseTwo ? 4 : 3;
					for (int i = 0; i < count; i++)
						Zone("circle", i == 0 ? target : new(115 + i * 115, 120 + i % 2 * 220), Vector2.Zero, .9f + i * .35f, .3f, 48, status: 2);
					_bossBusy = 2.4f;
				}
				else
				{
					Zone("ring", origin, origin, 1, 2.2f, 650, 18);
					if (PhaseTwo)
						Zone("ring", origin, origin, 3.5f, 2.2f, 650, 18);
					_bossBusy = PhaseTwo ? 5.8f : 3.3f;
				}
				break;
			case 3:
				if (attack == 0)
				{
					Zone("line", new(80, 65), new(560, 447), 1, .4f, width: 24);
					Zone("line", new(560, 65), new(80, 447), 1.6f, .4f, width: 24);
					if (PhaseTwo)
						Zone("line", new(30, 256), new(610, 256), 2.2f, .4f, width: 24);
					_bossBusy = 2.7f;
				}
				else if (attack == 1)
				{
					Zone("circle", target, Vector2.Zero, 1.1f, .5f, 82);
					if (PhaseTwo)
						Zone("circle", new(target.X < 320 ? 480 : 160, 256), Vector2.Zero, 1.8f, .5f, 82);
					_bossBusy = 2.4f;
				}
				else
					BeginBossRush(origin, aim, true);
				break;
		}
	}
	private void BeginBossRush(Vector2 origin, Vector2 aim, bool echo)
	{
		Vector2 end = (origin + aim * 215).Clamp(new Vector2(38, 38), new Vector2(602, 474));
		// Preview the exact path; damage belongs to the physical charging body.
		Zone("line", origin, end, .8f, .65f, width: 30).Damage = 0;
		Later(.8f, () => { _aim = aim; _charge = .65f; });
		if (PhaseTwo)
			Zone("line", origin, end, echo ? 2.2f : 1.6f, echo ? .35f : .7f, width: 26, status: echo ? 0 : 1);
		_bossBusy = PhaseTwo ? 2.6f : 1.7f;
	}
	private void DrawBossTransformation()
	{
		if (!PhaseTwo)
			return;
		Color tint = new(BossColor, .8f);
		float pulse = Mathf.Sin(_bossClock * 3) * 4;
		if (Game.Run.Level == 0)
		{
			DrawLine(new(-26, -41), new(-57, -58 - pulse), tint, 4);
			DrawLine(new(26, -41), new(57, -58 - pulse), tint, 4);
			DrawLine(new(-3, -65), new(5, -44), new Color("fff1df"), 2);
		}
		else if (Game.Run.Level == 1)
		{
			for (int i = 0; i < 7; i++)
				DrawLine(new(-30 + i * 10, -15), new(-40 + i * 13, -68 - pulse - i % 2 * 16), new Color(tint, .45f), 4);
			DrawCircle(new(0, -42), 7 + pulse / 3, new Color("ffca72"));
		}
		else if (Game.Run.Level == 2)
		{
			DrawArc(new(0, -20), 45 + pulse, .1f, Mathf.Pi - .1f, 32, tint, 4);
			DrawArc(new(0, -20), 57 - pulse, Mathf.Pi + .1f, Mathf.Tau - .1f, 32, tint, 3);
		}
		else
		{
			DrawArc(new(0, -52), 39 + pulse / 2, 0, Mathf.Tau, 32, tint, 2);
			for (int i = 0; i < 6; i++)
			{
				var at = new Vector2(0, -52) + Vector2.FromAngle(i * Mathf.Tau / 6 + _bossClock * .15f) * 48;
				DrawRect(new Rect2(at - new Vector2(5, 7), new Vector2(10, 14)), tint, false, 2);
			}
		}
	}
}
