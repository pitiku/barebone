using System;
using UnityEngine;

public class GameOptionsManager : SceneSingleton<GameOptionsManager>
{
#if UNITY_EDITOR || UNITY_STANDALONE

    public void Update()
    {
    }
#endif

    public void applyOptions()
    {
        applyMusicVolume();
        applySoundVolume();

#if UNITY_EDITOR || UNITY_STANDALONE
        applyQualityLevel();
        applyVSync();
#endif

        applyVibration();

        I2.Loc.LocalizationManager.CurrentLanguageCode = SaveManager.SaveData.m_sLanguageCode;
    }

    #region Appliers

    public void applyMusicVolume()
    {
        AudioManager.Instance.setMixerVolume(AudioManager.Instance.m_oMusicGroup, SaveManager.SaveData.m_fMusicVolume);
    }


    public void applySoundVolume()
    {
        AudioManager.Instance.setMixerVolume(AudioManager.Instance.m_oFXGroup, SaveManager.SaveData.m_fFXVolume);
    }

    void applyVibration()
    {
        RewiredManager.Instance.m_bVibrationEnabled = SaveManager.SaveData.m_bCameraShake;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void applyQualityLevel()
    {
        QualitySettings.SetQualityLevel(SaveManager.SaveData.m_iQualityLevel);
    }

    public void applyVSync()
    {
        QualitySettings.vSyncCount = SaveManager.SaveData.m_bVSync ? 1 : 0;
    }
#endif
    #endregion

    #region Setters

#if UNITY_EDITOR || UNITY_STANDALONE
    public void setVSync(bool _bValue)
    {
        SaveManager.SaveData.m_bVSync = _bValue;
        applyVSync();
    }

    public void setQualityLevel(int level)
    {
        SaveManager.SaveData.m_iQualityLevel = level;
        applyQualityLevel();
    }
#endif

    public void setSoundVolume(float value)
    {
        SaveManager.SaveData.m_fFXVolume = value;
        applySoundVolume();
    }

    public void setMusicVolume(float value)
    {
        SaveManager.SaveData.m_fMusicVolume = value;
        applyMusicVolume();
    }

    public void setDifficulty(int level)
    {
        SaveManager.SaveData.m_iDifficulty = level;
    }

    #endregion
}