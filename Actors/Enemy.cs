using Godot;
using System;
using System.Linq;
namespace BeyondTheGoodbye;
public partial class Enemy : CharacterBody2D
{
	public Main Game; public EnemyDef Def; public BossDef Boss; public AnimatedSprite2D Sprite;
	public int Health, MaxHealth; public bool IsBoss => Boss != null; public bool PhaseTwo; public bool Dead => Health <= 0 && !Reviving;
	public float Burn, Slow, Poison, Stun; public int CompletedAttacks => _pattern; public float AttackWait => PhaseTwo ? GameData.Foes.PhaseTwoWait : GameData.Foes.PhaseOneWait;
	private float _wait = 1.4f, _windup, _charge, _flash, _animation, _statusTick, _pathTime; private int _pattern; private Vector2 _aim, _path; private bool _awarded;
	public override void _Ready()
	{
		MaxHealth = Health = IsBoss ? Boss.Health : (int)(Def.Health * (1 + Game.Run.Level * .14f));
		CollisionLayer = 4;
		CollisionMask = 1;
		AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = IsBoss ? 22 : 11 } });
		Sprite = new AnimatedSprite2D { SpriteFrames = GameData.GetFrames(IsBoss ? Boss.Id : Def.Id), Position = new Vector2(0, IsBoss ? -22 : -15), Scale = Vector2.One * (IsBoss ? 1.6f : 1), TextureFilter = TextureFilterEnum.Nearest };
		AddChild(Sprite);
		Sprite.Play("idle");
	}
	public override void _PhysicsProcess(double delta)
	{
		if (Dead || !Game.Active || Game.Hero.Dead)
			return;
		float dt = (float)delta;
		if (Reviving)
		{
			TickBoss(dt);
			return;
		}
		_flash = Mathf.Max(0, _flash - dt);
		_animation = Mathf.Max(0, _animation - dt);
		Burn = Mathf.Max(0, Burn - dt);
		Poison = Mathf.Max(0, Poison - dt);
		Slow = Mathf.Max(0, Slow - dt);
		Stun = Mathf.Max(0, Stun - dt);
		_statusTick += dt;
		if (_statusTick >= 1)
		{
			_statusTick = 0;
			if (Burn > 0)
				Hit(6);
			if (Poison > 0)
				Hit(4);
			if (Dead)
				return;
		}
		if (IsBoss)
		{
			TickBoss(dt);
			return;
		}
		if (Stun > 0)
		{
			Velocity = Vector2.Zero;
			QueueRedraw();
			return;
		}
		Vector2 offset = Game.Hero.Position - Position;
		float distance = offset.Length();
		Vector2 direction = offset.Normalized();
		_pathTime -= dt;
		if (_pathTime <= 0)
		{
			_pathTime = .3f;
			_path = Game.PathDirection(Position, Game.Hero.Position);
		}
		if (_charge > 0)
		{
			_charge -= dt;
			Velocity = _aim * (IsBoss ? 330 : 220);
		}
		else if (_windup > 0)
		{
			Velocity = Vector2.Zero;
			_windup -= dt;
			if (_windup <= 0)
				ExecuteAttack();
		}
		else
		{
			_wait -= dt;
			float speed = IsBoss ? 45 + Game.Run.Level * 5 : Def.Speed;
			int behavior = IsBoss ? -1 : Def.Behavior;
			Vector2 move = _path;
			if (behavior == 2 || behavior == 3 || behavior == 6)
				move = distance < 145 ? -direction : distance < 245 ? Vector2.Zero : _path;
			if (behavior == 4 && distance < 100)
				move = Vector2.Zero;
			if (IsBoss && distance < 190)
				move = Vector2.Zero;
			Velocity = move * speed * (Slow > 0 ? .5f : 1);
			if (_wait <= 0 && (IsBoss || distance < (behavior == 0 ? 42 : behavior == 1 ? 170 : behavior == 4 ? 95 : behavior == 7 ? 115 : 340)))
				BeginAttack(direction);
		}
		foreach (var other in Game.Enemies)
		{
			if (other == this || !IsInstanceValid(other) || other.Dead)
				continue;
			Vector2 away = Position - other.Position;
			if (away.Length() > 1 && away.Length() < 25)
				Velocity += away.Normalized() * 30;
		}
		MoveAndSlide();
		if (_charge > 0 && distance < (IsBoss ? 36 : 25))
			Game.Hero.Hurt(IsBoss ? Boss.Damage : Def.Damage, false, IsBoss ? 0 : Def.Behavior == 1 ? 3 : 0);
		if (_animation <= 0)
			Sprite.Play(_windup > 0 ? "cast" : _charge > 0 ? "charge" : Velocity.Length() > 3 ? "move" : "idle");
		Sprite.FlipH = direction.X < 0;
		Sprite.Modulate = _flash > 0 ? new Color(1.8f, 1.6f, 1.3f) : Colors.White;
		QueueRedraw();
	}
	private void BeginAttack(Vector2 direction)
	{
		_aim = direction;
		_windup = IsBoss ? .7f : .5f;
		_animation = _windup;
		Sprite.Play("cast");
		if (Def.Behavior == 5)
			Game.Hazard(Game.Hero.Position, 48, .9f, Def.Damage, 1);
	}
	private void ExecuteAttack()
	{
		Sprite.Play(IsBoss && _pattern % 3 > 0 ? "attack" + (_pattern % 3 + 1) : "attack");
		_animation = .4f;
		switch (Def.Behavior)
		{
			case 0:
				if (Position.DistanceTo(Game.Hero.Position) < 48)
					Game.Hero.Hurt(Def.Damage);
				break;
			case 1:
				_charge = .5f;
				break;
			case 2:
				Game.Shoot(Position, _aim, 215, Def.Damage, true, 0, new Color("e8c08c"));
				break;
			case 3:
				for (int i = -1; i <= 1; i++)
					Game.Shoot(Position, _aim.Rotated(i * .2f), 140, Def.Damage, true, 2, new Color("a2d0dd"));
				break;
			case 4:
				Game.Hazard(Position + _aim * 45, 65, .25f, Def.Damage, 0);
				break;
			case 5:
				break;
			case 6:
				var target = Game.Enemies.Where(e => IsInstanceValid(e) && !e.Dead && e != this).OrderBy(e => e.Health / (float)e.MaxHealth).FirstOrDefault();
				if (target != null)
				{
					target.Health = Mathf.Min(target.MaxHealth, target.Health + 15);
					Game.FlashRing(target.Position, 30, new Color("bdc894"));
				}
				Game.Shoot(Position, _aim, 135, Def.Damage, true, 3, new Color("b2c088"));
				break;
			case 7:
				Game.Hazard(Position, 115, .3f, Def.Damage, 2);
				break;
		}
		_pattern++;
		_wait = Def.Behavior == 0 ? 1.1f : Def.Behavior == 6 ? 3.5f : 2.2f;
	}
	public void Hit(int damage, int status = 0, float stun = 0)
	{
		if (Dead || Transforming)
			return;
		Health = Mathf.Max(0, Health - damage);
		_flash = .15f;
		Stun = IsBoss ? 0 : Mathf.Max(Stun, stun);
		if (status == 1)
			Burn = 5;
		if (status == 2)
			Slow = 4;
		if (status == 3)
			Poison = 6;
		Game.Floating(Position, damage.ToString(), new Color("e8d5a5"));
		Game.Particles(Position, new Color("e5b789"), 5);
		if (Health == 0 && IsBoss && !PhaseTwo)
		{
			StartSecondPhase();
			return;
		}
		if (Health == 0 && !_awarded)
		{
			_awarded = true;
			Sprite.Play("death");
			CollisionLayer = 0;
			if (IsBoss)
				ClearBossThreats();
			Game.EnemyDefeated(this);
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate:a", 0f, .55f);
			tween.TweenCallback(Callable.From(QueueFree));
		}
	}
	public override void _Draw()
	{
		if (Dead)
			return;
		if (IsBoss)
			DrawBossTransformation();
		if (_windup > 0)
		{
			DrawCircle(Vector2.Zero, IsBoss ? 36 : 23, new Color(.9f, .55f, .35f, .12f));
			DrawArc(Vector2.Zero, IsBoss ? 36 : 23, 0, Mathf.Tau, 32, new Color("e6a977"), 2);
			DrawLine(Vector2.Zero, _aim * (IsBoss ? 110 : 48), new Color(1, .7f, .4f, .5f), 2);
		}
		if (!IsBoss && Health < MaxHealth)
		{
			DrawRect(new Rect2(-17, -44, 34, 3), new Color("242b32"));
			DrawRect(new Rect2(-17, -44, 34 * Health / (float)MaxHealth, 3), new Color("c5a283"));
		}
		if (Burn > 0)
			DrawCircle(new Vector2(16, -15), 3, new Color("efa269"));
		if (Poison > 0)
			DrawCircle(new Vector2(-16, -15), 3, new Color("a9b97b"));
	}
}
