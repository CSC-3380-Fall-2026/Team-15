using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Export(PropertyHint.File, "*.tscn")]
	public string MainMenuScenePath { get; set; } = "res://scenes/MainMenu.tscn";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;
		GetNode<Button>("%ResumeButton").Pressed += Resume;
		GetNode<Button>("%QuitToMenuButton").Pressed += QuitToMenu;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("pause")) return;
		if (GetTree().Paused) Resume(); else Pause();
		GetViewport().SetInputAsHandled();
	}

	private void Pause()
	{
		GetTree().Paused = true;
		Visible = true;
		GetNode<Button>("%ResumeButton").GrabFocus();
	}

	private void Resume()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	private void QuitToMenu()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
}
