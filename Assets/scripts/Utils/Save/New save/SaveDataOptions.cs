using Sirenix.OdinInspector;
using System;
using System.Runtime.Serialization;
using UnityEngine;

public interface ISave
{
    public string Version { get; set; }

    public void initialize();
    public void prepareSave();
    public void load();
}

[Serializable]
public class SaveDataOptions : AutoSerializableData, ISave
{
    public string m_sVersion;

    #region Declarations
    [FoldoutGroup("Options")] public float m_fMusicVolume = 1f;
    [FoldoutGroup("Options")] public float m_fFXVolume = 1f;
    [FoldoutGroup("Options")] public bool m_bCameraShake = false;
    [FoldoutGroup("Options")] public bool m_bSubtitles = false;
    [FoldoutGroup("Options")] public bool m_bFullscreen = true;
    [FoldoutGroup("Options")] public bool m_bShake = true;
    [FoldoutGroup("Options")] public int m_iResolutionWidth = 1920;
    [FoldoutGroup("Options")] public int m_iResolutionHeight = 1080;
    [FoldoutGroup("Options")] public int m_iQualityLevel = 4;
    [FoldoutGroup("Options")] public string m_sLanguageCode;

#if UNITY_STANDALONE || UNITY_EDITOR
    //Index of the FPS limit (check GameOptionsManager.m_saiFPSLimits for actual values)
    [FoldoutGroup("Options")] public int m_iFPS = -1;
    [FoldoutGroup("Options")] public bool m_bVSync = false;
#endif

    [FoldoutGroup("Other")] public bool m_bSendAnalyticsData = true;

    public int m_iCurrentSlot = 0;
    #endregion

    #region Basic
    public SaveDataOptions() { }
    public SaveDataOptions(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }

    public string Version { get => m_sVersion; set => m_sVersion = value; }

    public void initialize()
    {
        m_iQualityLevel = QualitySettings.count - 1;
#if UNITY_EDITOR || UNITY_STANDALONE
        //GameOptionsManager.Instance.m_oResolutionManager.checkCurrentResolution();
#endif
        checkSystemLanguage();
        GameOptionsManager.Instance.applyOptions();
    }

    public void load()
    {
        GameOptionsManager.Instance.applyOptions();
    }

    public void prepareSave()
    {
        m_sVersion = DataManager.CurrentVersion;
    }

    void checkSystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                m_sLanguageCode = Localization.CHINESE_CODE;
                break;
            default:
                m_sLanguageCode = Localization.ENGLISH_CODE;
                break;
        }
    }
    #endregion
}