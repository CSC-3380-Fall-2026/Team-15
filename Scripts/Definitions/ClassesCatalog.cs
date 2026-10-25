namespace BeyondTheGoodbye;
// Editable game definitions. Costs and cooldowns are specified here.
public static class ClassesCatalog
{
	public static ClassDef[] Create() => new[]
	{
		new ClassDef
		{
			Id = "guardian",
			Name = "Guardian",
			Weapon = "Oathkeeper",
			MaxHealth = 160,
			MaxMana = 80,
			Speed = 175,
			Damage = 28,
			AttackCooldown = 0.42f,
			Passive = "Steadfast",
			PassiveDescription = "Take 20% less damage. Melee hits restore 2 resolve.",
			Skills = new[]
			{
				new SkillDef
				{
					Id = "bash",
					Name = "Shield Bash",
					Cost = 16,
					Cooldown = 5,
					Description = "Strike a wide arc. Stun nearby foes."
				},
				new SkillDef
				{
					Id = "bulwark",
					Name = "Bulwark",
					Cost = 24,
					Cooldown = 12,
					Description = "Become invulnerable for 3 seconds."
				},
				new SkillDef
				{
					Id = "rally",
					Name = "Rally",
					Cost = 28,
					Cooldown = 14,
					Description = "Restore 45 health. Empower attacks for 6 seconds."
				}
			}
		},
		new ClassDef
		{
			Id = "ranger",
			Name = "Ranger",
			Weapon = "Thread of Dawn",
			MaxHealth = 110,
			MaxMana = 100,
			Speed = 215,
			Damage = 22,
			AttackCooldown = 0.32f,
			Passive = "Stillness",
			PassiveDescription = "Standing still for 0.8s empowers arrows by 35%.",
			Skills = new[]
			{
				new SkillDef
				{
					Id = "pierce",
					Name = "Piercing Shot",
					Cost = 18,
					Cooldown = 4,
					Description = "A powerful arrow pierces all enemies."
				},
				new SkillDef
				{
					Id = "volley",
					Name = "Starfall Volley",
					Cost = 28,
					Cooldown = 8,
					Description = "Release a fan of seven arrows."
				},
				new SkillDef
				{
					Id = "retreat",
					Name = "Windstep",
					Cost = 22,
					Cooldown = 9,
					Description = "Dodge backward and slow nearby foes."
				}
			}
		},
		new ClassDef
		{
			Id = "arcanist",
			Name = "Arcanist",
			Weapon = "Lantern Staff",
			MaxHealth = 100,
			MaxMana = 140,
			Speed = 190,
			Damage = 24,
			AttackCooldown = 0.48f,
			Passive = "Reservoir",
			PassiveDescription = "Regenerate 6 mana per second instead of 3.",
			Skills = new[]
			{
				new SkillDef
				{
					Id = "ember",
					Name = "Ember Lance",
					Cost = 18,
					Cooldown = 4,
					Description = "A piercing flame inflicts burn."
				},
				new SkillDef
				{
					Id = "nova",
					Name = "Winter Nova",
					Cost = 30,
					Cooldown = 8,
					Description = "Damage and slow all nearby enemies."
				},
				new SkillDef
				{
					Id = "heal",
					Name = "Healing Light",
					Cost = 35,
					Cooldown = 12,
					Description = "Restore 55 health and remove harmful effects."
				}
			}
		}
	};
}
