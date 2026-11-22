using Godot;
namespace BeyondTheGoodbye;
public partial class Main
{
	private void DrawChapterDetails(Color accent, Color stone)
	{
		// Floor inlays remain subtle so the brighter attack warnings stay legible.
		Color inlay = new(accent, .17f);
		if (Run.Level == 0)
		{
			for (int side = 0; side < 2; side++)
				for (int i = 0; i < 6; i++)
				{
					float x = 75 + i * 96, y = side == 0 ? 85 : 423;
					DrawArc(new(x, y), 17, 0, Mathf.Tau, 24, inlay, 2);
					DrawArc(new(x, y), 11, 0, Mathf.Tau, 24, inlay, 1);
					DrawLine(new(x - 24, y - 13), new(x - 24, y + 13), inlay, 2);
				}
			DrawLine(new(320, 256), new(320, 194), inlay, 3);
			DrawLine(new(320, 256), new(360, 273), inlay, 3);
		}
		else if (Run.Level == 1)
		{
			for (int i = 0; i < 8; i++)
			{
				float x = 80 + i * 68;
				DrawLine(new(x, 60), new(x + 30, 125), new Color(accent, .12f), 2);
				DrawLine(new(x + 30, 125), new(x + 15, 170), new Color(accent, .1f), 1);
				DrawLine(new(x, 451), new(x - 21, 380), new Color(accent, .12f), 2);
			}
		}
		else if (Run.Level == 2)
		{
			for (int i = 0; i < 10; i++)
			{
				var at = new Vector2(75 + i * 53, 85 + i % 3 * 155);
				DrawArc(at, 25 + Mathf.Sin(_time + i) * 4, 0, Mathf.Tau, 32, new Color(accent, .09f), 1);
				DrawRect(new Rect2(at, new Vector2(8, 12)), new Color("9eafa7", .14f));
			}
		}
		else
		{
			for (int side = 0; side < 2; side++)
				for (int i = 0; i < 8; i++)
				{
					var at = new Vector2(70 + i * 71, side == 0 ? 67 : 447);
					DrawLine(at, at + new Vector2(25, side == 0 ? 25 : -25), new Color(accent, .22f), 2);
					DrawCircle(at + new Vector2(15, 7), 6, new Color("879e83", .24f));
					for (int petal = 0; petal < 5; petal++)
						DrawCircle(at + Vector2.FromAngle(petal * Mathf.Tau / 5) * 5, 3, new Color(accent, .25f));
				}
		}
		foreach (var obstacle in Room.Obstacles)
		{
			Rect2 r = obstacle.Rect;
			if (Run.Level == 1)
			{
				foreach (var corner in new[] { r.Position + new Vector2(8, 8), r.End - new Vector2(8, 8) })
					DrawCircle(corner, 3, accent.Darkened(.2f));
				DrawLine(r.Position + new Vector2(6, 18), r.End - new Vector2(6, 8), accent.Darkened(.45f), 2);
			}
			else if (Run.Level == 0 || Run.Level == 2)
			{
				for (float x = r.Position.X + 8; x < r.End.X - 8; x += 8)
					DrawRect(new Rect2(x, r.Position.Y + 17, 5, Mathf.Min(18, r.Size.Y - 23)), new Color(accent.Darkened(.3f + ((int)x % 3) * .1f), .75f));
				DrawLine(r.Position + new Vector2(5, 37), new Vector2(r.End.X - 5, r.Position.Y + 37), stone.Darkened(.2f), 2);
			}
			else
			{
				DrawLine(r.Position + new Vector2(5, 5), r.End - new Vector2(7, 5), new Color("64826b"), 2);
				DrawCircle(r.Position + r.Size / 2, 5, new Color("b49caf"));
				DrawCircle(r.Position + r.Size / 2, 2, accent);
			}
		}
	}
	private void DrawMerchant(Color accent)
	{
		var at = new Vector2(418, 236);
		DrawRect(new Rect2(at + new Vector2(-32, 28), new Vector2(82, 13)), new Color("553e38"));
		DrawRect(new Rect2(at + new Vector2(-34, 24), new Vector2(86, 8)), new Color("9f805b"));
		DrawRect(new Rect2(at + new Vector2(-37, -43), new Vector2(92, 12)), new Color("78628b"));
		for (int i = 0; i < 6; i++)
			DrawRect(new Rect2(at + new Vector2(-37 + i * 16, -43), new Vector2(8, 12)), new Color("b29ab8"));
		DrawLine(at + new Vector2(-37, -43), at + new Vector2(-37, 39), new Color("a88c61"), 3);
		DrawLine(at + new Vector2(55, -43), at + new Vector2(55, 39), new Color("a88c61"), 3);
		DrawColoredPolygon(new[] { at + new Vector2(-9, -14), at + new Vector2(10, -14), at + new Vector2(19, 23), at + new Vector2(-18, 23) }, new Color("7d8393"));
		DrawCircle(at + new Vector2(0, -18), 10, new Color("d3b698"));
		DrawArc(at + new Vector2(0, -19), 13, Mathf.Pi, Mathf.Tau, 16, new Color("444b65"), 6);
		for (int i = 0; i < 4; i++)
		{
			var dye = new Vector2(390 + i * 19, 253);
			DrawRect(new Rect2(dye, new Vector2(9, 7)), new Color(new[] { "e19478", "81bac9", "a397ce", "d3b17d" }[i]));
			DrawRect(new Rect2(dye + new Vector2(2, -3), new Vector2(5, 3)), accent);
		}
		DrawString(ThemeDB.FallbackFont, new Vector2(376, 297), "DYES + SUPPLIES", HorizontalAlignment.Left, -1, 10, accent);
	}
}
