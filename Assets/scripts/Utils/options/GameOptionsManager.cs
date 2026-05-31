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

        I2.Loc.LocalizationManager.CurrentLanguageCode = DataManager.SaveDataOptions.m_sLanguageCode;
    }

    #region Appliers

    public void applyMusicVolume()
    {
        AudioManager.Instance.setMixerVolume(AudioManager.Instance.m_oMusicGroup, DataManager.SaveDataOptions.m_fMusicVolume);
    }


    public void applySoundVolume()
    {
        AudioManager.Instance.setMixerVolume(AudioManager.Instance.m_oFXGroup, DataManager.SaveDataOptions.m_fFXVolume);
    }

    void applyVibration()
    {
        RewiredManager.Instance.m_bVibrationEnabled = DataManager.SaveDataOptions.m_bCameraShake;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void applyQualityLevel()
    {
        QualitySettings.SetQualityLevel(DataManager.SaveDataOptions.m_iQualityLevel);
    }

    public void applyVSync()
    {
        QualitySettings.vSyncCount = DataManager.SaveDataOptions.m_bVSync ? 1 : 0;
    }
#endif
    #endregion

    #region Setters

#if UNITY_EDITOR || UNITY_STANDALONE
    public void setVSync(bool _bValue)
    {
        DataManager.SaveDataOptions.m_bVSync = _bValue;
        applyVSync();
    }

    public void setQualityLevel(int level)
    {
        DataManager.SaveDataOptions.m_iQualityLevel = level;
        applyQualityLevel();
    }
#endif

    public void setSoundVolume(float value)
    {
        DataManager.SaveDataOptions.m_fFXVolume = value;
        applySoundVolume();
    }

    public void setMusicVolume(float value)
    {
        DataManager.SaveDataOptions.m_fMusicVolume = value;
        applyMusicVolume();
    }

    public void setDifficulty(int level)
    {
        DataManager.SaveDataProgression.m_iDifficulty = level;
    }

    #endregion
}