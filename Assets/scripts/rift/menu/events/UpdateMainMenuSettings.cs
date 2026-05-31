using System;

public class UpdateMainMenuSettings : SequentialBehavior
{
#if UNITY_EDITOR || UNITY_STANDALONE
    bool m_bFullScreen = false;
    int m_iResoutionIndex = 0;
    int m_iQuality = 0;
    bool m_bVSync;
#endif

    MainMenuManager m_oMainMenuManager;

    protected override void Awake()
    {
        base.Awake();
        m_oMainMenuManager = MainMenuManager.Instance;
    }

    public override void activate()
    {
        base.activate();

        // Activate Objects
        m_oMainMenuManager.m_oOptionsScreen.SetActive(true);
        m_oMainMenuManager.m_oMainMenuObj.SetActive(false);

        // Set Sliders
        m_oMainMenuManager.m_musicSlider.value = DataManager.SaveDataOptions.m_fMusicVolume * 10;
        m_oMainMenuManager.m_soundSlider.value = DataManager.SaveDataOptions.m_fFXVolume * 10;

        // Shake
        m_oMainMenuManager.m_oShake.SetIsOnWithoutNotify(DataManager.SaveDataOptions.m_bShake);

        // Add listeners
        m_oMainMenuManager.m_oShake.onValueChanged.AddListener(setShake);
        m_oMainMenuManager.m_musicSlider.onValueChanged.AddListener(setMusicVolume);
        m_oMainMenuManager.m_soundSlider.onValueChanged.AddListener(setSoundVolume);

#if UNITY_EDITOR || UNITY_STANDALONE
        activateWindows();
#endif
    }

    public override void update()
    {
        base.update();

        if (GameUtils.cancelButton())
        {
            exit();
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        updateWindows();
#endif
    }

    void exit()
    {
        m_oMainMenuManager.m_oShake.onValueChanged.RemoveListener(setShake);
        m_oMainMenuManager.m_musicSlider.onValueChanged.RemoveListener(setMusicVolume);
        m_oMainMenuManager.m_soundSlider.onValueChanged.RemoveListener(setSoundVolume);
        m_oMainMenuManager.m_oAnalyticsSlider.onValueChanged.RemoveListener(setAnalytics);

        m_oMainMenuManager.m_oOptionsScreen.SetActive(false);
        m_oMainMenuManager.m_oMainMenuObj.SetActive(true);
        DataManager.Instance.saveGame(true, false);

#if UNITY_EDITOR || UNITY_STANDALONE
        exitWindows();
#endif

        toNextState();
    }

    void setMusicVolume(float value)
    {
        GameOptionsManager.Instance.setMusicVolume(value * 0.1f);
    }

    void setSoundVolume(float value)
    {
        GameOptionsManager.Instance.setSoundVolume(value * 0.1f);
    }

    void setAnalytics(float _fValue)
    {
        DataManager.SaveDataOptions.m_bSendAnalyticsData = _fValue > 0.5;
    }


    void setShake(bool _bValue)
    {
        DataManager.SaveDataOptions.m_bShake = _bValue;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void activateWindows()
    {
        // Set Variables
        m_bFullScreen = DataManager.SaveDataOptions.m_bFullscreen;
        m_iQuality = DataManager.SaveDataOptions.m_iQualityLevel;
        //m_iResoutionIndex = GameOptionsManager.Instance.m_oResolutionManager.getResolutionIndex();
        m_bVSync = DataManager.SaveDataOptions.m_bVSync;

        // analytics
        m_oMainMenuManager.m_oAnalyticsSlider.value = DataManager.SaveDataOptions.m_bSendAnalyticsData ? 1 : 0;

        // resolution
        m_oMainMenuManager.m_fullscreen.SetIsOnWithoutNotify(m_bFullScreen);
        //GameOptionsManager.Instance.m_oResolutionManager.m_oFullScreenShotcutPressedEvent += onFullScreenShotcutPressedEvent;
        m_oMainMenuManager.m_qualityLevel.value = m_iQuality;
        m_oMainMenuManager.m_resolutions.value = (float)m_iResoutionIndex;
        m_oMainMenuManager.m_oVSync.SetIsOnWithoutNotify(DataManager.SaveDataOptions.m_bVSync);

        // Add listeners
        m_oMainMenuManager.m_oVSync.onValueChanged.AddListener(setVSync);
        m_oMainMenuManager.m_oAnalyticsSlider.onValueChanged.AddListener(setAnalytics);
        m_oMainMenuManager.m_resolutions.onValueChanged.AddListener(setResolutions);
        m_oMainMenuManager.m_fullscreen.onValueChanged.AddListener(setFullScreen);
        m_oMainMenuManager.m_qualityLevel.onValueChanged.AddListener(setQualityLevel);
    }

    private void updateWindows()
    {
        if (m_bScreenOptionsDirty && GameUtils.acceptButton(false))
        {
            applyGraphicsOptions();
        }
    }

    public override void deactivate()
    {
        base.deactivate();
        //GameOptionsManager.Instance.m_oResolutionManager.m_oFullScreenShotcutPressedEvent -= onFullScreenShotcutPressedEvent;
    }

    private void exitWindows()
    {
        m_oMainMenuManager.m_oVSync.onValueChanged.RemoveListener(setVSync);
        m_oMainMenuManager.m_resolutions.onValueChanged.RemoveListener(setResolutions);
        m_oMainMenuManager.m_fullscreen.onValueChanged.RemoveListener(setFullScreen);
        m_oMainMenuManager.m_qualityLevel.onValueChanged.RemoveListener(setQualityLevel);
    }

    private void onFullScreenShotcutPressedEvent(object sender, EventArgs e)
    {
        m_bFullScreen = DataManager.SaveDataOptions.m_bFullscreen;
        m_oMainMenuManager.m_fullscreen.SetIsOnWithoutNotify(m_bFullScreen);
    }

    void applyGraphicsOptions()
    {
        bool bScreenOptionsDirtyQuality = m_bScreenOptionsDirtyQuality;
        if (bScreenOptionsDirtyQuality) GameOptionsManager.Instance.setQualityLevel(m_iQuality);
        //if (m_bScreenOptionsDirtyFullScreen || m_bScreenOptionsDirtyResolutionIndex)
        //    GameOptionsManager.Instance.m_oResolutionManager.setResolutionIndex(m_iResoutionIndex, m_bFullScreen);
        //Quality setting may override vsync and FPS limit, so when dirty always set them
        if (bScreenOptionsDirtyQuality || m_bScreenOptionsDirtyVSYNC) GameOptionsManager.Instance.setVSync(m_bVSync);
        //if (bScreenOptionsDirtyQuality || m_bScreenOptionsDirtyFPS) GameOptionsManager.Instance.m_oFPSLimitManager.setFPS(m_oMainMenuManager.m_oFPSLimit.SelectedIndex);
    }

    void setVSync(bool _bValue)
    {
        m_bVSync = _bValue;
        //MainMenuManager.Instance.m_oFPSLimit.setup(_bValue);
    }

    void setQualityLevel(float _fValue)
    {
        m_iQuality = (int)_fValue;
    }
    void setFullScreen(bool _bValue)
    {
        m_bFullScreen = _bValue;
    }

    void setResolutions(float _fValue)
    {
        m_iResoutionIndex = (int)m_oMainMenuManager.m_resolutions.value;
    }

    bool m_bScreenOptionsDirty
    {
        get
        {
            return m_bScreenOptionsDirtyFPS || m_bScreenOptionsDirtyQuality || m_bScreenOptionsDirtyVSYNC || m_bScreenOptionsDirtyFullScreen;
        }
    }

    bool m_bScreenOptionsDirtyFPS => (m_bFullScreen != DataManager.SaveDataOptions.m_bFullscreen);
    bool m_bScreenOptionsDirtyQuality => m_iQuality != DataManager.SaveDataOptions.m_iQualityLevel;
    bool m_bScreenOptionsDirtyVSYNC => m_bVSync != DataManager.SaveDataOptions.m_bVSync;
    bool m_bScreenOptionsDirtyFullScreen => m_bFullScreen != DataManager.SaveDataOptions.m_bFullscreen;
    //bool m_bScreenOptionsDirtyResolutionIndex => m_iResoutionIndex != GameOptionsManager.Instance.m_oResolutionManager.getResolutionIndex();
#endif
}
