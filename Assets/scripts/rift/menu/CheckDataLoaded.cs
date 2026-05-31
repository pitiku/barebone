public class CheckDataLoaded : SequentialBehavior
{
    public override void activate()
    {
        base.activate();
        if (GameUtils.isInPreGameScenes()) // is mainmenu, or intro, or loading scene
        {
            DataManager.Instance.allowSave();
            DataManager.Instance.load();
        }
        else if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings == 1) // si se está buildeando solo una escena
        {
            DataManager.Instance.createEmptySaveData();
        }
        else
        {
            DataManager.Instance.allowSave();
            DataManager.Instance.load();
        }
    }

    public override void update()
    {
        base.update();

        if (DataManager.Instance.IsLoadDone() && DataManager.Instance.IsSavedDone())
        {
            toNextState();
        }
    }
}
