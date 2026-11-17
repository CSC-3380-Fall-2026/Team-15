using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BeyondTheLastGoodbye.Data;

/// <summary>
/// Everything loaded from data/. Lists are never null: if a file fails to load,
/// its list is empty and the reason is in Errors.
/// </summary>
public class GameDatabase
{
    public List<ClassData> Classes { get; } = new();
    public List<EnemyData> Enemies { get; } = new();
    public List<ItemData> Items { get; } = new();
    public List<LevelData> Levels { get; } = new();
    public List<string> Errors { get; } = new();
    public bool HasErrors => Errors.Count > 0;

    public ClassData? GetClass(string id) => Classes.FirstOrDefault(c => c.Id == id);
    public EnemyData? GetEnemy(string id) => Enemies.FirstOrDefault(e => e.Id == id);
    public ItemData? GetItem(string id) => Items.FirstOrDefault(i => i.Id == id);
    public LevelData? GetLevel(string id) => Levels.FirstOrDefault(l => l.Id == id);
}

/// <summary>
/// Pure C# loader (no Godot types) so it can be unit tested with NUnit.
/// readFile takes a file name like "classes.json" and returns its text,
/// or null if the file doesn't exist.
/// </summary>
public static class DataLoader
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static GameDatabase LoadAll(Func<string, string?> readFile)
    {
        var db = new GameDatabase();
        Load<ClassesFile>(readFile, "classes.json", db, f => db.Classes.AddRange(f.Classes));
        Load<EnemiesFile>(readFile, "enemies.json", db, f => db.Enemies.AddRange(f.Enemies));
        Load<ItemsFile>(readFile, "items.json", db, f => db.Items.AddRange(f.Items));
        Load<LevelsFile>(readFile, "levels.json", db, f => db.Levels.AddRange(f.Levels));
        return db;
    }

    private static void Load<T>(Func<string, string?> readFile, string fileName,
                                GameDatabase db, Action<T> store) where T : class
    {
        string? text;
        try { text = readFile(fileName); }
        catch (Exception e) { db.Errors.Add($"{fileName}: could not read file ({e.Message})"); return; }

        if (text == null) { db.Errors.Add($"{fileName}: file is missing"); return; }

        try
        {
            T? parsed = JsonSerializer.Deserialize<T>(text, JsonOptions);
            if (parsed == null) { db.Errors.Add($"{fileName}: file is empty"); return; }
            store(parsed);
        }
        catch (JsonException e)
        {
            // e.LineNumber is 0-based
            db.Errors.Add($"{fileName}: invalid JSON at line {(e.LineNumber ?? 0) + 1} ({e.Message})");
        }
    }
}
