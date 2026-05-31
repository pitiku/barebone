public class PlatformUpdate : Behavior
{
    public override void update()
    {
        base.update();

        PlatformManager.Current.update();
    }
}
