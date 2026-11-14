using Godot;

namespace BeyondTheLastGoodbye.Data;

/// <summary>
/// Autoload that loads data/*.json once at startup.
/// Use anywhere: GameDataLoader.Db.GetEnemy("skeleton_grunt")
/// A missing or broken file prints a clear error and the game keeps running.
/// </summary>
public partial class GameDataLoader : Node
{
    private const string DataFolder = "res://data/";

    public static GameDatabase Db { get; private set; } = new();

    public override void _EnterTree()
    {
        Db = DataLoader.LoadAll(ReadFile);

        if (Db.HasErrors)
        {
            foreach (string error in Db.Errors)
                GD.PushError($"[GameData] {error}");
        }
        else
        {
            GD.Print($"[GameData] Loaded {Db.Classes.Count} classes, {Db.Enemies.Count} enemies, " +
                     $"{Db.Items.Count} items, {Db.Levels.Count} levels.");
        }
    }

    // Uses Godot's FileAccess so it also works in exported builds (files inside the .pck).
    private static string? ReadFile(string fileName)
    {
        string path = DataFolder + fileName;
        if (!FileAccess.FileExists(path)) return null;
        return FileAccess.GetFileAsString(path);
    }
}
