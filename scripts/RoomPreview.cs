using Godot;
using System;

public partial class RoomPreview : Node2D
{
    public override void _Ready()
    {
        GetNode<PlayerPreview>("Room/PlayerPreview").Position = GetNode<Marker2D>("Room/PlayerSpawn").Position;
        for (int index = 1; index <= 3; index++)
        {
            var marker = GetNode<Marker2D>($"Room/EnemySpawn{index}");
            marker.AddChild(new Polygon2D
            {
                Polygon = new[] { new Vector2(-10, -10), new Vector2(10, -10), new Vector2(10, 10), new Vector2(-10, 10) },
                Color = new Color(0.9f, 0.5f, 0.65f)
            });
        }
        string[] args = OS.GetCmdlineUserArgs();
        if (Array.IndexOf(args, "--extract-frames") >= 0) { ExtractFrames.Export(); GetTree().Quit(); }
        else if (Array.IndexOf(args, "--verify") >= 0) Verify();
        else if (Array.IndexOf(args, "--capture") >= 0) Capture();
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int index = 1; index <= 3; index++)
        {
            var marker = GetNode<Marker2D>($"Room/EnemySpawn{index}");
            DrawString(ThemeDB.FallbackFont, ToLocal(marker.GlobalPosition) + new Vector2(-27, -17),
                $"Enemy {index}", HorizontalAlignment.Left, -1, 13, new Color(1f, 0.85f, 0.9f));
        }
    }

    private async void Capture()
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng("res://assets/room_preview.png");
        GetTree().Quit();
    }

    private async void Verify()
    {
        try { await VerifyPreview.Run(this); GetTree().Quit(); }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
}

