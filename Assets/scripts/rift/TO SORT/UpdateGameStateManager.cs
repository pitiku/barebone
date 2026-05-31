public class UpdateGameStateManager : Behavior
{
    bool m_bDone;
    public override void activate()
    {
        base.activate();
        if (!m_bDone)
        {
            GameStateManager.Instance.linkSceneStates(); // do thsi once
            m_bDone = true;
        }
    }
    public override void update()
    {
        base.update();

        GameplayManager.Instance.update();
        GameStateManager.Instance.updateSceneStates();
        GameStateManager.Instance.checkPause();
    }
}