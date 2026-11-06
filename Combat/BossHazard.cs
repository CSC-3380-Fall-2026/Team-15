using Godot;
namespace BeyondTheGoodbye;

// All geometry is in room coordinates. A hazard previews its actual hit shape.
public partial class BossHazard : Node2D
{
	public Main Game;
	public string Shape = "circle";
	public Vector2 From, To;
	public Color Tint = new("ddbb88");
	public float Delay = .9f, Duration = .25f, Radius = 45, Width = 20;
	public float Angle, Arc = 1.2f, GapX = 320, GapWidth = 140;
	public int Damage, Status;
	public float Age
	{
		get; private set;
	}
	private bool _hit;
	public bool ContainsPoint(Vector2 point, float progress)
	{
		if (Shape == "line")
			return Geometry2D.GetClosestPointToSegment(point, From, To).DistanceTo(point) <= Width / 2 + 9;
		if (Shape == "wave")
			return Mathf.Abs(point.Y - Mathf.Lerp(From.Y, To.Y, progress)) < Width / 2 + 9
				&& Mathf.Abs(point.X - GapX) > GapWidth / 2 - 9;
		var offset = point - From;
		if (Shape == "ring")
			return Mathf.Abs(offset.Length() - Mathf.Lerp(25, Radius, progress)) < Width / 2 + 9;
		if (Shape == "arc")
			return offset.Length() < Radius + 9 && Mathf.Abs(Mathf.AngleDifference(Angle, offset.Angle())) < Arc;
		return offset.Length() < Radius + 9;
	}
	public override void _PhysicsProcess(double delta)
	{
		if (!Game.Active || Game.Hero == null || Game.Hero.Dead)
			return;
		Age += (float)delta;
		if (Damage > 0 && Age >= Delay && !_hit && ContainsPoint(Game.Hero.Position, Mathf.Clamp((Age - Delay) / Duration, 0, 1)))
		{
			Game.Hero.Hurt(Damage, false, Status);
			_hit = true;
		}
		if (Age >= Delay + Duration)
			QueueFree();
		QueueRedraw();
	}
	public override void _Draw()
	{
		bool active = Age >= Delay;
		float progress = Mathf.Clamp((Age - Delay) / Duration, 0, 1);
		Color edge = new(Tint, active ? 1 : .75f), fill = new(Tint, active ? .45f : .13f);
		if (Shape == "line")
		{
			DrawLine(From, To, fill, Width);
			var side = (To - From).Normalized().Orthogonal() * Width / 2;
			DrawLine(From + side, To + side, edge, 2);
			DrawLine(From - side, To - side, edge, 2);
			DrawLine(From, To, edge, active ? 4 : 1);
		}
		else if (Shape == "wave")
		{
			float y = Mathf.Lerp(From.Y, To.Y, progress);
			DrawLine(new(24, y), new(GapX - GapWidth / 2, y), fill, Width);
			DrawLine(new(GapX + GapWidth / 2, y), new(616, y), fill, Width);
			DrawLine(new(24, y), new(GapX - GapWidth / 2, y), edge, 3);
			DrawLine(new(GapX + GapWidth / 2, y), new(616, y), edge, 3);
			// The entire safe corridor is visible before the wave moves.
			DrawLine(new(GapX - GapWidth / 2, 28), new(GapX - GapWidth / 2, 484), new Color(Tint, .24f), 1);
			DrawLine(new(GapX + GapWidth / 2, 28), new(GapX + GapWidth / 2, 484), new Color(Tint, .24f), 1);
		}
		else if (Shape == "ring")
		{
			float r = Mathf.Lerp(25, Radius, progress);
			DrawArc(From, r, 0, Mathf.Tau, 72, fill, Width);
			DrawArc(From, r, 0, Mathf.Tau, 72, edge, 2);
		}
		else if (Shape == "arc")
		{
			var points = new Vector2[34];
			points[0] = From;
			for (int i = 0; i <= 32; i++)
				points[i + 1] = From + Vector2.FromAngle(Angle - Arc + Arc * 2 * i / 32) * Radius;
			DrawColoredPolygon(points, fill);
			DrawArc(From, Radius, Angle - Arc, Angle + Arc, 32, edge, 2);
			DrawLine(From, points[1], edge, 2);
			DrawLine(From, points[33], edge, 2);
		}
		else
		{
			DrawCircle(From, Radius, fill);
			DrawArc(From, Radius, 0, Mathf.Tau, 40, edge, 2);
			DrawArc(From, Radius * Mathf.Clamp(Age / Delay, 0, 1), 0, Mathf.Tau, 40, edge, 1);
		}
	}
}
