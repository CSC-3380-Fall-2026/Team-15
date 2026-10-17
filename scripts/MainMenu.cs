using Godot;

public partial class MainMenu : Control
{
	[Export(PropertyHint.File, "*.tscn")]
	public string GameScenePath { get; set; } = "res://scenes/main.tscn";

	public override void _Ready()
	{
		var begin = GetNode<Button>("%BeginButton");
		begin.Pressed += () => GetTree().ChangeSceneToFile(GameScenePath);
		GetNode<Button>("%QuitButton").Pressed += () => GetTree().Quit();
		begin.GrabFocus();
	}
}
