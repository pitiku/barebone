using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SceneSingleton<SoundManager>
{
    private AudioSource m_oSceneMusic;
    private AudioSource m_oSceneAmbienceMusic;
    private AudioSource m_oWeatherAmbienceMusic;

    private float m_fFadeTime = 0.5f;

    private Dictionary<string, AudioSource> m_aoBarksPlaying = new();

    public void fadeOutCurrentSceneMusic()
    {
        if (m_oSceneMusic != null)
        {
            AudioManager.Instance.fadeOutAndStopCurrentSong(m_fFadeTime);
        }
    }

    AudioSource play(SoundUnit oSB, bool _bSong, bool _bLoop)
    {
        if (oSB != null && !oSB.m_aoClips.isNullOrEmpty())
        {
            if (_bSong)
            {
                return AudioManager.Instance.playSong(oSB.m_aoClips.getRandom(), 1 * oSB.m_vVolumeVariance.range(null, ""), _bLoop);
            }
            return AudioManager.Instance.playAudioClip(oSB.m_aoClips.getRandom(), 1 * oSB.m_vVolumeVariance.range(null, ""), _bLoop);
        }
        return null;
    }

    public void fadeCurrentSongAndAmbience(float _fFadeTime)
    {
        if (m_oSceneAmbienceMusic != null)
        {
            AudioManager.Instance.fadeOutAndStop(m_oSceneAmbienceMusic, _fFadeTime);
            m_oSceneAmbienceMusic = null;
        }
        if (m_oWeatherAmbienceMusic != null)
        {
            AudioManager.Instance.fadeOutAndStop(m_oWeatherAmbienceMusic, _fFadeTime);
            m_oWeatherAmbienceMusic = null;
        }
        AudioManager.Instance.fadeOutAndStopCurrentSong(_fFadeTime);
        AudioManager.Instance.disableCurrentSong(false);
    }

    public void clearCurrentSounds()
    {
        m_aoBarksPlaying.Clear();
    }
}
