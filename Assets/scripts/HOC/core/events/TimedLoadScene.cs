using UnityEngine.SceneManagement;

[System.Serializable]
public class TimedLoadScene : TimedEvent
{
    public bool m_bCurrentScene = false;
    public string m_sScene;
    public bool m_bLoadingScreen = true;

    public bool m_bFadeOutSounds = true;
    public bool m_bFadeOutMusic = true;

    public override void play()
    {
        base.play();

        LoadingManager.Instance.LoadScene(m_bCurrentScene ? SceneManager.GetSceneAt(SceneManager.sceneCount - 1).name : m_sScene, m_bFadeOutSounds, m_bFadeOutMusic, m_bLoadingScreen);
    }
}
