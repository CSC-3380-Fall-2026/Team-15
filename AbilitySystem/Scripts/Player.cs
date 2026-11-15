using Godot;
namespace BeyondTheGoodbye;
public partial class Player : CharacterBody2D
{
    public Main Game; public ClassDef Def; public AnimatedSprite2D Sprite;
    public int Health; public float Mana; public Vector2 Aim = Vector2.Right;
    public float DodgeRemaining, Invulnerable, Haste, Empower, Burn, Poison, Slow, DodgeTime;
    public float[] Cooldowns = new float[3];
    public bool Dead => Health <= 0; public bool IsDodging => DodgeTime > 0;
    public float DamageMultiplier => (Game.Run.Weapon == Def.Id + "_weapon" ? 1.15f : 1) * (Empower > 0 ? 1.4f : 1) * (Def.Id == "ranger" && _still > .8f ? 1.35f : 1);
    private float _attackTime, _hurt, _still, _statusTick, _animation; private Vector2 _dodgeDirection;
    public override void _Ready()
    {
        CollisionLayer = 2;
        CollisionMask = 1;
        AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 10 } });
        Sprite = new AnimatedSprite2D { SpriteFrames = GameData.GetFrames(Def.Id), Position = new Vector2(0, -15), TextureFilter = TextureFilterEnum.Nearest, Material = Cosmetics.Material(Game.Run), Scale = Vector2.One * 1.22f };
        AddChild(Sprite);
        Sprite.Play("idle");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (Dead || !Game.Active)
            return;
        float dt = (float)delta;
        DodgeRemaining = Mathf.Max(0, DodgeRemaining - dt);
        DodgeTime = Mathf.Max(0, DodgeTime - dt);
        Invulnerable = Mathf.Max(0, Invulnerable - dt);
        _hurt = Mathf.Max(0, _hurt - dt);
        Haste = Mathf.Max(0, Haste - dt);
        Empower = Mathf.Max(0, Empower - dt);
        Burn = Mathf.Max(0, Burn - dt);
        Poison = Mathf.Max(0, Poison - dt);
        Slow = Mathf.Max(0, Slow - dt);
        _attackTime = Mathf.Max(0, _attackTime - dt);
        _animation = Mathf.Max(0, _animation - dt);
        for (int i = 0; i < 3; i++)
            Cooldowns[i] = Mathf.Max(0, Cooldowns[i] - dt);
        Mana = Mathf.Min(Def.MaxMana, Mana + dt * (Def.Id == "arcanist" ? 6 : 3));
        _statusTick += dt;
        if (_statusTick >= 1)
        {
            _statusTick = 0;
            if (Burn > 0)
                Hurt(3, true);
            if (Poison > 0)
                Hurt(2, true);
        }
        Vector2 mouse = GetGlobalMousePosition() - GlobalPosition;
        if (mouse.Length() > 4)
            Aim = mouse.Normalized();
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        _still = direction.IsZeroApprox() ? _still + dt : 0;
        if (Input.IsActionJustPressed("dodge"))
            Dodge(direction.IsZeroApprox() ? Aim : direction);
        Velocity = IsDodging ? _dodgeDirection * 590 : direction * Def.Speed * (Slow > 0 ? .58f : 1) * (Haste > 0 ? 1.45f : 1);
        MoveAndSlide();
        if (Input.IsActionPressed("attack") && new Rect2(Game.WorldOrigin, new Vector2(640, 512)).HasPoint(GetGlobalMousePosition()))
            BasicAttack();
        for (int i = 0; i < 3; i++)
            if (Input.IsActionJustPressed("skill" + (i + 1)))
                UseSkill(i);
        for (int i = 0; i < 3; i++)
            if (Input.IsActionJustPressed("item" + (i + 1)))
                Game.UseItem(Game.Run.QuickSlots[i]);
        if (_animation <= 0)
            Sprite.Play(IsDodging ? "dodge" : direction.IsZeroApprox() ? "idle" : "move");
        Sprite.FlipH = Aim.X < 0;
        Sprite.Modulate = _hurt > 0 ? new Color(1.7f, 1.3f, 1.3f) : Colors.White;
        QueueRedraw();
    }
    public void RefreshAppearance()
    {
        Cosmetics.Update((ShaderMaterial)Sprite.Material, Game.Run);
        Sprite.Modulate = Colors.White;
    }
    public bool Dodge(Vector2 direction)
    {
        if (DodgeRemaining > 0)
            return false;
        _dodgeDirection = direction.IsZeroApprox() ? Vector2.Right : direction.Normalized();
        DodgeTime = .2f;
        DodgeRemaining = 2;
        Game.Sound("dodge");
        Game.FlashRing(Position, 35, new Color("8bd1c3"));
        return true;
    }
    public void BasicAttack()
    {
        if (_attackTime > 0 || Dead)
            return;
        _attackTime = Def.AttackCooldown;
        Animate("attack", .25f);
        Game.Sound("attack");
        int damage = (int)(Def.Damage * DamageMultiplier);
        if (Def.Id == "guardian")
        {
            int hits = Game.Melee(Position, Aim, 80, damage, .05f);
            if (hits > 0)
                Mana = Mathf.Min(Def.MaxMana, Mana + 2 * hits);
        }
        else
            Game.Shoot(Position + Aim * 16, Aim, Def.Id == "ranger" ? 470 : 350, damage, false, 0, Def.Id == "ranger" ? new Color("d5cd91") : new Color("b7a1ef"));
    }
    public bool UseSkill(int index)
    {
        var s = Def.Skills[index];
        if (Cooldowns[index] > 0 || Mana < s.Cost || Dead)
        {
            Game.Toast("Not enough energy, or skill is cooling down.");
            return false;
        }
        Mana -= s.Cost;
        Cooldowns[index] = s.Cooldown;
        Animate("skill" + (index + 1), .4f);
        Game.Sound("skill");
        switch (s.Id)
        {
            case "bash":
                Game.Melee(Position, Aim, 115, (int)(44 * DamageMultiplier), -.4f, 2);
                break;
            case "bulwark":
                Invulnerable = 3;
                Game.FlashRing(Position, 80, new Color("80c6c0"));
                break;
            case "rally":
                Heal(45);
                Empower = 6;
                break;
            case "pierce":
                Game.Shoot(Position, Aim, 650, (int)(65 * DamageMultiplier), false, 0, new Color("f1d696"), true);
                break;
            case "volley":
                for (int i = -3; i <= 3; i++)
                    Game.Shoot(Position, Aim.Rotated(i * .14f), 430, (int)(23 * DamageMultiplier), false, 0, new Color("ddcb8f"));
                break;
            case "retreat":
                DodgeRemaining = 0;
                Dodge(-Aim);
                Game.AreaAttack(Position, 130, 18, 2);
                break;
            case "ember":
                Game.Shoot(Position, Aim, 440, (int)(55 * DamageMultiplier), false, 1, new Color("f1a57b"), true);
                break;
            case "nova":
                Game.AreaAttack(Position, 180, (int)(48 * DamageMultiplier), 2);
                break;
            case "heal":
                Heal(55);
                Burn = Poison = Slow = 0;
                break;
        }
        return true;
    }
    public void Animate(string name, float duration)
    {
        _animation = duration;
        Sprite.Play(name);
    }
    public void Hurt(int damage, bool dot = false, int status = 0)
    {
        if (Dead || Invulnerable > 0 || IsDodging || (!dot && _hurt > 0))
            return;
        float mitigation = (Def.Id == "guardian" ? .8f : 1) * (Game.Run.Armor != "" ? .85f : 1);
        int actual = Mathf.Max(1, (int)(damage * mitigation));
        Health = Mathf.Max(0, Health - actual);
        if (!dot)
        {
            _hurt = .55f;
            Animate("hurt", .18f);
            Game.Sound("hit");
            Game.Shake = .16f;
        }
        if (status == 1)
            Burn = 4;
        if (status == 2)
            Slow = 3;
        if (status == 3)
            Poison = 5;
        Game.Floating(Position, "-" + actual, new Color("ef9b90"));
        if (Dead)
        {
            Sprite.Play("death");
            Game.CallDeferred(nameof(Main.Die));
        }
    }
    public void Heal(int amount)
    {
        Health = Mathf.Min(Def.MaxHealth, Health + amount);
        Game.Floating(Position, "+" + amount, new Color("a7d7a7"));
        Game.FlashRing(Position, 55, new Color("a7d7a7"));
        Game.Sound("heal");
    }
    public override void _Draw()
    {
        if (Invulnerable > 0)
            DrawArc(Vector2.Zero, 23, 0, Mathf.Tau, 32, new Color(.6f, .9f, 1, .8f), 2);
        if (Slow > 0)
            DrawArc(Vector2.Zero, 17, 0, Mathf.Tau, 24, new Color("a6bddd"), 2);
        DrawLine(Aim * 22, Aim * 28, new Color(1, .9f, .65f, .7f), 2);
    }
}

