using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace BeyondTheGoodbye;
public partial class Main : Node2D
{
	public SaveData Run; public SettingsData Settings = new(); public Player Hero; public Node2D World; public List<Enemy> Enemies = new();
	public bool Active; public float Shake; public string Screen = "title"; public RoomDef Room => GameData.Levels[Run.Level].Rooms[Run.Room];
	public readonly Vector2 WorldOrigin = new(320, 112); public Color Gold = new("d3b984"), Muted = new("8a999e"), Ink = new("0b121b");
	private AStarGrid2D _navigation; private CanvasLayer _uiLayer; private Control _ui; private Label _toastLabel; private float _toastTime, _time, _autosave;
	private string _toast = ""; private AudioStreamPlayer _music; private List<AudioStreamPlayer> _voices = new(); private Dictionary<string, AudioStream> _sounds = new(); private Texture2D _titleArt;
	private bool _testMode; private int _selectedClass, _selectedPalette; private string _overlay = ""; private List<(Vector2 Position, int Destination)> _doors = new();
	public override void _Ready()
	{
		GameData.Load();
		SetupInput();
		_testMode = OS.GetCmdlineUserArgs().Contains("--test-suite");
		if (ResourceLoader.Exists("res://Art/title.png"))
			_titleArt = GD.Load<Texture2D>("res://Art/title.png");
		_uiLayer = new CanvasLayer();
		AddChild(_uiLayer);
		foreach (string name in new[] { "attack", "hit", "dodge", "heal", "chest", "clear", "skill", "death" })
			_sounds[name] = GD.Load<AudioStream>("res://Audio/" + name + ".wav");
		for (int i = 0; i < 8; i++)
		{
			var voice = new AudioStreamPlayer();
			AddChild(voice);
			_voices.Add(voice);
		}
		_music = new AudioStreamPlayer { Stream = GD.Load<AudioStream>("res://Audio/pilgrim_theme.wav"), VolumeDb = -8 };
		AddChild(_music);
		_music.Finished += () => _music.Play();
		if (FileAccess.FileExists("user://settings.json"))
		{
			try
			{
				Settings = GameData.Read<SettingsData>("user://settings.json") ?? new();
			}
			catch { Settings = new(); }
		}
		ApplySettings();
		if (!_testMode)
			_music.Play();
		ShowTitle();
		if (_testMode)
			CallDeferred(nameof(RunTests));
		if (OS.GetCmdlineUserArgs().Contains("--capture-full"))
			CallDeferred(nameof(CaptureScreens));
	}
	private static void Bind(string action, params Key[] keys)
	{
		if (!InputMap.HasAction(action))
			InputMap.AddAction(action);
		foreach (var key in keys)
			InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
	}
	private static void SetupInput()
	{
		Bind("move_left", Key.A, Key.Left);
		Bind("move_right", Key.D, Key.Right);
		Bind("move_up", Key.W, Key.Up);
		Bind("move_down", Key.S, Key.Down);
		Bind("dodge", Key.Space);
		Bind("interact", Key.E);
		Bind("pause", Key.Escape);
		Bind("inventory", Key.I);
		Bind("map", Key.M);
		Bind("skill1", Key.Q);
		Bind("skill2", Key.F);
		Bind("skill3", Key.R);
		Bind("item1", Key.Key1);
		Bind("item2", Key.Key2);
		Bind("item3", Key.Key3);
		InputMap.AddAction("attack");
		InputMap.ActionAddEvent("attack", new InputEventMouseButton { ButtonIndex = MouseButton.Left });
	}
	public void NewRun(string classId, int palette)
	{
		var c = GameData.Classes.First(x => x.Id == classId);
		Run = new SaveData { ClassId = classId, Palette = palette, Health = c.MaxHealth, Mana = c.MaxMana, Weapon = classId + "_weapon", Settings = Settings };
		Run.Inventory[classId + "_weapon"] = 1;
		Run.Inventory["health_tonic"] = 3;
		Run.Inventory["mana_tonic"] = 2;
		Run.Inventory["fire_bomb"] = 1;
		Screen = "game";
		LoadRoom(0, 0);
		Save();
		Toast("WASD move • mouse aims • left click attacks • Space dodges. Clear the echoes.", 6);
	}
	public void ContinueRun()
	{
		var save = SaveStore.Load(out string error);
		if (save == null)
		{
			Toast(error, 8);
			return;
		}
		Run = save;
		Settings = Run.Settings;
		ApplySettings();
		if (Run.Completed)
		{
			ShowEnding();
			return;
		}
		if (Run.Health <= 0)
			Run.Health = GameData.Classes.First(c => c.Id == Run.ClassId).MaxHealth;
		Screen = "game";
		LoadRoom(Run.Level, Run.Room);
		Toast("Your journey continues.");
	}
	public void LoadRoom(int level, int room)
	{
		if (World != null)
		{
			World.ProcessMode = ProcessModeEnum.Disabled;
			World.QueueFree();
		}
		Enemies.Clear();
		Run.Level = level;
		Run.Room = room;
		_doors.Clear();
		_overlay = "";
		Active = true;
		Screen = "game";
		World = new Node2D { Position = WorldOrigin };
		AddChild(World);
		Hero = new Player { Game = this, Def = GameData.Classes.First(c => c.Id == Run.ClassId), Health = Run.Health, Mana = Run.Mana, Position = new Vector2(92, 256) };
		World.AddChild(Hero);
		Wall(new Rect2(0, 0, 640, 16));
		Wall(new Rect2(0, 496, 640, 16));
		Wall(new Rect2(0, 0, 16, 512));
		Wall(new Rect2(624, 0, 16, 512));
		foreach (var obstacle in Room.Obstacles)
			Wall(obstacle.Rect);
		_navigation = new AStarGrid2D { Region = new Rect2I(0, 0, 20, 16), CellSize = new Vector2(32, 32), Offset = new Vector2(16, 16), DiagonalMode = AStarGrid2D.DiagonalModeEnum.OnlyIfNoObstacles };
		_navigation.Update();
		for (int x = 0; x < 20; x++)
			for (int y = 0; y < 16; y++)
			{
				Vector2 center = new(x * 32 + 16, y * 32 + 16);
				bool blocked = x == 0 || y == 0 || x == 19 || y == 15 || Room.Obstacles.Any(o => o.Rect.Grow(12).HasPoint(center));
				_navigation.SetPointSolid(new Vector2I(x, y), blocked);
			}
		if (!Run.Cleared.Contains(Room.Id))
		{
			for (int i = 0; i < Room.Enemies.Length; i++)
				SpawnEnemy(Room.Enemies[i], new Vector2(450 + i % 2 * 80, 145 + i * 105));
			if (Room.Kind == "boss")
			{
				var boss = new Enemy { Game = this, Boss = GameData.Foes.Bosses[level], Position = new Vector2(455, 256) };
				World.AddChild(boss);
				Enemies.Add(boss);
				Sound("skill");
			}
		}
		int forward = 0;
		foreach (int next in Room.Links)
		{
			Vector2 position = next < room ? new Vector2(33, 256) : new Vector2(605, Room.Links.Count(n => n > room) > 1 ? (forward++ == 0 ? 170 : 345) : 256);
			_doors.Add((position, next));
		}
		if (Room.Kind == "rest")
		{
			Hero.Health = Hero.Def.MaxHealth;
			Hero.Mana = Hero.Def.MaxMana;
			MarkCleared();
			Toast("A quiet place. Health and energy restored. E at the lantern opens the supply shrine.", 5);
		}
		if (Room.Kind == "treasure")
			Toast("Two keepsakes await. Approach a chest and press E.", 4);
		BuildHud();
		QueueRedraw();
	}
	private void Wall(Rect2 rect)
	{
		var wall = new StaticBody2D { Position = rect.GetCenter(), CollisionLayer = 1 };
		wall.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = rect.Size } });
		World.AddChild(wall);
	}
	public Vector2 PathDirection(Vector2 from, Vector2 to)
	{
		var a = new Vector2I(Mathf.Clamp((int)(from.X / 32), 1, 18), Mathf.Clamp((int)(from.Y / 32), 1, 14));
		var b = new Vector2I(Mathf.Clamp((int)(to.X / 32), 1, 18), Mathf.Clamp((int)(to.Y / 32), 1, 14));
		if (_navigation.IsPointSolid(a) || _navigation.IsPointSolid(b))
			return (to - from).Normalized();
		var path = _navigation.GetPointPath(a, b);
		return path.Length > 1 ? (path[1] - from).Normalized() : (to - from).Normalized();
	}
	public void SpawnEnemy(string id, Vector2 at)
	{
		if (Room.Obstacles.Any(o => o.Rect.Grow(18).HasPoint(at)))
			at = new[] { new Vector2(500, 100), new Vector2(530, 255), new Vector2(500, 420), new Vector2(370, 90) }.First(p => !Room.Obstacles.Any(o => o.Rect.Grow(18).HasPoint(p)) && !Enemies.Any(e => e.Position.DistanceTo(p) < 35));
		var enemy = new Enemy { Game = this, Def = GameData.Foes.Enemies.First(e => e.Id == id), Position = at };
		World.AddChild(enemy);
		Enemies.Add(enemy);
	}
	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_time += dt;
		_toastTime = Mathf.Max(0, _toastTime - dt);
		if (_toastLabel != null && IsInstanceValid(_toastLabel))
			_toastLabel.Text = _toastTime > 0 ? _toast : "";
		if (Screen == "game")
		{
			if (Input.IsActionJustPressed("pause"))
			{
				if (_overlay != "")
					Resume();
				else
					ShowPause();
			}
			if (Input.IsActionJustPressed("inventory"))
			{
				if (_overlay == "inventory")
					Resume();
				else if (Active)
					ShowInventory();
			}
			if (Input.IsActionJustPressed("map"))
			{
				if (_overlay == "map")
					Resume();
				else if (Active)
					ShowMap();
			}
			if (Active)
			{
				if (Input.IsActionJustPressed("interact"))
					Interact();
				_autosave += dt;
				if (_autosave >= 30)
				{
					_autosave = 0;
					Save();
				}
				Shake = Mathf.Max(0, Shake - dt);
				World.Position = WorldOrigin + (Settings.Shake && Shake > 0 ? new Vector2(Mathf.Sin(_time * 92), Mathf.Cos(_time * 79)) * Shake * 20 : Vector2.Zero);
				UpdateHud();
			}
		}
		QueueRedraw();
	}
	private void Interact()
	{
		if (Room.Kind == "rest" && (Hero.Position.DistanceTo(new Vector2(320, 256)) < 90 || Hero.Position.DistanceTo(new Vector2(418, 260)) < 90))
		{
			ShowShop();
			return;
		}
		if (Room.Kind == "treasure")
		{
			for (int i = 0; i < 2; i++)
				if (Hero.Position.DistanceTo(new Vector2(270 + i * 130, 250)) < 58)
				{
					OpenChest(i);
					return;
				}
		}
		if (Enemies.Any(e => IsInstanceValid(e) && !e.Dead))
		{
			Toast("The gates are sealed while enemies remain.");
			return;
		}
		if (Room.Kind == "boss" && Hero.Position.DistanceTo(new Vector2(320, 70)) < 65)
		{
			AdvanceChapter();
			return;
		}
		foreach (var door in _doors)
		{
			if (Hero.Position.DistanceTo(door.Position) > 52)
				continue;
			MarkCleared();
			SyncRun();
			LoadRoom(Run.Level, door.Destination);
			Save();
			return;
		}
		Toast("Approach a gate, chest, or resting lantern to interact.");
	}
	public void OpenChest(int index)
	{
		string key = Room.Id + ":" + index;
		if (Run.Chests.Contains(key))
		{
			Toast("This keepsake has already been collected.");
			return;
		}
		Run.Chests.Add(key);
		Run.Gold += 35 + Run.Level * 10;
		string loot = index == 0 ? new[] { "health_tonic", "mana_tonic", "ward_draught", "haste_draught" }[Run.Level] : new[] { "fire_bomb", "antidote", "health_tonic", "mana_tonic" }[Run.Level];
		AddItem(loot, 2);
		if (index == 1)
		{
			AddItem(Run.ClassId + "_armor", 1);
			Run.Armor = Run.ClassId + "_armor";
		}
		Sound("chest");
		FlashRing(new Vector2(270 + index * 130, 250), 70, Gold);
		Toast("Keepsake found: 2 " + GameData.Items.First(i => i.Id == loot).Name + ", gold" + (index == 1 ? ", and your class armor." : "."), 5);
		if (Run.Chests.Contains(Room.Id + ":0") && Run.Chests.Contains(Room.Id + ":1"))
			MarkCleared();
		Save();
	}
	public void AddItem(string id, int count) => Run.Inventory[id] = Mathf.Min(999, Run.Inventory.GetValueOrDefault(id) + count);
	public bool UseItem(string id)
	{
		if (Hero == null || Hero.Dead || Run.Inventory.GetValueOrDefault(id) <= 0)
		{
			Toast("You do not have that item.");
			return false;
		}
		var item = GameData.Items.First(i => i.Id == id);
		if (item.Kind != "consumable")
			return false;
		if (id == "health_tonic" && Hero.Health >= Hero.Def.MaxHealth)
		{
			Toast("Health is already full.");
			return false;
		}
		if (id == "mana_tonic" && Hero.Mana >= Hero.Def.MaxMana)
		{
			Toast("Energy is already full.");
			return false;
		}
		Run.Inventory[id]--;
		switch (id)
		{
			case "health_tonic":
				Hero.Heal(60);
				break;
			case "mana_tonic":
				Hero.Mana = Mathf.Min(Hero.Def.MaxMana, Hero.Mana + 65);
				Sound("heal");
				break;
			case "antidote":
				Hero.Burn = Hero.Slow = Hero.Poison = 0;
				Sound("heal");
				break;
			case "fire_bomb":
				AreaAttack(Hero.Position, 170, 42, 1);
				Sound("skill");
				break;
			case "ward_draught":
				Hero.Invulnerable = 4;
				break;
			case "haste_draught":
				Hero.Haste = 8;
				break;
		}
		Toast(item.Name + " used.");
		return true;
	}
	public void EnemyDefeated(Enemy enemy)
	{
		Run.Gold += enemy.IsBoss ? 80 + Run.Level * 20 : enemy.Def.Gold;
		Sound("hit");
		if (enemy.IsBoss)
		{
			if (!Run.Bosses.Contains(Run.Level))
				Run.Bosses.Add(Run.Level);
			Hero.Heal(35);
		}
		if (!Enemies.Any(e => IsInstanceValid(e) && !e.Dead))
		{
			MarkCleared();
			Sound("clear");
			Toast(Room.Kind == "boss" ? "The memory is released. E at the northern light to continue." : "Room cleared. The gates are open.", 5);
			Save();
		}
	}
	private void MarkCleared()
	{
		if (!Run.Cleared.Contains(Room.Id))
			Run.Cleared.Add(Room.Id);
	}
	public void AdvanceChapter()
	{
		if (!Run.Bosses.Contains(Run.Level))
			return;
		SyncRun();
		if (Run.Level == 3)
		{
			Run.Completed = true;
			Save();
			ShowEnding();
			return;
		}
		int next = Run.Level + 1;
		Hero.Health = Hero.Def.MaxHealth;
		Hero.Mana = Hero.Def.MaxMana;
		SyncRun();
		LoadRoom(next, 0);
		Save();
		Toast(GameData.Levels[next].Memory, 6);
	}
	private void SyncRun()
	{
		if (Hero == null || !IsInstanceValid(Hero))
			return;
		Run.Health = Hero.Health;
		Run.Mana = Hero.Mana;
		Run.Settings = Settings;
	}
	public void Save()
	{
		if (Run == null || _testMode)
			return;
		SyncRun();
		if (!SaveStore.Write(Run, out string error))
			Toast(error, 6);
	}
	public void Die()
	{
		if (Hero == null || !Hero.Dead)
			return;
		Active = false;
		World.ProcessMode = ProcessModeEnum.Disabled;
		Sound("death");
		ShowDeath();
	}
	public void Retry()
	{
		Run.Health = Hero.Def.MaxHealth;
		Run.Mana = Hero.Def.MaxMana;
		Run.Gold = Mathf.Max(0, Run.Gold - 15);
		LoadRoom(Run.Level, Run.Room);
		Save();
		Toast("The lantern still burns. 15 gold offered to return.");
	}
	public int Melee(Vector2 position, Vector2 aim, float radius, int damage, float arc, float stun = 0)
	{
		int hits = 0;
		World.AddChild(new VisualEffect { Position = position, Kind = "slash", Direction = aim, Radius = radius, Tint = Gold, Life = .2f });
		foreach (var e in Enemies)
		{
			if (!IsInstanceValid(e) || e.Dead)
				continue;
			Vector2 offset = e.Position - position;
			if (offset.Length() < radius && aim.Dot(offset.Normalized()) > arc)
			{
				e.Hit(damage, 0, stun);
				hits++;
			}
		}
		return hits;
	}
	public void AreaAttack(Vector2 position, float radius, int damage, int status)
	{
		FlashRing(position, radius, status == 2 ? new Color("a9c6df") : Gold);
		foreach (var e in Enemies)
			if (IsInstanceValid(e) && !e.Dead && e.Position.DistanceTo(position) < radius)
				e.Hit(damage, status);
	}
	public void Shoot(Vector2 position, Vector2 direction, float speed, int damage, bool hostile, int status, Color tint, bool piercing = false) => World.AddChild(new Projectile { Game = this, Position = position, Direction = direction.Normalized(), Speed = speed, Damage = damage, Hostile = hostile, Status = status, Tint = tint, Piercing = piercing });
	public void Hazard(Vector2 at, float radius, float delay, int damage, int status) => World.AddChild(new DangerZone { Game = this, Position = at, Radius = radius, Delay = delay, Damage = damage, Status = status });
	public void Floating(Vector2 at, string text, Color tint) => World.AddChild(new VisualEffect { Position = at, Kind = "text", Text = text, Tint = tint, Life = .7f });
	public void FlashRing(Vector2 at, float radius, Color tint) => World.AddChild(new VisualEffect { Position = at, Radius = radius, Tint = tint, Life = .55f });
	public void Particles(Vector2 at, Color tint, int count) => World.AddChild(new VisualEffect { Position = at, Kind = "particles", Tint = tint, Radius = 35, Count = count, Life = .45f });
	public void Toast(string text, float seconds = 3)
	{
		_toast = text;
		_toastTime = seconds;
	}
	public void Sound(string name)
	{
		if (_testMode)
			return;
		var voice = _voices.FirstOrDefault(v => !v.Playing) ?? _voices[0];
		voice.Stream = _sounds[name];
		voice.VolumeDb = -9;
		voice.Play();
	}
	public void ApplySettings()
	{
		Settings.Volume = Mathf.Clamp(float.IsFinite(Settings.Volume) ? Settings.Volume : .65f, 0, 1);
		AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(Mathf.Max(.0001f, Settings.Volume)));
		if (!_testMode)
			DisplayServer.WindowSetMode(Settings.Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
	}
	private void StoreSettings()
	{
		using var f = FileAccess.Open("user://settings.json", FileAccess.ModeFlags.Write);
		f?.StoreString(System.Text.Json.JsonSerializer.Serialize(Settings));
		Save();
	}
}
