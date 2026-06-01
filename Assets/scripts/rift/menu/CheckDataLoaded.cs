public class CheckDataLoaded : SequentialBehavior
{
    public override void activate()
    {
        base.activate();
        if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings == 1) // si se está buildeando solo una escena
        {
            SaveManager.Instance.createEmptySaveData();
        }
        else
        {
            SaveManager.Instance.load();
        }
    }

    public override void update()
    {
        base.update();

        if (SaveManager.Instance.isLoadDone() && SaveManager.Instance.isSavedDone())
        {
            toNextState();
        }
    }
}
