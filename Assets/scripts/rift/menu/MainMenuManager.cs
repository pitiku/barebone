using Sirenix.OdinInspector;
using System;
using TMPro;
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : SceneSingleton<MainMenuManager>
{
    public TextMeshProUGUI m_oCurrentVersion;

    [FoldoutGroup("Root")] public Canvas m_oMenuOptions;
    [FoldoutGroup("Root")] public GameObject m_oMainMenuObj;

    [FoldoutGroup("States")] public State m_oOptionsState;
    [FoldoutGroup("States")] public State m_oGoToXanderTowerState;
    [FoldoutGroup("States")] public State m_oContinueRunState;
    [FoldoutGroup("States")] public State m_oCompendiumState;
    [FoldoutGroup("States")] public State m_oSettingsState;

    [FoldoutGroup("Settings")] public GameObject m_oOptionsScreen;
    [FoldoutGroup("Settings")] public Slider m_musicSlider;
    [FoldoutGroup("Settings")] public Slider m_soundSlider;
    [FoldoutGroup("Settings")] public Slider m_resolutions;
    [FoldoutGroup("Settings")] public Toggle m_fullscreen;
    [FoldoutGroup("Settings")] public Slider m_qualityLevel;
    [FoldoutGroup("Settings")] public Toggle m_oVSync;
    [FoldoutGroup("Settings")] public Toggle m_oShake;

    [FoldoutGroup("Settings")] public GameObject m_oDifficultyButtonsContainer;

    protected override void Awake()
    {
        base.Awake();
        m_oMainMenuObj.SetActive(false);
        m_oCurrentVersion.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
    }

    public void prepare()
    {
        GameplayManager.Instance.m_bAbandonRun = false;
        GameplayManager.Instance.m_bExitRun = false;

        //m_oCurrentVersion.text = DataManager.Instance.CurrentVersion;
        m_oCurrentVersion.gameObject.SetActive(true);
        m_oOptionsScreen.SetActive(false);

        m_oMenuOptions.worldCamera = GameplayManager.Instance.m_oManagementCamera;
    }

    public void showMenu()
    {
        m_oMainMenuObj.SetActive(true);
    }

    public void updateMenu()
    {
    }
}
