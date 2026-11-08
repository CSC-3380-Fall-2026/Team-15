using Godot;
namespace BeyondTheGoodbye;
public partial class Main
{
private void ContinueRun()
{
string error;
	SaveData saved = SaveStore.Load(out error);
	if (saved == null)
		{
		Toast(error);
		return;}
		Run = saved;
		BuildWorldFromRun();
		if (Hero != null)
		{
		Hero.Health = Run.Health;
		Hero.Mana = Run.Mana;}
	Resume();}
}