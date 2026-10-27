using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
namespace BeyondTheGoodbye;
public partial class Main
{
	private Label _health, _energy, _status, _roomName, _objective, _bossText;
	private Label[] _skillLabels = new Label[3], _itemLabels = new Label[3];
	private ProgressBar _healthBar, _energyBar, _bossBar; private int _assignSlot;
	private readonly SystemFont _serif = new() { FontNames = new[] { "Georgia", "Times New Roman" } };
	private void ResetUi()
	{
		if (_ui != null)
		{
			_ui.Hide();
			_ui.QueueFree();
		}
		_ui = new Control { Size = new Vector2(1280, 720), MouseFilter = Control.MouseFilterEnum.Ignore };
		_uiLayer.AddChild(_ui);
		_toastLabel = Text("", 40, 678, 1200, 14, Gold);
		_toastLabel.HorizontalAlignment = HorizontalAlignment.Center;
	}
	private Label Text(string text, float x, float y, float width, int size = 18, Color? color = null, bool serif = false)
	{
		var label = new Label { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 0), AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = Control.MouseFilterEnum.Ignore };
		label.AddThemeFontSizeOverride("font_size", size);
		label.AddThemeColorOverride("font_color", color ?? new Color("d5ddda"));
		if (serif)
			label.AddThemeFontOverride("font", _serif);
		_ui.AddChild(label);
		label.SetDeferred(Control.PropertyName.Size, new Vector2(width, 0));
		return label;
	}
	private StyleBoxFlat Box(Color fill, Color border, int width = 1) => new() { BgColor = fill, BorderColor = border, BorderWidthLeft = width, BorderWidthRight = width, BorderWidthTop = width, BorderWidthBottom = width, CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3, ContentMarginLeft = 12, ContentMarginRight = 12, ContentMarginTop = 7, ContentMarginBottom = 7 };
	private void Panel(float x, float y, float w, float h, Color? fill = null)
	{
		var panel = new Panel { Position = new Vector2(x, y), Size = new Vector2(w, h), MouseFilter = Control.MouseFilterEnum.Ignore };
		panel.AddThemeStyleboxOverride("panel", Box(fill ?? new Color("111d27"), new Color("33424a")));
		_ui.AddChild(panel);
	}
	private Button Button(string caption, float x, float y, float w, Action action, float h = 42, bool primary = false)
	{
		var button = new Button { Text = caption, Position = new Vector2(x, y), Size = new Vector2(w, h), MouseDefaultCursorShape = Control.CursorShape.PointingHand };
		button.AddThemeFontSizeOverride("font_size", 15);
		button.AddThemeColorOverride("font_color", primary ? new Color("111922") : new Color("d8ddcd"));
		button.AddThemeStyleboxOverride("normal", Box(primary ? Gold : new Color("15232d"), primary ? Gold : new Color("475754")));
		button.AddThemeStyleboxOverride("hover", Box(primary ? new Color("ead3a7") : new Color("293b40"), Gold));
		button.AddThemeStyleboxOverride("pressed", Box(new Color("637d75"), Gold));
		button.AddThemeStyleboxOverride("focus", Box(new Color(0, 0, 0, 0), Gold, 2));
		button.Pressed += action;
		_ui.AddChild(button);
		return button;
	}
	private ProgressBar Bar(float x, float y, float w, float h, Color color)
	{
		var bar = new ProgressBar { Position = new Vector2(x, y), Size = new Vector2(w, h), ShowPercentage = false, MouseFilter = Control.MouseFilterEnum.Ignore };
		bar.AddThemeStyleboxOverride("background", new StyleBoxFlat { BgColor = new Color("19252d") });
		bar.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = color });
		_ui.AddChild(bar);
		bar.SetDeferred(Control.PropertyName.Size, new Vector2(w, h));
		return bar;
	}
	public void ShowTitle()
	{
		Active = false;
		Screen = "title";
		_overlay = "";
		_toastTime = 0;
		if (World != null)
		{
			World.QueueFree();
			World = null;
			Hero = null;
		}
		ResetUi();
		Text("AN ACTION ADVENTURE ABOUT LOSS & HOPE", 76, 102, 570, 13, Gold);
		Text("Beyond\nthe Goodbye", 70, 145, 690, 66, new Color("eee5ce"), true);
		Text("Carry what matters.\nLeave the darkness behind.", 78, 332, 430, 20, new Color("aebbb9"));
		Button("Begin a new journey", 78, 427, 310, ShowClassSelection, 48, true);
		Button("Continue your journey", 78, 488, 310, ContinueRun, 48).Disabled = !SaveStore.Exists;
		Button("Settings", 78, 551, 146, () => ShowSettings(false), 38);
		Button("How to play", 242, 551, 146, () => ShowHelp(false), 38);
		Button("Credits", 78, 601, 146, ShowCredits, 34);
		Button("Quit", 242, 601, 146, () => GetTree().Quit(), 34);
		Text("GROUP 15 / COMPLETE CAMPAIGN", 940, 666, 290, 12, Muted);
		QueueRedraw();
	}
	private void ShowClassSelection()
	{
		Screen = "classes";
		ResetUi();
		Text("Choose your light", 70, 46, 850, 40, Gold, true);
		Text("Three paths through the same goodbye. Each has a weapon, three skills, and a passive.", 72, 104, 1110, 17, Muted);
		for (int i = 0; i < 3; i++)
		{
			int pick = i;
			var c = GameData.Classes[i];
			float x = 70 + i * 383;
			Panel(x, 155, 357, 379, i == _selectedClass ? new Color("22332f") : new Color("121e28"));
			var sprite = new AnimatedSprite2D { SpriteFrames = GameData.GetFrames(c.Id), Position = new Vector2(x + 176, 235), Scale = new Vector2(2, 2), Material = Cosmetics.Material(new SaveData { Palette = _selectedPalette }) };
			sprite.Play("idle");
			_ui.AddChild(sprite);
			Text(c.Name, x + 22, 291, 310, 27, Gold, true);
			Text(c.Weapon + " / " + c.MaxHealth + " HP", x + 22, 332, 310, 15);
			Text(c.Passive + ": " + c.PassiveDescription, x + 22, 368, 307, 14, Muted);
			Text(string.Join(" / ", c.Skills.Select(s => s.Name)), x + 22, 425, 310, 13, new Color("bcc6c0"));
			Button(i == _selectedClass ? "Selected" : "Choose " + c.Name, x + 22, 476, 313, () => { _selectedClass = pick; ShowClassSelection(); }, 38, i == _selectedClass);
		}
		Text("OUTFIT PALETTE", 74, 557, 170, 12, Muted);
		string[] names = { "Original", "Moonlit", "Ember", "Dusk" };
		for (int i = 0; i < 4; i++)
		{
			int p = i;
			Button(names[i], 228 + i * 124, 548, 112, () => { _selectedPalette = p; ShowClassSelection(); }, 36, i == _selectedPalette);
		}
		Text(SaveStore.Exists ? "Starting replaces the single saved journey." : "Your journey is saved automatically as you explore.", 74, 608, 685, 14, Muted);
		Button("Back", 74, 640, 140, ShowTitle, 38);
		Button("Carry the lantern", 877, 596, 330, StartJourney, 54, true);
	}
	private void StartJourney()
	{
		NewRun(GameData.Classes[_selectedClass].Id, _selectedPalette);
		Modal("The broken watch", "prologue");
		Text("One more moment. That is all you wanted.", 151, 172, 950, 26, Gold, true);
		Text("Someone you love has died. Their watch has stopped, but you still carry it.\n\nBeyond your home, a lantern burns at the entrance to a forgotten house. They say its deepest rooms hold a way to turn back time.\n\nYou have brought a weapon, a little courage, and a promise you do not yet know how to keep.", 153, 246, 905, 21, new Color("cbd2c9"));
		Button("Follow the lantern", 152, 548, 400, Resume, 48, true);
	}
	private void ShowAppearance()
	{
		if (Hero == null || Hero.Dead)
			return;
		Modal("The dye atelier", "appearance");
		Text("OUTFIT + WEAPON / " + Hero.Def.Name.ToUpper(), 151, 146, 920, 14, Muted);
		Panel(151, 190, 325, 332, new Color("1b2c36"));
		var preview = new AnimatedSprite2D { SpriteFrames = GameData.GetFrames(Run.ClassId), Position = new Vector2(312, 341), Scale = new Vector2(3.6f, 3.6f), Material = Cosmetics.Material(Run), TextureFilter = TextureFilterEnum.Nearest };
		preview.Play("idle");
		_ui.AddChild(preview);
		Text("LIVE PREVIEW", 212, 479, 240, 13, Gold);
		Text("Starter outfit palettes", 515, 184, 540, 17, Gold);
		string[] names = { "Original", "Moonlit", "Ember", "Dusk" };
		for (int i = 0; i < 4; i++)
		{
			int pick = i;
			Button(names[i], 515 + i * 146, 217, 136, () => { SetAppearance(pick); ShowAppearance(); }, 37, Run.BodyColor == "" && Run.Palette == i);
		}
		Text("OUTFIT COLOR", 515, 279, 270, 13, Muted);
		Text("WEAPON COLOR", 816, 279, 270, 13, Muted);
		void Picker(bool weapon, float x)
		{
			var picker = new ColorPickerButton { Position = new Vector2(x, 309), Size = new Vector2(275, 48), EditAlpha = false, Color = weapon ? new Color(Run.WeaponColor) : Run.BodyColor == "" ? GameData.Palettes[Run.Palette] : new Color(Run.BodyColor) };
			picker.ColorChanged += color => { SetDye(weapon, color.ToHtml(false)); Cosmetics.Update((ShaderMaterial)preview.Material, Run); };
			_ui.AddChild(picker);
		}
		Picker(false, 515); Picker(true, 816);
		Text("OUTFIT DESIGN", 515, 379, 280, 13, Muted);
		Text("WEAPON FINISH", 515, 466, 280, 13, Muted);
		string[] patterns = { "Plain", "Gilded trim", "Brocade" };
		string[] finishes = { "Classic", "Runic glow", "Silver bands" };
		for (int i = 0; i < 3; i++)
		{
			int pick = i;
			Button(patterns[i], 515 + i * 197, 407, 184, () => { Run.OutfitPattern = pick; Hero.RefreshAppearance(); Save(); ShowAppearance(); }, 35, Run.OutfitPattern == i).Disabled = i != 0 && !Cosmetics.Packs.Any(p => p.Pattern == i && Run.OwnedCosmetics.Contains(p.Id));
			Button(finishes[i], 515 + i * 197, 494, 184, () => { Run.WeaponFinish = pick; Hero.RefreshAppearance(); Save(); ShowAppearance(); }, 35, Run.WeaponFinish == i).Disabled = i != 0 && !Cosmetics.Packs.Any(p => p.Finish == i && Run.OwnedCosmetics.Contains(p.Id));
		}
		Button("Dye shop / " + Run.Gold + " gold", 151, 556, 325, ShowCosmeticShop, 48, true);
		Text("Mix any colors freely. Buy new designs with earned gold.\nChanges save automatically. Equipment stays in I / Satchel.", 515, 559, 590, 15, Muted);
	}
	private void SetAppearance(int palette)
	{
		if (palette < 0 || palette >= GameData.Palettes.Length)
			return;
		Run.Palette = palette;
		Run.BodyColor = "";
		_selectedPalette = palette;
		Hero.RefreshAppearance();
		Save();
	}
	private bool SetDye(bool weapon, string hex)
	{
		if (!Cosmetics.ValidHex(hex))
			return false;
		if (weapon) Run.WeaponColor = hex; else Run.BodyColor = hex;
		Hero.RefreshAppearance();
		Save();
		return true;
	}
	private bool BuyCosmetic(string id)
	{
		var pack = Cosmetics.Packs.FirstOrDefault(p => p.Id == id);
		if (pack == null || Run.OwnedCosmetics.Contains(id) || Run.Gold < pack.Price)
			return false;
		Run.Gold -= pack.Price;
		Run.OwnedCosmetics.Add(id);
		Save();
		Sound("chest");
		return true;
	}
	private void WearCosmetic(string id)
	{
		var pack = Cosmetics.Packs.FirstOrDefault(p => p.Id == id);
		if (pack == null || !Run.OwnedCosmetics.Contains(id))
			return;
		Run.BodyColor = pack.Body; Run.WeaponColor = pack.Weapon;
		Run.OutfitPattern = pack.Pattern; Run.WeaponFinish = pack.Finish;
		Hero.RefreshAppearance(); Save();
	}
	private void ShowCosmeticShop()
	{
		Modal("The lantern merchant", "cosmetic_shop");
		Text("DESIGN COLLECTIONS / " + Run.Gold + " GOLD", 151, 145, 940, 15, Gold);
		Text("Buy once per journey. Mix unlocked designs with any outfit and weapon colors.", 151, 176, 950, 16, Muted);
		for (int i = 0; i < Cosmetics.Packs.Length; i++)
		{
			var pack = Cosmetics.Packs[i];
			float x = 151 + i * 245;
			Panel(x, 221, 229, 331, new Color("192a32"));
			var display = new SaveData { ClassId = Run.ClassId, BodyColor = pack.Body, WeaponColor = pack.Weapon, OutfitPattern = pack.Pattern, WeaponFinish = pack.Finish };
			var sprite = new AnimatedSprite2D { SpriteFrames = GameData.GetFrames(Run.ClassId), Position = new Vector2(x + 112, 304), Scale = Vector2.One * 2.2f, Material = Cosmetics.Material(display), TextureFilter = TextureFilterEnum.Nearest };
			sprite.Play("idle"); _ui.AddChild(sprite);
			Text(pack.Name, x + 12, 374, 205, 22, Gold, true);
			Text(pack.Description, x + 12, 410, 202, 14, Muted);
			bool owned = Run.OwnedCosmetics.Contains(pack.Id);
			Button(owned ? "Owned / Wear" : "Buy / " + pack.Price + " gold", x + 12, 498, 205, () => { if (owned || BuyCosmetic(pack.Id)) WearCosmetic(pack.Id); ShowCosmeticShop(); }, 38, owned).Disabled = !owned && Run.Gold < pack.Price;
		}
		Button("Customize colors", 151, 576, 450, ShowAppearance, 42, true);
		Button("Supplies at rest lanterns", 626, 576, 485, ShowShop, 42).Disabled = Room.Kind != "rest";
	}
	private void BuildHud()
	{
		ResetUi();
		Text("BEYOND THE GOODBYE", 34, 26, 370, 22, Gold, true);
		Text("CHAPTER " + (Run.Level + 1).ToString("00") + " / " + GameData.Levels[Run.Level].Name.ToUpper(), 34, 66, 650, 12, Muted);
		_roomName = Text(Room.Name, 320, 75, 640, 20, new Color("d8decf"), true);
		_roomName.HorizontalAlignment = HorizontalAlignment.Center;
		Text(Hero.Def.Name, 37, 138, 250, 30, Gold, true);
		Text(Hero.Def.Weapon, 39, 179, 240, 14, Muted);
		_health = Text("", 39, 221, 230, 14);
		_healthBar = Bar(39, 249, 239, 10, new Color("bf816f"));
		_energy = Text("", 39, 281, 240, 14);
		_energyBar = Bar(39, 309, 239, 8, new Color("7da6b8"));
		_status = Text("", 39, 349, 240, 15);
		Text(Hero.Def.Passive.ToUpper(), 39, 466, 240, 12, Gold);
		Text(Hero.Def.PassiveDescription, 39, 490, 237, 14, Muted);
		Button("I  Satchel", 38, 568, 115, ShowInventory, 34);
		Button("M  Map", 165, 568, 115, ShowMap, 34);
		Button("Colors + Dye shop", 38, 614, 242, ShowAppearance, 34);
		Text("ARTS", 992, 125, 245, 12, Gold);
		for (int i = 0; i < 3; i++)
		{
			var s = Hero.Def.Skills[i];
			_skillLabels[i] = Text("", 991, 155 + i * 101, 250, 16);
			Text(s.Description, 991, 202 + i * 101, 244, 13, Muted);
		}
		Text("QUICK ITEMS", 992, 470, 245, 12, Gold);
		for (int i = 0; i < 3; i++)
			_itemLabels[i] = Text("", 992, 499 + i * 31, 250, 14);
		Text("WASD move / Mouse attack\nSpace dodge / E interact\nQ F R skills / 1 2 3 items", 991, 601, 260, 12, Muted);
		_objective = Text("", 320, 636, 640, 14, new Color("bbc7bc"));
		_objective.HorizontalAlignment = HorizontalAlignment.Center;
		_bossText = Text("", 370, 117, 540, 15, Gold);
		_bossText.HorizontalAlignment = HorizontalAlignment.Center;
		_bossBar = Bar(383, 145, 514, 7, new Color("c78c72"));
		Button("Esc  Pause", 1104, 30, 136, ShowPause, 32);
		UpdateHud();
	}
	private void UpdateHud()
	{
		if (_overlay != "" || Hero == null)
			return;
		_health.Text = "VITALITY     " + Hero.Health + " / " + Hero.Def.MaxHealth;
		_healthBar.Value = 100f * Hero.Health / Hero.Def.MaxHealth;
		_energy.Text = (Hero.Def.Id == "guardian" ? "RESOLVE" : "MANA") + "       " + (int)Hero.Mana + " / " + Hero.Def.MaxMana;
		_energyBar.Value = 100f * Hero.Mana / Hero.Def.MaxMana;
		_status.Text = "Dodge   " + (Hero.DodgeRemaining <= 0 ? "Ready" : Hero.DodgeRemaining.ToString("0.0") + "s") + "\nGold     " + Run.Gold + "\nMemories     " + Run.Bosses.Count + " / 4" + (Hero.Burn > 0 ? "\nBURNING" : "") + (Hero.Slow > 0 ? "\nSLOWED" : "") + (Hero.Poison > 0 ? "\nPOISONED" : "");
		string[] keys = { "Q", "F", "R" };
		for (int i = 0; i < 3; i++)
			_skillLabels[i].Text = keys[i] + "  " + Hero.Def.Skills[i].Name + "\n" + (Hero.Cooldowns[i] > 0 ? Hero.Cooldowns[i].ToString("0.0") + "s" : Hero.Def.Skills[i].Cost + " energy");
		for (int i = 0; i < 3; i++)
		{
			var it = GameData.Items.First(it => it.Id == Run.QuickSlots[i]);
			_itemLabels[i].Text = (i + 1) + "  " + it.Name + "   x" + Run.Inventory.GetValueOrDefault(it.Id);
		}
		var boss = Enemies.FirstOrDefault(e => IsInstanceValid(e) && e.IsBoss && !e.Dead);
		_bossText.Visible = _bossBar.Visible = boss != null;
		if (boss != null)
		{
			_bossText.Text = boss.Boss.Name + (boss.Reviving ? " / REVIVING..." : boss.PhaseTwo ? " / II - REBORN" : " / I");
			_bossBar.Value = 100f * boss.Health / boss.MaxHealth;
		}
		int count = Enemies.Count(e => IsInstanceValid(e) && !e.Dead);
		_objective.Text = count > 0 ? count + " enemies remain / Gates sealed" : Room.Kind == "boss" ? "E at the northern light / Continue the journey" : Room.Kind == "treasure" ? "E near each chest / Collect your keepsakes" : Room.Kind == "rest" ? "E at lantern or merchant / Shop and rest" : "E near a gate / Explore the next room";
	}
	private void Modal(string title, string id)
	{
		Active = false;
		_overlay = id;
		if (World != null)
			World.ProcessMode = ProcessModeEnum.Disabled;
		ResetUi();
		var shade = new ColorRect { Color = new Color(0, 0, 0, .68f), Size = new Vector2(1280, 720), MouseFilter = Control.MouseFilterEnum.Stop };
		_ui.AddChild(shade);
		_ui.MoveChild(shade, 0);
		Panel(105, 57, 1070, 600, new Color("0e1923"));
		Text(title, 143, 83, 850, 34, Gold, true);
		Button("Close", 1016, 80, 121, Resume, 36);
	}
	private void Resume()
	{
		if (Screen != "game")
		{
			ShowTitle();
			return;
		}
		if (Hero.Dead)
		{
			ShowDeath();
			return;
		}
		Active = true;
		_overlay = "";
		World.ProcessMode = ProcessModeEnum.Inherit;
		BuildHud();
	}
	private void ShowPause()
	{
		if (Hero == null || Hero.Dead)
			return;
		Modal("A moment of quiet", "pause");
		Text(GameData.Levels[Run.Level].Memory, 148, 149, 970, 24, new Color("c9d1c3"), true);
		Text("Chapter " + (Run.Level + 1) + " / " + Room.Name + "\n" + Run.Cleared.Count + " / 16 rooms cleared / " + Run.Bosses.Count + " / 4 memories released", 148, 229, 910, 18, Muted);
		Button("Return to the journey", 148, 343, 430, Resume, 48, true);
		Button("Save journey", 607, 343, 430, () => { Save(); Toast("Journey saved."); }, 48);
		Button("Settings", 148, 409, 430, () => ShowSettings(true), 45);
		Button("How to play", 607, 409, 430, () => ShowHelp(true), 45);
		Button("Save and return to title", 148, 491, 889, () => { Save(); ShowTitle(); }, 46);
		Button("Change appearance", 148, 554, 430, ShowAppearance, 44);
	}
	private void ShowInventory()
	{
		if (Hero == null || Hero.Dead)
			return;
		Modal("The satchel", "inventory");
		Text("Gold: " + Run.Gold + " / Select a quick slot, then assign a supply. Equipment is class-exclusive.", 145, 134, 960, 14, Muted);
		for (int i = 0; i < 3; i++)
		{
			int slot = i;
			Button("Slot " + (i + 1) + ": " + GameData.Items.First(it => it.Id == Run.QuickSlots[i]).Name, 146 + i * 298, 169, 281, () => { _assignSlot = slot; ShowInventory(); }, 34, _assignSlot == i);
		}
		for (int i = 0; i < GameData.Items.Length; i++)
		{
			var it = GameData.Items[i];
			float x = i < 6 ? 146 : 644;
			float y = 224 + i % 6 * 65;
			int amount = Run.Inventory.GetValueOrDefault(it.Id);
			Text(it.Name + " x" + amount, x, y, 290, 16, amount > 0 ? new Color("d7dcca") : Muted);
			Text(it.Description, x, y + 24, 287, 11, Muted);
			if (it.Kind == "consumable")
			{
				Button("Use", x + 299, y, 64, () => { UseItem(it.Id); ShowInventory(); }, 29).Disabled = amount == 0;
				Button("Set " + (_assignSlot + 1), x + 370, y, 77, () => { Run.QuickSlots[_assignSlot] = it.Id; Save(); ShowInventory(); }, 29);
			}
			else
			{
				bool equipped = Run.Weapon == it.Id || Run.Armor == it.Id;
				Button(equipped ? "Equipped" : "Equip", x + 317, y, 114, () => { if (it.Kind == "weapon") Run.Weapon = it.Id; else Run.Armor = it.Id; Save(); ShowInventory(); }, 30, equipped).Disabled = amount == 0 || it.ClassId != Run.ClassId;
			}
		}
	}
	private void ShowMap()
	{
		if (Hero == null)
			return;
		Modal("Atlas of memories", "map");
		Text(GameData.Levels[Run.Level].Name, 148, 137, 940, 21, new Color("cad4c3"), true);
		Text("You are in " + Room.Name + ". Explore through gates; the atlas does not teleport.", 148, 175, 930, 14, Muted);
		Vector2[] pos = { new(180, 275), new(390, 275), new(390, 430), new(610, 275), new(610, 430), new(830, 275), new(830, 430), new(1020, 430) };
		for (int i = 0; i < 8; i++)
			foreach (int j in GameData.Levels[Run.Level].Rooms[i].Links)
				if (j > i)
				{
					var line = new Line2D { Width = 2, DefaultColor = new Color("4b615c"), Points = new[] { pos[i], pos[j] } };
					_ui.AddChild(line);
				}
		for (int i = 0; i < 8; i++)
		{
			var r = GameData.Levels[Run.Level].Rooms[i];
			bool clear = Run.Cleared.Contains(r.Id);
			Panel(pos[i].X - 38, pos[i].Y - 30, 90, 70, Run.Room == i ? new Color("425749") : clear ? new Color("203b35") : new Color("1e2934"));
			Text((i + 1).ToString("00") + "\n" + r.Kind.ToUpper(), pos[i].X - 31, pos[i].Y - 22, 88, 11, Run.Room == i ? Gold : Muted);
			Text(r.Name, pos[i].X - 60, pos[i].Y + 50, 160, 12, Muted);
		}
		Text("Gold: your location / Green: cleared / Dark: unexplored\nSide paths hold rest and treasure. The final chamber guards the memory.", 165, 562, 900, 15, Muted);
	}
	private void ShowShop()
	{
		Modal("The resting lantern", "shop");
		Text("A little preparation goes a long way. Your gold: " + Run.Gold, 147, 139, 635, 17, Muted);
		Button("Dyes + designs", 815, 140, 306, ShowCosmeticShop, 35, true);
		for (int i = 0; i < 6; i++)
		{
			var it = GameData.Items[i];
			float y = 203 + i * 62;
			Text(it.Name, 148, y, 310, 20, Gold, true);
			Text(it.Description, 440, y + 5, 360, 14, Muted);
			Button(it.Price + " gold / Buy", 876, y, 245, () => { if (Run.Gold >= it.Price) { Run.Gold -= it.Price; AddItem(it.Id, 1); Save(); ShowShop(); Sound("chest"); } }, 37).Disabled = Run.Gold < it.Price;
		}
	}
	private void ShowSettings(bool inGame)
	{
		if (inGame)
			Modal("Settings", "settings");
		else
		{
			Screen = "settings";
			ResetUi();
			Panel(105, 57, 1070, 600);
			Text("Settings", 143, 83, 950, 34, Gold, true);
			Button("Back", 1016, 80, 121, ShowTitle, 36);
		}
		Text("MASTER VOLUME", 155, 196, 400, 14, Muted);
		var slider = new HSlider { Position = new Vector2(155, 239), Size = new Vector2(720, 32), MinValue = 0, MaxValue = 1, Step = .05, Value = Settings.Volume };
		slider.ValueChanged += v => { Settings.Volume = (float)v; ApplySettings(); StoreSettings(); };
		_ui.AddChild(slider);
		Button("Fullscreen: " + (Settings.Fullscreen ? "On" : "Off"), 155, 326, 400, () => { Settings.Fullscreen = !Settings.Fullscreen; ApplySettings(); StoreSettings(); ShowSettings(inGame); }, 46);
		Button("Screen shake: " + (Settings.Shake ? "On" : "Off"), 592, 326, 400, () => { Settings.Shake = !Settings.Shake; StoreSettings(); ShowSettings(inGame); }, 46);
		Text("Settings are saved locally. Keyboard and mouse are the core control scheme.", 155, 437, 820, 16, Muted);
	}
	private void ShowHelp(bool inGame)
	{
		if (inGame)
			Modal("How to carry the light", "help");
		else
		{
			Screen = "help";
			ResetUi();
			Panel(105, 57, 1070, 600);
			Text("How to carry the light", 143, 83, 950, 34, Gold, true);
			Button("Back", 1016, 80, 121, ShowTitle, 36);
		}
		Text("WASD / arrows    Move\nMouse + left click    Aim and attack\nSpace    Dodge (2-second cooldown)\nQ / F / R    Three class skills\n1 / 2 / 3    Quick items\nE    Gates, chests, resting lantern\nI    Inventory and equipment\nM    Room map\nEsc    Pause", 151, 165, 470, 19);
		Text("Read the warning", 681, 165, 410, 25, Gold, true);
		Text("Enemies show amber warnings before striking. Step away from circles or dodge through attacks. Dodging briefly prevents damage.\n\nDefeat the boss to release each chapter's memory. Defeat the first health bar: the boss revives once with a fresh second bar.\n\nSide paths lead to rest and treasure. Saves occur on transitions, clears and periodically. Retry the current room after death for up to 15 gold.\n\nBurn and poison deal damage over time; slow reduces movement. Clearwater removes all three.", 681, 212, 410, 16, Muted);
	}
	private void ShowDeath()
	{
		Modal("The ember has not gone out", "death");
		Text("Even now, something in you chooses to continue.", 150, 167, 970, 26, new Color("c7cfbf"), true);
		Text("Return to the current room with full health and energy.\nThe lantern takes up to 15 gold; your collected keepsakes remain.", 150, 264, 910, 19, Muted);
		Button("Rise again", 150, 427, 430, Retry, 52, true);
		Button("Return to title", 611, 427, 430, ShowTitle, 52);
	}
	private void ShowEnding()
	{
		Active = false;
		Screen = "ending";
		_toastTime = 0;
		if (World != null)
		{
			World.QueueFree();
			World = null;
			Hero = null;
		}
		ResetUi();
		Text("FIRST TWO LEVELS COMPLETE", 77, 103, 820, 14, Gold);
		Text("Beyond\nthe Goodbye", 72, 152, 790, 64, new Color("efe4cb"), true);
		Text("You have reached the end of this two-level edition.\nThe journey continues beyond these rooms.", 79, 327, 770, 22, new Color("d1d8c7"), true);
		Text("Two-level edition complete / " + Run.ClassId + "\n" + Run.Cleared.Count + " / 16 rooms cleared / " + Run.Chests.Count + " / 4 keepsakes found", 80, 451, 740, 17, Muted);
		Button("Return to title", 80, 553, 310, ShowTitle, 48, true);
		Button("Credits", 410, 553, 200, ShowCredits, 48);
		Sound("clear");
	}
	private void ShowCredits()
	{
		Screen = "credits";
		Active = false;
		ResetUi();
		Panel(105, 57, 1070, 600);
		Text("Beyond the Goodbye", 148, 95, 1000, 39, Gold, true);
		Text("GROUP 15\nAntonio Johnson / Javin Vezinat / Isaac Moreno\nJacob Wear / Justin Mexil", 150, 178, 900, 22);
		Button("Back to title", 150, 562, 310, ShowTitle, 45, true);
	}
}
