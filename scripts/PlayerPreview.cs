using Godot;

public partial class PlayerPreview : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 160f;
    private AnimatedSprite2D _sprite;

    public override void _Ready() => _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = new(
            Axis(Key.D, Key.Right) - Axis(Key.A, Key.Left),
            Axis(Key.S, Key.Down) - Axis(Key.W, Key.Up));
        Velocity = direction.Normalized() * Speed;
        MoveAndSlide();
        _sprite.Play(direction.IsZeroApprox() ? "idle" : "walk");
        if (direction.X != 0) _sprite.FlipH = direction.X < 0;
    }

    private static float Axis(Key first, Key second) =>
        Input.IsPhysicalKeyPressed(first) || Input.IsPhysicalKeyPressed(second) ? 1f : 0f;
}

