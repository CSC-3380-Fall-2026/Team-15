using Godot;
namespace BeyondTheGoodbye;
public partial class Main
{
public void Save()
	{
	if (Hero == null)
		{
		return;}
		Run.Health = Hero.Health;
		Run.Mana = Hero.Mana;
		string error;
		bool saved = SaveStore.Write(Run, out error);
		if (!saved)
		{
		GD.PushWarning(error);}
	}
}