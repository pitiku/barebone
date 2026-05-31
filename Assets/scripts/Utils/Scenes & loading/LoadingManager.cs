using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public delegate void StartLoadingSceneHandler(Scene _oScene);

public class LoadingManager : StateMachineSingleton<LoadingManager>
{
    public struct SceneEntered { }
    public struct SceneExited { }

    public State m_oStartLoadingState;
    public string m_sLoadingScene;

    private AsyncOperation m_oAsyncOperation;
    private string m_sSceneToLoad = "";
    private bool m_bLoadingScreen = false;

    [HideInInspector]
    public int m_iScenesLoaded = 0;

    bool m_bLoadingScene = false;

    private CompositeDisposable m_oCompositeDisposable = new();

    private void OnDestroy()
    {
        if (isNull()) return;
        m_oCompositeDisposable?.Dispose();
    }

    // Loading methods
    public void LoadScene(string _sScene)
    {
        LoadScene(_sScene, false, true, true);
    }

    public void LoadScene(string _sScene, bool _bFadeSounds, bool _bFadeMusic, bool _bLoadingScreen = true)
    {
        m_bLoadingScene = true;

        m_sSceneToLoad = _sScene;
        m_bLoadingScreen = _bLoadingScreen;
        HOCUtils.changeLonelyState(m_oStartLoadingState, this);
    }

    // Loading flow methods
    public bool UseLoadingScreen()
    {
        return m_bLoadingScreen;
    }

    public void LoadLoadingScene()
    {
        EventManager.TriggerEventsWithArgs(new SceneExited());
        ++m_iScenesLoaded;
        SceneManager.LoadScene(m_sLoadingScene);
    }

    //Sync loading
    public void ExecuteLoading()
    {
        ++m_iScenesLoaded;
        SceneManager.LoadScene(m_sSceneToLoad);
    }

    //Async loading
    public void ExecuteAsyncLoading()
    {
        ++m_iScenesLoaded;
        Deb.log($"[System] Loading scene: {m_sSceneToLoad}");
        m_oAsyncOperation = SceneManager.LoadSceneAsync(m_sSceneToLoad);
        m_oAsyncOperation.allowSceneActivation = false;
    }

    public bool IsAsyncLoadReadyToComplete()
    {
        return m_oAsyncOperation == null || m_oAsyncOperation.progress >= 0.9f;
    }

    public void CompleteLoading()
    {
        if (m_oAsyncOperation != null)
        {
            m_oAsyncOperation.allowSceneActivation = true;
        }
    }

    public bool IsLoadingCompleted()
    {
        return m_oAsyncOperation == null || m_oAsyncOperation.isDone;
    }

    public void CollectGarbage()
    {
        GC.Collect();
    }

    public bool IsSyncLoadingDone()
    {
        return SceneManager.GetSceneAt(SceneManager.sceneCount - 1) == SceneManager.GetSceneByName(m_sSceneToLoad);
    }

    public bool IsLoadDone()
    {
#if UNITY_XBOXONE
        return !XboxOneManager.Instance.HasUser() || DataManager.Instance.IsLoadDone();
#else
        return DataManager.Instance.IsLoadDone();
#endif
    }

    public bool isLoadSceneFinished()
    {
        return !m_bLoadingScene;
    }

    public void loadSceneFinished()
    {
        EventManager.TriggerEventsWithArgs(new SceneEntered());
        m_bLoadingScene = false;
    }

    public void subscribeOnLoadedScene(Action<SceneEntered> action)
    {
        EventManager.Subscribe(action).AddTo(m_oCompositeDisposable);
    }

    public void subscribeOnExitedScene(Action<SceneExited> action)
    {
        EventManager.Subscribe(action).AddTo(m_oCompositeDisposable);
    }

    // Wrap the parameterless action into an Action<SceneExited>
    public void subscribeOnLoadedScene(Action action)
    {
        subscribeOnLoadedScene(_ => action());
    }

    public void subscribeOnExitedScene(Action action)
    {
        subscribeOnExitedScene(_ => action());
    }
}