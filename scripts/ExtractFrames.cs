using Godot;
using System;

public static class ExtractFrames
{
    public static void Export()
    {
        var sheet = GD.Load<Texture2D>("res://assets/guardian_reference_sheet.svg");
        using Image image = sheet.GetImage();
        for (int index = 0; index < 4; index++)
        {
            using Image frame = image.GetRegion(new Rect2I((index + 4) * 64, 0, 64, 64));
            Error result = frame.SavePng($"res://assets/walk_reference_{index + 1}.png");
            if (result != Error.Ok) throw new InvalidOperationException($"Frame export failed: {result}");
        }
        GD.Print("Exported four reference PNG walk frames.");
    }
}

