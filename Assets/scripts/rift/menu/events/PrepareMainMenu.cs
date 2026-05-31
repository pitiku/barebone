
public class PrepareMainMenu : TimedEvent
{
    public override void play()
    {
        base.play();

        MainMenuManager.Instance.prepare();
    }
}
