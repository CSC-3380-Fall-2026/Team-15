using Godot;
using System.Linq;
namespace BeyondTheGoodbye;

public record CosmeticPack(string Id, string Name, string Body, string Weapon, int Pattern, int Finish, int Price, string Description);
public static class Cosmetics
{
	public static readonly CosmeticPack[] Packs = {
		new("emberforged", "Emberforged", "df8760", "ffb04d", 1, 1, 45, "Copper cloth, gold trim and a pulsing ember weapon."),
		new("moonglass", "Moon Glass", "8bb6e8", "a7f4ff", 2, 2, 65, "Ice-blue stitching and a silver-banded weapon."),
		new("verdant", "Verdant Oath", "83c493", "cce891", 1, 2, 85, "Forest colors, luminous edging and leaf-gold bands."),
		new("royaldusk", "Royal Dusk", "bd91dc", "f5c47d", 2, 1, 100, "Violet brocade and a warm runic weapon glow.")
	};
	public static ShaderMaterial Material(SaveData run, int? palette = null)
	{
		var material = new ShaderMaterial { Shader = GD.Load<Shader>("res://Art/character_dyes.gdshader") };
		Update(material, run, palette);
		return material;
	}
	public static void Update(ShaderMaterial material, SaveData run, int? palette = null)
	{
		material.SetShaderParameter("outfit_color", palette.HasValue ? GameData.Palettes[palette.Value] : run.BodyColor == "" ? GameData.Palettes[run.Palette] : new Color(run.BodyColor));
		material.SetShaderParameter("custom_outfit", !palette.HasValue && run.BodyColor != "");
		material.SetShaderParameter("weapon_color", new Color(run.WeaponColor));
		material.SetShaderParameter("outfit_pattern", run.OutfitPattern);
		material.SetShaderParameter("weapon_finish", run.WeaponFinish);
	}
	public static bool ValidHex(string color) => color != null && color.Length == 6 && color.All(c => "0123456789abcdefABCDEF".Contains(c));
}
