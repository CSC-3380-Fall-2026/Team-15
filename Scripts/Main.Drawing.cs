using Godot;
using System;
using System.Linq;
namespace BeyondTheGoodbye;
public partial class Main
{
	public override void _Draw()
	{
		DrawRect(new Rect2(0, 0, 1280, 720), new Color("0a121c"));
		if (Screen != "game")
		{
			if (_titleArt != null)
				DrawTextureRect(_titleArt, new Rect2(0, 0, 1280, 720), false);
			for (int i = 0; i < 64; i++)
				DrawRect(new Rect2(i * 20, 0, 20, 720), new Color(.018f, .035f, .055f, Mathf.Clamp(.84f - i * .016f, .06f, .84f)));
			if (Screen == "classes" || Screen == "settings" || Screen == "help" || Screen == "credits")
				DrawRect(new Rect2(0, 0, 1280, 720), new Color(.025f, .045f, .065f, .65f));
			for (int i = 0; i < 25; i++)
			{
				float x = (i * 139.7f + _time * (5 + i % 3)) % 1280;
				float y = 720 - ((i * 83 + _time * (9 + i % 4)) % 720);
				DrawCircle(new Vector2(x, y), i % 3 == 0 ? 2 : 1, new Color(.88f, .72f, .39f, .25f + .15f * Mathf.Sin(_time + i)));
			}
			return;
		}
		if (Run == null || World == null)
			return;
		DrawRect(new Rect2(22, 111, 274, 546), new Color("0f1c25"));
		DrawRect(new Rect2(975, 111, 287, 546), new Color("0f1c25"));
		DrawLine(new Vector2(34, 102), new Vector2(1245, 102), new Color("39484a"));
		DrawLine(new Vector2(38, 451), new Vector2(278, 451), new Color("39484a"));
		DrawSetTransform(World.Position);
		var accent = new Color(GameData.Levels[Run.Level].AccentColor);
		var stone = new Color(GameData.Levels[Run.Level].FloorColor);
		DrawRect(new Rect2(-7, -7, 654, 526), new Color("070e16"));
		DrawRect(new Rect2(-3, -3, 646, 518), stone.Darkened(.65f));
		DrawRect(new Rect2(0, 0, 640, 512), new Color("10181e"));
		for (int x = 0; x < 20; x++)
			for (int y = 0; y < 16; y++)
			{
				int hash = (x * 173 + y * 71 + Run.Level * 83 + Run.Room * 31) % 17;
				Color tile = stone.Darkened(.75f + hash * .002f);
				var cell = new Rect2(x * 32 + 1, y * 32 + 1, 30, 30);
				DrawRect(cell, tile);
				DrawLine(cell.Position, cell.Position + new Vector2(29, 0), tile.Lightened(.018f));
				if (hash < 3)
				{
					DrawLine(new Vector2(x * 32 + 4, y * 32 + 9), new Vector2(x * 32 + 14, y * 32 + 15), tile.Darkened(.25f));
					DrawLine(new Vector2(x * 32 + 14, y * 32 + 15), new Vector2(x * 32 + 13, y * 32 + 24), tile.Darkened(.25f));
				}
				if (hash == 7)
					DrawRect(new Rect2(x * 32 + 21, y * 32 + 6, 3, 2), accent.Darkened(.65f));
			}
		// Inlaid procession path and central mosaic vary by chapter.
		DrawRect(new Rect2(38, 233, 562, 47), new Color(accent.Darkened(.78f), .6f));
		DrawLine(new Vector2(38, 234), new Vector2(600, 234), accent.Darkened(.65f));
		DrawLine(new Vector2(38, 279), new Vector2(600, 279), accent.Darkened(.65f));
		DrawArc(new Vector2(320, 256), 93, 0, Mathf.Tau, 64, accent.Darkened(.69f), 2);
		DrawArc(new Vector2(320, 256), 82, 0, Mathf.Tau, 64, accent.Darkened(.76f), 1);
		for (int i = 0; i < 8; i++)
		{
			Vector2 a = new Vector2(320, 256) + Vector2.FromAngle(i * Mathf.Pi / 4) * 70;
			DrawLine(a, a + Vector2.FromAngle(i * Mathf.Pi / 4) * 18, accent.Darkened(.66f), 2);
		}
		if (Run.Level == 2)
		{
			for (int i = 0; i < 6; i++)
				DrawArc(new Vector2(150 + i * 76, 420 + i % 2 * 13), 29 + Mathf.Sin(_time + i) * 3, 0, Mathf.Pi, 24, new Color(.36f, .54f, .7f, .15f), 1);
		}
		for (int x = 0; x < 20; x++)
		{
			DrawRect(new Rect2(x * 32, 0, 31, 21), stone.Darkened(.59f));
			DrawRect(new Rect2(x * 32, 496, 31, 16), stone.Darkened(.64f));
			DrawLine(new Vector2(x * 32, 22), new Vector2(x * 32 + 31, 22), stone.Darkened(.3f), 2);
		}
		for (int y = 0; y < 16; y++)
		{
			DrawRect(new Rect2(0, y * 32, 16, 31), stone.Darkened(.6f));
			DrawRect(new Rect2(624, y * 32, 16, 31), stone.Darkened(.6f));
		}
		for (int i = 0; i < 5; i++)
		{
			float x = 95 + i * 112;
			DrawRect(new Rect2(x - 13, 23, 26, 20), new Color("09131d"));
			DrawArc(new Vector2(x, 25), 14, Mathf.Pi, Mathf.Tau, 16, stone.Darkened(.4f), 3);
			DrawLine(new Vector2(x, 16), new Vector2(x, 40), stone.Darkened(.53f), 2);
		}
		foreach (var o in Room.Obstacles)
		{
			Rect2 r = o.Rect;
			DrawRect(new Rect2(r.Position + new Vector2(6, 9), r.Size), new Color(0, 0, 0, .28f));
			DrawRect(r, stone.Darkened(.58f));
			DrawRect(new Rect2(r.Position - new Vector2(3, 4), new Vector2(r.Size.X + 6, 14)), stone.Darkened(.28f));
			DrawRect(new Rect2(r.Position + new Vector2(4, 13), r.Size - new Vector2(8, 18)), stone.Darkened(.45f));
			DrawLine(r.Position + new Vector2(8, 17), r.Position + new Vector2(8, r.Size.Y - 6), stone.Darkened(.23f), 2);
		}
		DrawChapterDetails(accent, stone);
		for (int i = 0; i < 4; i++)
		{
			Vector2 p = new(i < 2 ? 40 : 600, i % 2 == 0 ? 72 : 436);
			DrawRect(new Rect2(p - new Vector2(5, 2), new Vector2(10, 17)), stone.Darkened(.3f));
			Glow(p - new Vector2(0, 8), accent, 30);
			DrawCircle(p - new Vector2(0, 8), 3 + Mathf.Sin(_time * 5 + i), accent);
			DrawLine(p - new Vector2(0, 7), p - new Vector2(0, 15), new Color("fff0bd"), 2);
		}
		bool locked = Enemies.Any(e => IsInstanceValid(e) && !e.Dead);
		foreach (var door in _doors)
		{
			Vector2 p = door.Position;
			Color c = locked ? new Color("a46e62") : accent;
			Glow(p, c, 31);
			DrawRect(new Rect2(p - new Vector2(12, 30), new Vector2(24, 60)), new Color("08131b"));
			DrawRect(new Rect2(p - new Vector2(13, 31), new Vector2(26, 62)), c.Darkened(.35f), false, 2);
			if (locked)
				for (int i = -1; i <= 1; i++)
					DrawLine(p + new Vector2(i * 7, -27), p + new Vector2(i * 7, 27), c, 2);
			else
				DrawString(ThemeDB.FallbackFont, p + new Vector2(-6, 5), (door.Destination + 1).ToString(), HorizontalAlignment.Left, -1, 15, c);
		}
		if (Room.Kind == "rest")
		{
			DrawMerchant(accent);
			Vector2 p = new(320, 256);
			DrawArc(p, 62, 0, Mathf.Tau, 48, accent.Darkened(.2f), 2);
			Glow(p, accent, 73);
			DrawRect(new Rect2(307, 239, 26, 34), stone.Darkened(.15f));
			DrawCircle(p - new Vector2(0, 17), 9, new Color("eedca7"));
			DrawArc(p - new Vector2(0, 17), 15, Mathf.Pi, Mathf.Tau, 20, accent, 2);
		}
		if (Room.Kind == "treasure")
			for (int i = 0; i < 2; i++)
			{
				bool open = Run.Chests.Contains(Room.Id + ":" + i);
				Vector2 p = new(270 + i * 130, 250);
				if (!open)
					Glow(p, Gold, 38);
				DrawRect(new Rect2(p - new Vector2(21, 9), new Vector2(42, 26)), new Color("6c5544"));
				DrawRect(new Rect2(p - new Vector2(23, open ? 25 : 17), new Vector2(46, 15)), new Color("977651"));
				DrawRect(new Rect2(p - new Vector2(3, 15), new Vector2(6, 31)), Gold.Darkened(.15f));
				DrawRect(new Rect2(p - new Vector2(6, 3), new Vector2(12, 8)), Gold);
			}
		if (Room.Kind == "boss" && !locked)
		{
			Vector2 p = new(320, 70);
			Glow(p, Gold, 61);
			DrawArc(p, 30, 0, Mathf.Tau, 48, Gold, 3);
			DrawArc(p, 23, -_time, _time + Mathf.Pi, 32, new Color("eee3b6"), 2);
		}
		for (int i = 0; i < 22; i++)
		{
			float x = 35 + (i * 127 + Run.Room * 41) % 570;
			float y = 35 + (i * 83 + _time * (4 + i % 3)) % 440;
			DrawCircle(new Vector2(x, y), 1, new Color(accent, .16f + .13f * Mathf.Sin(_time + i)));
		}
		DrawSetTransform(Vector2.Zero);
	}
	private void Glow(Vector2 point, Color color, float radius)
	{
		for (int i = 5; i >= 1; i--)
			DrawCircle(point, radius * i / 5f, new Color(color, .016f * (6 - i)));
	}
}
