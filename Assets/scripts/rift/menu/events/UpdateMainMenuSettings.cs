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
        m_oMainMenuManager.m_musicSlider.value = SaveManager.SaveData.m_fMusicVolume * 10;
        m_oMainMenuManager.m_soundSlider.value = SaveManager.SaveData.m_fFXVolume * 10;

        // Shake
        m_oMainMenuManager.m_oShake.SetIsOnWithoutNotify(SaveManager.SaveData.m_bShake);

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

        m_oMainMenuManager.m_oOptionsScreen.SetActive(false);
        m_oMainMenuManager.m_oMainMenuObj.SetActive(true);
        SaveManager.Instance.saveGame();

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

    void setShake(bool _bValue)
    {
        SaveManager.SaveData.m_bShake = _bValue;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void activateWindows()
    {
        // Set Variables
        m_bFullScreen = SaveManager.SaveData.m_bFullscreen;
        m_iQuality = SaveManager.SaveData.m_iQualityLevel;
        m_bVSync = SaveManager.SaveData.m_bVSync;

        // resolution
        m_oMainMenuManager.m_fullscreen.SetIsOnWithoutNotify(m_bFullScreen);
        m_oMainMenuManager.m_qualityLevel.value = m_iQuality;
        m_oMainMenuManager.m_resolutions.value = (float)m_iResoutionIndex;
        m_oMainMenuManager.m_oVSync.SetIsOnWithoutNotify(SaveManager.SaveData.m_bVSync);

        // Add listeners
        m_oMainMenuManager.m_oVSync.onValueChanged.AddListener(setVSync);
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
        m_bFullScreen = SaveManager.SaveData.m_bFullscreen;
        m_oMainMenuManager.m_fullscreen.SetIsOnWithoutNotify(m_bFullScreen);
    }

    void applyGraphicsOptions()
    {
        bool bScreenOptionsDirtyQuality = m_bScreenOptionsDirtyQuality;
        if (bScreenOptionsDirtyQuality) GameOptionsManager.Instance.setQualityLevel(m_iQuality);
        if (bScreenOptionsDirtyQuality || m_bScreenOptionsDirtyVSYNC) GameOptionsManager.Instance.setVSync(m_bVSync);
    }

    void setVSync(bool _bValue)
    {
        m_bVSync = _bValue;
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

    bool m_bScreenOptionsDirtyFPS => (m_bFullScreen != SaveManager.SaveData.m_bFullscreen);
    bool m_bScreenOptionsDirtyQuality => m_iQuality != SaveManager.SaveData.m_iQualityLevel;
    bool m_bScreenOptionsDirtyVSYNC => m_bVSync != SaveManager.SaveData.m_bVSync;
    bool m_bScreenOptionsDirtyFullScreen => m_bFullScreen != SaveManager.SaveData.m_bFullscreen;
#endif
}
