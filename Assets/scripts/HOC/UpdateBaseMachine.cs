
public class UpdateBaseMachine : Behavior
{
    public override void activate()
    {
        base.activate();

        GameplayManager.Instance.initialize();

        Layers.initialize();
        GameUtils.initialize();

        RewiredManager.Instance.initialize();

        AudioManager.Instance.initialize();
        PlatformManager.Current.Initialize();
        LocalizationGlobalParameters.Instance.CheckInitialized();

        FadeManager.Instance.activate();
        LoadingManager.Instance.activate();
        AchievementsManager.Instance.activate();
        GameStateManager.Instance.activate();

        FadeManager.Instance.FadeToBlack(0);
    }

    public override void update()
    {
        base.update();

        RewiredManager.Instance.UpdateInputs();
        PlatformManager.Current.update();

        GameStateManager.Instance.update();
        FadeManager.Instance.update();
        LoadingManager.Instance.update();
        AchievementsManager.Instance.update();
    }
}
