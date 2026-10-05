using Godot;
using System;
using System.Threading.Tasks;

public static class VerifyPreview
{
    public static async Task Run(RoomPreview room)
    {
        var player = room.GetNode<PlayerPreview>("Room/PlayerPreview");
        var sprite = player.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        Check(sprite.SpriteFrames.GetFrameCount("walk") == 4, "Four walk frames");
        Check((int)sprite.SpriteFrames.GetAnimationLoopMode("walk") != 0, "Walk loops");
        Check(player.Position == room.GetNode<Marker2D>("Room/PlayerSpawn").Position, "Spawn alignment");
        player.SetPhysicsProcess(false);
        for (int index = 0; index < 120; index++)
        {
            await room.ToSignal(room.GetTree(), SceneTree.SignalName.PhysicsFrame);
            player.Velocity = new Vector2(-160, 0);
            player.MoveAndSlide();
        }
        Check(player.Position.X >= 25 && player.Position.X <= 28, "West wall blocks movement");
        GD.Print("ALL 4 C# PREVIEW CHECKS PASSED");
    }

    private static void Check(bool passed, string name)
    {
        if (!passed) throw new InvalidOperationException(name);
        GD.Print($"PASS: {name}");
    }
}
