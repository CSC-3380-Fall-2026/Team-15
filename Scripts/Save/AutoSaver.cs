using Godot;
namespace BeyondTheGoodbye;
public partial class AutoSaver : Node
{
private int _lastLevel = -1;
private int _lastRoom = -1;
public override void _Process(double delta)
	{
	Main game = GetTree().CurrentScene as Main;
	if (game == null || game.Hero == null || game.Hero.Dead)
		{
		_lastLevel = -1;
		_lastRoom = -1;
		return;}
	if (game.Run.Level != _lastLevel || game.Run.Room != _lastRoom)
		{
		game.Save();
		_lastLevel = game.Run.Level;
		_lastRoom = game.Run.Room;}
	}
}