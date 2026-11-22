using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace BeyondTheGoodbye;
public partial class Main
{
	private int _checks;
	private void Check(bool condition, string message)
	{
		if (!condition)
			throw new Exception("FAILED: " + message);
		_checks++;
		GD.Print("PASS " + _checks + ": " + message);
	}
	private async Task Frames(int count = 2)
	{
		for (int i = 0; i < count; i++)
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}
	private async Task Wait(float seconds) => await ToSignal(GetTree().CreateTimer(seconds), Timer.SignalName.Timeout);
	public async void RunTests()
	{
		SaveStore.Path = "user://automated-test-save.json";
		try
		{
			Check(_titleArt != null, "Title artwork loads from imported resources");
			Check(GameData.Classes.Length == 3 && GameData.Classes.Sum(c => c.Skills.Length) == 9, "Three classes and nine skills");
			Check(GameData.Items.Length == 12 && GameData.Items.Count(i => i.Kind == "consumable") == 6, "Twelve items; six consumables");
			Check(GameData.Foes.Enemies.Length == 8 && GameData.Foes.Bosses.Length == 4, "Eight regular enemy types and four bosses");
			Check(GameData.Levels.Length == 4 && GameData.Levels.All(l => l.Rooms.Length == 8), "Four levels and 32 rooms");
			Check(GameData.Levels.Sum(l => l.Rooms.Sum(r => r.Enemies.Length)) == 60, "Sixty regular enemy placements");
			Check(GameData.Levels.Sum(l => l.Rooms.Sum(r => r.Chests)) == 8, "Eight chests");
			Check(GameData.Art.Sum(a => a.Animations.Length) == 148 && GameData.Art.Sum(a => a.AuthoredFrames) == 424 && GameData.Art.Sum(a => a.Slots) == 496, "148 clips / 424 source frames / 496 slots / 72 mirrors");
			foreach (var art in GameData.Art)
				Check(GameData.GetFrames(art.Id).GetAnimationNames().Length == art.Animations.Length, "Imported animations: " + art.Id);
			foreach (var level in GameData.Levels)
			{
				var visited = new HashSet<int>();
				var queue = new Queue<int>();
				queue.Enqueue(0);
				while (queue.Count > 0)
				{
					int r = queue.Dequeue();
					if (!visited.Add(r))
						continue;
					foreach (int next in level.Rooms[r].Links)
						queue.Enqueue(next);
				}
				Check(visited.Count == 8, "All rooms connected: " + level.Name);
				Check(level.Rooms.Count(r => r.Kind == "combat") == 5 && level.Rooms.Count(r => r.Kind == "boss") == 1 && level.Rooms.Count(r => r.Kind == "rest") == 1 && level.Rooms.Count(r => r.Kind == "treasure") == 1, "Room allocation: " + level.Name);
			}
			foreach (var c in GameData.Classes)
			{
				NewRun(c.Id, 0);
				await Frames();
				foreach (var e in Enemies)
					e.ProcessMode = ProcessModeEnum.Disabled;
				for (int i = 0; i < 3; i++)
				{
					Hero.Mana = c.MaxMana;
					Hero.Health = c.MaxHealth - 60;
					Hero.Cooldowns[i] = 0;
					Check(Hero.UseSkill(i), c.Name + " skill activates: " + c.Skills[i].Name);
					Check(Mathf.IsEqualApprox(Hero.Mana, c.MaxMana - c.Skills[i].Cost) && Hero.Cooldowns[i] > 0, "Mana cost and cooldown recorded");
					Check(!Hero.UseSkill(i), "Cooldown prevents duplicate cast");
				}
				Hero.Invulnerable = 0;
				Hero.DodgeTime = 0;
				Hero.Health = c.MaxHealth;
				Hero.Hurt(20);
				Check(Hero.Health == c.MaxHealth - (c.Id == "guardian" ? 16 : 20), "Class damage mitigation: " + c.Name);
				Hero.Health = c.MaxHealth - 50;
				Hero.Mana = 0;
				Hero.Burn = Hero.Poison = Hero.Slow = 3;
				foreach (var it in GameData.Items.Where(i => i.Kind == "consumable"))
				{
					AddItem(it.Id, 1);
					int before = Run.Inventory[it.Id];
					Check(UseItem(it.Id), "Use " + it.Id + " as " + c.Id);
					Check(Run.Inventory[it.Id] == before - 1, "Consumes exactly one item");
				}
				Check(Hero.Burn == 0 && Hero.Poison == 0 && Hero.Slow == 0, "Antidote clears three statuses");
				Hero.DodgeRemaining = 0;
				Hero.DodgeTime = 0;
				Check(Hero.Dodge(Vector2.Left) && !Hero.Dodge(Vector2.Left), "Dodge cooldown blocks spam");
				Check(Mathf.IsEqualApprox(Hero.DodgeRemaining, 2), "Dodge cooldown exactly two seconds");
				int hp = Hero.Health;
				Hero.Hurt(30);
				Check(Hero.Health == hp, "Dodge or ward prevents damage");
			}
			NewRun("guardian", 1);
			await Frames();
			foreach (var e in Enemies)
				e.ProcessMode = ProcessModeEnum.Disabled;
			Hero.Position = new Vector2(40, 110);
			Input.ActionPress("move_left");
			await Wait(.3f);
			Input.ActionRelease("move_left");
			Check(Hero.Position.X >= 25, "West wall collision");
			Hero.Position = new Vector2(200, 256);
			Hero.Aim = Vector2.Right;
			var target = Enemies[0];
			target.Position = new Vector2(250, 256);
			int oldHp = target.Health;
			Melee(Hero.Position, Vector2.Left, 80, 20, .05f);
			Check(target.Health == oldHp, "Melee misses behind aim");
			Melee(Hero.Position, Vector2.Right, 80, 20, .05f);
			Check(target.Health == oldHp - 20, "Melee damages within arc");
			target.Hit(1, 1);
			target.Hit(1, 2);
			target.Hit(1, 3);
			Check(target.Burn > 0 && target.Slow > 0 && target.Poison > 0, "Enemies receive all three statuses");
			ShowPause();
			Check(!Active && World.ProcessMode == ProcessModeEnum.Disabled, "Pause freezes world");
			Resume();
			Check(Active, "Resume re-enables play");
			ShowInventory();
			Check(_overlay == "inventory" && !Active, "Inventory pauses play");
			Resume();
			ShowMap();
			Check(_overlay == "map", "Map opens");
			Resume();
			NewRun("ranger", 0);
			await Frames();
			foreach (var e in Enemies)
				e.ProcessMode = ProcessModeEnum.Disabled;
			Hero.SetPhysicsProcess(false);
			Hero.Position = new Vector2(100, 100);
			Hero.Aim = Vector2.Right;
			var rangedTarget = Enemies[0];
			rangedTarget.Position = new Vector2(230, 100);
			int rangedHp = rangedTarget.Health;
			Hero.BasicAttack();
			await Wait(.5f);
			Check(rangedTarget.Health < rangedHp, "Ranger projectile hits through physics processing");
			Hero.Invulnerable = 0;
			Hero.DodgeTime = 0;
			int rangedHeroHp = Hero.Health;
			Shoot(new Vector2(200, 100), Vector2.Left, 200, 10, true, 0, Gold);
			await Wait(.65f);
			Check(Hero.Health < rangedHeroHp, "Hostile projectile damages player");
			Hero.Invulnerable = 4;
			rangedHeroHp = Hero.Health;
			Shoot(new Vector2(200, 100), Vector2.Left, 200, 10, true, 0, Gold);
			await Wait(.65f);
			Check(Hero.Health == rangedHeroHp, "Ward blocks projectile damage");
			rangedTarget.Position = new Vector2(450, 160);
			rangedHp = rangedTarget.Health;
			Shoot(new Vector2(150, 160), Vector2.Right, 400, 20, false, 0, Gold);
			await Wait(1);
			Check(rangedTarget.Health == rangedHp, "Projectiles cannot pass through stone pillars");
			Hero.SetPhysicsProcess(true);
			Hero.Invulnerable = 0;
			Hero.DodgeTime = 0;
			await Wait(.6f);
			var telegraph = new BossHazard { Game = this, Shape = "circle", From = Hero.Position, Delay = .3f, Duration = .2f, Radius = 50, Damage = 12 };
			World.AddChild(telegraph);
			int beforeTelegraph = Hero.Health;
			await Wait(.1f);
			Check(Hero.Health == beforeTelegraph, "Boss telegraph does not damage before warning expires");
			ShowPause();
			float pausedAge = telegraph.Age;
			await Wait(.4f);
			Check(telegraph.Age == pausedAge, "Pause freezes boss warning timer");
			Resume();
			await Wait(.5f);
			Check(Hero.Health < beforeTelegraph, "Active boss hazard applies damage after warning");
			await Wait(.6f);
			int beforePreview = Hero.Health;
			World.AddChild(new BossHazard { Game = this, Shape = "line", From = Hero.Position - Vector2.Right * 60, To = Hero.Position + Vector2.Right * 60, Delay = .1f, Duration = .2f, Damage = 0 });
			await Wait(.4f);
			Check(Hero.Health == beforePreview, "Charge preview never causes phantom minimum damage");
			var shape = new BossHazard { Shape = "line", From = new(100, 100), To = new(500, 100), Width = 24 };
			Check(shape.ContainsPoint(new(300, 100), 0) && !shape.ContainsPoint(new(300, 150), 0), "Line hit geometry matches warning corridor");
			shape.Shape = "wave";
			shape.From = new(24, 50);
			shape.To = new(616, 450);
			shape.GapX = 320;
			shape.GapWidth = 150;
			Check(!shape.ContainsPoint(new(320, 250), .5f) && shape.ContainsPoint(new(100, 250), .5f), "Wave leaves its displayed gap safe");
			shape.Shape = "arc";
			shape.From = Vector2.Zero;
			shape.Angle = 0;
			shape.Radius = 100;
			Check(shape.ContainsPoint(new(50, 0), 0) && !shape.ContainsPoint(new(-50, 0), 0), "Sweep only hits inside its directional sector");
			shape.Shape = "ring";
			shape.Radius = 225;
			Check(shape.ContainsPoint(new(125, 0), .5f) && !shape.ContainsPoint(new(50, 0), .5f), "Expanding ring has a safe interior");
			shape.Free();
			ShowAppearance();
			int keptGold = Run.Gold;
			for (int palette = 0; palette < 4; palette++)
			{
				SetAppearance(palette);
				Check(Run.Palette == palette && Run.BodyColor == "" && Hero.Sprite.Material is ShaderMaterial, "Appearance changes in current run: " + palette);
			}
			Check(!Active && Run.Gold == keptGold && _overlay == "appearance", "Appearance screen pauses and preserves progress");
			SyncRun();
			Check(SaveStore.Write(Run, out string appearanceError) && SaveStore.Load(out appearanceError).Palette == 3, "Changed appearance survives save/load");
			Check(SetDye(false, "ee7766") && SetDye(true, "66ccff"), "Independent outfit and weapon colors accept RGB");
			Check(Run.BodyColor == "ee7766" && Run.WeaponColor == "66ccff", "Weapon dye does not overwrite outfit dye");
			Check(!SetDye(true, "invalid"), "Invalid color rejected");
			Run.Gold = 0;
			Check(!BuyCosmetic("emberforged") && !Run.OwnedCosmetics.Contains("emberforged"), "Shop rejects unaffordable designs");
			Run.Gold = 100;
			Check(BuyCosmetic("emberforged") && Run.Gold == 55, "Shop deducts exact price once");
			Check(!BuyCosmetic("emberforged") && Run.Gold == 55, "Owned design cannot be purchased twice");
			WearCosmetic("emberforged");
			Check(Run.OutfitPattern == 1 && Run.WeaponFinish == 1, "Owned pack equips both design channels");
			SetDye(false, "ee7766"); SetDye(true, "66ccff");
			SyncRun();
			Check(SaveStore.Write(Run, out string dyeError), "Cosmetic save writes");
			var dyed = SaveStore.Load(out dyeError);
			Check(dyed.BodyColor == "ee7766" && dyed.WeaponColor == "66ccff" && dyed.OwnedCosmetics.Contains("emberforged"), "Colors and purchases survive save round trip");
			var legacy = System.Text.Json.JsonSerializer.Deserialize<SaveData>("{\"ClassId\":\"guardian\"}");
			Check(SaveStore.Validate(legacy), "Old saves without cosmetic fields retain valid defaults");
			Resume();
			Engine.TimeScale = 6;
			for (int level = 0; level < 4; level++)
			{
				Run.Health = Hero.Def.MaxHealth;
				Run.Mana = Hero.Def.MaxMana;
				LoadRoom(level, 7);
				Hero.Invulnerable = 999;
				var patternBoss = Enemies[0];
				Check(patternBoss.Boss.Name == new[] { "The Silent Host", "The Ember Knight", "The Mourning Tide", "The Weaver of If" }[level], "Selected roster loaded");
				await Wait(22);
				Check(patternBoss.BossAttackCounts.All(count => count > 0), "All three phase-I attacks execute: " + patternBoss.Boss.Name);
				var phaseOneCounts = (int[])patternBoss.BossAttackCounts.Clone();
				patternBoss.Hit(patternBoss.MaxHealth / 2);
				await Frames(2);
				Check(!patternBoss.PhaseTwo && !patternBoss.Reviving, "Half health no longer triggers revival");
				int goldBeforeRevival = Run.Gold;
				int memoriesBeforeRevival = Run.Bosses.Count;
				patternBoss.Hit(999999);
				await Frames(2);
				Check(patternBoss.Reviving && !patternBoss.Dead && patternBoss.Health == 0, "First defeat starts revival instead of death");
				Check(Run.Gold == goldBeforeRevival && Run.Bosses.Count == memoriesBeforeRevival && !Run.Cleared.Contains(Room.Id), "First form gives no rewards or room clear");
				Hero.Position = new Vector2(320, 70);
				Interact();
				Check(Run.Level == level && Run.Room == 7, "Exit stays locked during revival");
				patternBoss.Hit(99999);
				Check(patternBoss.Health == 0 && patternBoss.Reviving, "Overkill cannot skip revival");
				await Wait(2.6f);
				Check(patternBoss.PhaseTwo && patternBoss.Health == patternBoss.MaxHealth && patternBoss.PhaseTransitions == 1, "Revival restores a fresh full second health bar");
				Check(patternBoss.Sprite.Scale.X > 1.6f, "Phase-II silhouette grows");
				await Wait(32);
				Check(Enumerable.Range(0, 3).All(i => patternBoss.BossAttackCounts[i] > phaseOneCounts[i]), "All three phase-II attacks execute");
				Check(patternBoss.PhaseTransitions == 1, "Transition does not repeat");
				patternBoss.Hit(99999);
				await Frames(3);
				Check(!World.GetChildren().Any(n => n is BossHazard), "Boss death clears remaining hazards");
			}
			Engine.TimeScale = 6;
			// Walk the real room graph and verify every encounter, reward, boss phase, and chapter transition.
			foreach (var c in GameData.Classes)
			{
				NewRun(c.Id, 2);
				await Frames();
				for (int level = 0; level < 4; level++)
				{
					int[] route = { 0, 1, 2, 1, 3, 4, 3, 5, 6, 7 };
					for (int step = 0; step < route.Length; step++)
					{
						Check(Run.Room == route[step] && Run.Level == level, "Route reached " + c.Id + " L" + level + " R" + route[step]);
						Hero.Invulnerable = 999;
						if (Room.Kind == "boss")
						{
							var boss = Enemies.First(e => e.IsBoss);
							Check(boss.AttackWait == 2, "Boss phase one wait");
							boss.Hit(boss.MaxHealth);
							await Wait(2.6f);
							Check(boss.PhaseTwo && Mathf.IsEqualApprox(boss.AttackWait, 1.4f), "Boss revives before phase two, 1.4s wait");
						}
						foreach (var enemy in Enemies.ToArray())
							if (IsInstanceValid(enemy) && !enemy.Dead)
								enemy.Hit(99999);
						if (Room.Kind == "treasure")
						{
							OpenChest(0);
							OpenChest(1);
							int gold = Run.Gold;
							int chestCount = Run.Chests.Count;
							OpenChest(1);
							Check(Run.Gold == gold && Run.Chests.Count == chestCount, "Chest cannot be looted twice");
						}
						if (Room.Kind == "rest")
							Check(Hero.Health == Hero.Def.MaxHealth && Hero.Mana == Hero.Def.MaxMana, "Rest restores vitality and energy");
						if (step < route.Length - 1)
						{
							var door = _doors.First(d => d.Destination == route[step + 1]);
							Hero.Position = door.Position;
							Interact();
							await Frames();
						}
					}
					Hero.Position = new Vector2(320, 70);
					Interact();
					await Frames();
				}
				Check(Run.Completed && Screen == "ending", "Complete ending reached as " + c.Id);
				Check(Run.Cleared.Count == 32 && Run.Bosses.Count == 4 && Run.Chests.Count == 8, "All campaign counts complete as " + c.Id);
				Check(SaveStore.Write(Run, out string err), "Save writes atomically: " + err);
				var loaded = SaveStore.Load(out err);
				Check(loaded != null && loaded.ClassId == c.Id && loaded.Completed && loaded.Cleared.Count == 32 && loaded.Chests.Count == 8, "Full save round trip as " + c.Id);
			}
			Engine.TimeScale = 1;
			using (var file = FileAccess.Open(SaveStore.Path, FileAccess.ModeFlags.Write))
				file.StoreString("{bad json");
			Check(SaveStore.Load(out string invalid) == null && invalid.Contains("preserved"), "Corrupt save rejected without overwrite");
			Check(FileAccess.GetFileAsString(SaveStore.Path) == "{bad json", "Corrupt bytes preserved");
			NewRun("guardian", 0);
			await Frames();
			Hero.Hurt(10000);
			await Frames();
			Check(_overlay == "death" && !Active, "Death overlay and simulation stop");
			Retry();
			Check(Hero.Health == Hero.Def.MaxHealth && Active, "Retry restores health and encounter");
			GD.Print("ALL " + _checks + " CHECKS PASSED");
			GetTree().Quit();
		}
		catch (Exception ex) { GD.PushError(ex.ToString()); GetTree().Quit(1); }
	}
	public async void CaptureScreens()
	{
		_testMode = true;
		string folder = ProjectSettings.GlobalizePath("res://../Screenshots");
		System.IO.Directory.CreateDirectory(folder);
		async Task Shot(string name)
		{
			await Frames(3);
			await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
			GetViewport().GetTexture().GetImage().SavePng(folder + "/" + name + ".png");
		}
		ShowTitle();
		await Shot("01-title");
		ShowClassSelection();
		await Shot("02-classes");
		NewRun("guardian", 0);
		World.ProcessMode = ProcessModeEnum.Disabled;
		await Shot("03-guardian");
		ShowInventory();
		await Shot("04-inventory");
		Resume();
		ShowMap();
		await Shot("05-map");
		NewRun("ranger", 1);
		LoadRoom(1, 7);
		World.ProcessMode = ProcessModeEnum.Disabled;
		await Shot("06-huntsman");
		NewRun("arcanist", 3);
		LoadRoom(2, 7);
		World.ProcessMode = ProcessModeEnum.Disabled;
		await Shot("07-choir");
		LoadRoom(3, 7);
		World.ProcessMode = ProcessModeEnum.Disabled;
		await Shot("08-finale");
		Run.Gold = 250;
		BuyCosmetic("emberforged"); WearCosmetic("emberforged");
		SetDye(false, "ac7bdd"); SetDye(true, "72e4e9");
		ShowAppearance();
		await Shot("10-appearance");
		ShowCosmeticShop();
		await Shot("12-cosmetic-shop");
		Resume();
		LoadRoom(0, 2);
		World.ProcessMode = ProcessModeEnum.Disabled;
		await Shot("13-merchant-stall");
		ShowShop();
		await Shot("14-supply-shop");
		Resume();
		for (int level = 0; level < 4; level++)
		{
			NewRun("guardian", 0);
			LoadRoom(level, 7);
			Hero.Invulnerable = 999;
			var boss = Enemies[0];
			World.ProcessMode = ProcessModeEnum.Disabled;
			await Shot("boss-" + (level + 1) + "-phase-1");
			World.ProcessMode = ProcessModeEnum.Inherit;
			boss.Hit(boss.MaxHealth);
			World.ProcessMode = ProcessModeEnum.Disabled;
			await Shot("boss-" + (level + 1) + "-reviving");
			World.ProcessMode = ProcessModeEnum.Inherit;
			await Wait(2.6f);
			World.ProcessMode = ProcessModeEnum.Disabled;
			await Shot("boss-" + (level + 1) + "-phase-2");
		}
		StartJourney();
		await Shot("11-prologue");
		Run.Completed = true;
		Run.Cleared = Enumerable.Range(0, 32).ToList();
		Run.Bosses = new() { 0, 1, 2, 3 };
		Run.Chests = Enumerable.Range(0, 8).Select(i => i.ToString()).ToList();
		ShowEnding();
		await Shot("09-ending");
		GetTree().Quit();
	}
}
