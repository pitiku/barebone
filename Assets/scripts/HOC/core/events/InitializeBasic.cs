public class InitializeBasic : TimedEvent
{
    public override void play()
    {
        base.play();

        Layers.initialize();
        GameUtils.initialize();

        RewiredManager.Instance.initialize();

        AudioManager.Instance.initialize();
        PlatformManager.Current.Initialize();
        LocalizationGlobalParameters.Instance.CheckInitialized();
    }
}
