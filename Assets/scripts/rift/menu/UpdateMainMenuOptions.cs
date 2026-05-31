// TO DEL
public class UpdateMainMenuOptions : Behavior
{
    public override void update()
    {
        base.update();
        MainMenuManager.Instance.updateMenu();
    }
}
