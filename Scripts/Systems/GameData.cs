using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
namespace BeyondTheGoodbye;
public class SkillDef
{
	public string Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public int Cost
	{
		get; set;
	}
	public float Cooldown
	{
		get; set;
	}
	public string Description
	{
		get; set;
	}
}
public class ClassDef
{
	public string Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public string Weapon
	{
		get; set;
	}
	public int MaxHealth
	{
		get; set;
	}
	public int MaxMana
	{
		get; set;
	}
	public float Speed
	{
		get; set;
	}
	public int Damage
	{
		get; set;
	}
	public float AttackCooldown
	{
		get; set;
	}
	public string Passive
	{
		get; set;
	}
	public string PassiveDescription
	{
		get; set;
	}
	public SkillDef[] Skills
	{
		get; set;
	}
}
public class EnemyDef
{
	public string Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public int Health
	{
		get; set;
	}
	public int Damage
	{
		get; set;
	}
	public float Speed
	{
		get; set;
	}
	public int Behavior
	{
		get; set;
	}
	public int Gold
	{
		get; set;
	}
}
public class BossDef
{
	public string Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public int Health
	{
		get; set;
	}
	public int Damage
	{
		get; set;
	}
	public string[] Attacks
	{
		get; set;
	}
}
public class FoeCatalog
{
	public EnemyDef[] Enemies
	{
		get; set;
	}
	public BossDef[] Bosses
	{
		get; set;
	}
	public string PhaseTrigger
	{
		get; set;
	}
	public float PhaseOneWait
	{
		get; set;
	}
	public float PhaseTwoWait
	{
		get; set;
	}
}
public class ItemDef
{
	public string Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public string Kind
	{
		get; set;
	}
	public string Description
	{
		get; set;
	}
	public int Power
	{
		get; set;
	}
	public int Price
	{
		get; set;
	}
	public string ClassId
	{
		get; set;
	}
}
public class ObstacleDef
{
	public float X
	{
		get; set;
	}
	public float Y
	{
		get; set;
	}
	public float W
	{
		get; set;
	}
	public float H
	{
		get; set;
	}
	public Rect2 Rect => new(X, Y, W, H);
}
public class RoomDef
{
	public Vector2 PlayerSpawn = new(92,256);
	public Vector2 BossSpawn = new(455,256);
	public Vector2[] EnemySpawns = System.Array.Empty<Vector2>();
	public int Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public string Kind
	{
		get; set;
	}
	public int[] Links
	{
		get; set;
	}
	public string[] Enemies
	{
		get; set;
	}
	public ObstacleDef[] Obstacles
	{
		get; set;
	}
	public int Chests
	{
		get; set;
	}
}
public class LevelDef
{
	public int Id
	{
		get; set;
	}
	public string Name
	{
		get; set;
	}
	public string Memory
	{
		get; set;
	}
	public string FloorColor
	{
		get; set;
	}
	public string AccentColor
	{
		get; set;
	}
	public RoomDef[] Rooms
	{
		get; set;
	}
}
public class AnimationDef
{
	public string Name
	{
		get; set;
	}
	public int[] Frames
	{
		get; set;
	}
	public float Fps
	{
		get; set;
	}
	public bool Loop
	{
		get; set;
	}
}
public class ActorArt
{
	public string Id
	{
		get; set;
	}
	public string Texture
	{
		get; set;
	}
	public int AuthoredFrames
	{
		get; set;
	}
	public int Slots
	{
		get; set;
	}
	public AnimationDef[] Animations
	{
		get; set;
	}
}
public static class GameData
{
	public static ClassDef[] Classes; public static FoeCatalog Foes; public static ItemDef[] Items; public static LevelDef[] Levels; public static ActorArt[] Art;
	private static readonly Dictionary<string, SpriteFrames> Frames = new();
	public static readonly Color[] Palettes = { Colors.White, new("bcd9ff"), new("ffcbb0"), new("d1baff") };
	public static T Read<T>(string path) => JsonSerializer.Deserialize<T>(FileAccess.GetFileAsString(path));
	public static void Load()
	{
		Classes = ClassesCatalog.Create();
		Foes = FoesCatalog.Create();
		Items = ItemsCatalog.Create();
		Levels = LevelCatalog.Create();
		Art = ArtCatalog.Create();
	}
	public static SpriteFrames GetFrames(string id)
	{
		if (Frames.TryGetValue(id, out var cached))
			return cached;
		var def = Art.First(a => a.Id == id);
		var texture = GD.Load<Texture2D>(def.Texture);
		var frames = new SpriteFrames();
		frames.RemoveAnimation("default");
		foreach (var a in def.Animations)
		{
			frames.AddAnimation(a.Name);
			frames.SetAnimationSpeed(a.Name, a.Fps);
			frames.SetAnimationLoopMode(a.Name, a.Loop ? SpriteFrames.LoopMode.Linear : SpriteFrames.LoopMode.None);
			foreach (int n in a.Frames)
				frames.AddFrame(a.Name, new AtlasTexture { Atlas = texture, Region = new Rect2(n % 8 * 64, n / 8 * 64, 64, 64) });
		}
		Frames[id] = frames;
		return frames;
	}
}
public class SettingsData
{
	public float Volume { get; set; } = .65f; public bool Fullscreen
	{
		get; set;
	}
	public bool Shake { get; set; } = true;
}
public class SaveData
{
	public string BodyColor { get; set; } = "";
	public string WeaponColor { get; set; } = "e6dbc0";
	public int OutfitPattern { get; set; }
	public int WeaponFinish { get; set; }
	public List<string> OwnedCosmetics { get; set; } = new();
	public int Version { get; set; } = 1; public string ClassId { get; set; } = "guardian"; public int Palette
	{
		get; set;
	}
	public int Level
	{
		get; set;
	}
	public int Room
	{
		get; set;
	}
	public int Health { get; set; } = 160; public float Mana { get; set; } = 80; public int Gold
	{
		get; set;
	}
	public List<int> Cleared { get; set; } = new(); public List<string> Chests { get; set; } = new(); public List<int> Bosses { get; set; } = new();
	public Dictionary<string, int> Inventory { get; set; } = new(); public string[] QuickSlots { get; set; } = { "health_tonic", "mana_tonic", "fire_bomb" };
	public string Weapon { get; set; } = "guardian_weapon"; public string Armor { get; set; } = ""; public bool Completed
	{
		get; set;
	}
	public SettingsData Settings { get; set; } = new();
}
public static class SaveStore
{
	public static string Path = "user://journey.json";
	public static bool Exists => FileAccess.FileExists(Path);
	public static bool Validate(SaveData s)
	{
		if (s == null || s.Version != 1 || s.Level < 0 || s.Level >= GameData.Levels.Length || s.Room < 0 || s.Room >= GameData.Levels[s.Level].Rooms.Length)
			return false;
		if (s.BodyColor == null || (s.BodyColor != "" && !Cosmetics.ValidHex(s.BodyColor)) || !Cosmetics.ValidHex(s.WeaponColor)
			|| s.OutfitPattern < 0 || s.OutfitPattern > 2 || s.WeaponFinish < 0 || s.WeaponFinish > 2
			|| s.OwnedCosmetics == null || s.OwnedCosmetics.Any(id => !Cosmetics.Packs.Any(p => p.Id == id)))
			return false;
		var c = GameData.Classes.FirstOrDefault(x => x.Id == s.ClassId);
		if (c == null || s.Health < 0 || s.Health > c.MaxHealth || !float.IsFinite(s.Mana) || s.Mana < 0 || s.Mana > c.MaxMana || s.Palette < 0 || s.Palette > 3 || s.Gold < 0)
			return false;
		if (s.Cleared == null || s.Chests == null || s.Bosses == null || s.Inventory == null || s.QuickSlots?.Length != 3 || s.Settings == null)
			return false;
		if (s.Cleared.Any(i => !GameData.Levels.Any(l => l.Rooms.Any(r => r.Id == i))) || s.Bosses.Any(i => i < 0 || i >= GameData.Levels.Length))
			return false;
		if (s.Inventory.Any(kv => !GameData.Items.Any(i => i.Id == kv.Key) || kv.Value < 0 || kv.Value > 999))
			return false;
		if (s.QuickSlots.Any(id => !GameData.Items.Any(i => i.Id == id && i.Kind == "consumable")))
			return false;
		if (!GameData.Items.Any(i => i.Id == s.Weapon && i.Kind == "weapon" && i.ClassId == s.ClassId))
			return false;
		if (s.Armor != "" && !GameData.Items.Any(i => i.Id == s.Armor && i.Kind == "armor" && i.ClassId == s.ClassId))
			return false;
		return float.IsFinite(s.Settings.Volume) && s.Settings.Volume >= 0 && s.Settings.Volume <= 1;
	}
	public static SaveData Load(out string error)
	{
		error = "";
		try
		{
			if (!Exists)
			{
				error = "No saved journey yet.";
				return null;
			}
			var s = GameData.Read<SaveData>(Path);
			if (!Validate(s))
				throw new Exception("Invalid save values or version.");
			return s;
		}
		catch (Exception ex) { error = "Could not read save. Your file is preserved. " + ex.Message; return null; }
	}
	public static bool Write(SaveData s, out string error)
	{
		error = "";
		try
		{
			if (!Validate(s))
				throw new Exception("Save validation failed.");
			using (var f = FileAccess.Open(Path + ".tmp", FileAccess.ModeFlags.Write))
			{
				if (f == null)
					throw new Exception(FileAccess.GetOpenError().ToString());
				f.StoreString(JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true }));
				f.Flush();
			}
			System.IO.File.Move(ProjectSettings.GlobalizePath(Path) + ".tmp", ProjectSettings.GlobalizePath(Path), true);
			return true;
		}
		catch (Exception ex) { error = "Save failed: " + ex.Message; return false; }
	}
}
