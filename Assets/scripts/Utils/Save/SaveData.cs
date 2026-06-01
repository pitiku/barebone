using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SaveData : AutoSerializableData
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

    public int m_iDifficulty = 0;
    public List<string> m_asAchievementsCompleted = new List<string>();
    #endregion

    #region Basic
    public SaveData() { }
    public SaveData(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }

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