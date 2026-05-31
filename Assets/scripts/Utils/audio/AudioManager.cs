using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Flags]
public enum eAudioFade
{
    None = 1 << 0,
    Sound = 1 << 1,
    Music = 1 << 2,
    All = 1 << 3,
}

public class DestroyAudioSource
{
    public GameObject oAudio;
    public float fTargetTime;
    public int iTimer = GameStateManager.APPLICATION_TIMER;
}

public class AudioManager : SceneSingleton<AudioManager>
{
    public GameObject m_o3DSource;
    public GameObject m_o2DSource;
    public Transform m_oPersistantParent;
    public float m_fBaseSourceVolume = 0.8f;

    public const string SOUND_TAG = "sound";
    public const string MUSIC_TAG = "music";

    public SoundObjectContainer m_oSoundContainer;

    public AudioMixerGroup m_oMasterGroup;
    public AudioMixerGroup m_oMusicGroup;
    public AudioMixerGroup m_oFXGroup;

    Dictionary<string, Object> m_aoOwnContainerSounds = new Dictionary<string, Object>();
    Dictionary<string, Object> m_aoContainerSounds = new Dictionary<string, Object>();

    float m_fFadeOutRatio;
    bool m_bFadingOut = false;
    float m_fFadeOutVolume;
    eAudioFade m_iAudioFade = eAudioFade.None;

    public AudioSource m_oCurrentSong = null;

    // destroy audio sources at the right time, to avoid problems when pausing sounds or changing time scale
    List<DestroyAudioSource> m_aoScheduledDestroys = new List<DestroyAudioSource>(100);

    Dictionary<AudioMixerGroup, float> m_dsMixerVolume = new Dictionary<AudioMixerGroup, float>();
    Dictionary<AudioMixerGroup, float> m_dsMixerVolumeMultiplier = new Dictionary<AudioMixerGroup, float>();
    Dictionary<AudioMixerGroup, float> m_dsMixerVolumeCurrent = new Dictionary<AudioMixerGroup, float>();

    Timer m_oMusicTimer;
    bool m_bFading = false;

    public void initialize()
    {
        m_oMusicTimer = new Timer(0.15f, eType.NeverStop);
        m_oMusicTimer.start(0);

        Object[] aoSounds = m_oSoundContainer.m_aoSoundObjects;
        for (int i = 0; i < aoSounds.Length; ++i)
        {
            m_aoOwnContainerSounds.Add(aoSounds[i].name, aoSounds[i]);
        }
        searchContainers();
    }

    protected override void Awake()
    {
        base.Awake();
        LoadingManager.Instance.subscribeOnLoadedScene(searchContainers);
        LoadingManager.Instance.subscribeOnExitedScene(onExitScene);
    }

    public void searchContainers()
    {
        SoundObjectContainer[] aoContainers = GameObject.FindObjectsByType<SoundObjectContainer>(0);
        for (int sc = 0; sc < aoContainers.Length; ++sc)
        {
            // don't get the objects of the container located in AudioManager (those are already loaded)
            if (aoContainers[sc].transform != transform)
            {
                Object[] aoSounds = aoContainers[sc].m_aoSoundObjects;
                for (int i = 0; i < aoSounds.Length; ++i)
                {
                    m_aoContainerSounds.Add(aoSounds[i].name, aoSounds[i]);
                }
            }
        }
    }

    public void onExitScene()
    {
        m_aoContainerSounds.Clear();
    }

    void Update()
    {
        // update scheduled destroys
        for (int i = 0; i < m_aoScheduledDestroys.Count; ++i)
        {
            if (GameStateManager.Instance.m_aoTimers[m_aoScheduledDestroys[i].iTimer] > m_aoScheduledDestroys[i].fTargetTime)
            {
                // check if the audio hasnt been previously removed
                if (m_aoScheduledDestroys[i].oAudio != null)
                {
                    m_aoScheduledDestroys[i].oAudio.destroy();
                }

                // unregister the scheduled destroy
                m_aoScheduledDestroys.RemoveAt(i);
                --i;
            }
        }

        // update fades
        if (m_bFadingOut)
        {
            if (m_fFadeOutVolume > 0.0f)
            {
                m_fFadeOutVolume -= m_fFadeOutRatio * Time.deltaTime;
                if (m_iAudioFade.HasFlag(eAudioFade.All))
                {
                    modifyAllVolumes(m_fFadeOutVolume, SOUND_TAG);
                    modifyAllVolumes(m_fFadeOutVolume, MUSIC_TAG);
                }
                else
                {
                    if (m_iAudioFade.HasFlag(eAudioFade.Sound))
                    {
                        modifyAllVolumes(m_fFadeOutVolume, SOUND_TAG);
                    }
                    if (m_iAudioFade.HasFlag(eAudioFade.Music))
                    {
                        modifyAllVolumes(m_fFadeOutVolume, MUSIC_TAG);
                    }
                }
            }
            else
            {
                m_bFadingOut = false;
                m_iAudioFade = 0;
            }
        }

        if (m_bFading)
        {
            updateMixerVolume(m_oMusicGroup);
            m_bFading = !m_oMusicTimer.isFinished();
        }
    }

    #region PLAY
    Object getAudioObjectFromContainers(string _sName)
    {
        if (m_aoContainerSounds.ContainsKey(_sName))
        {
            return m_aoContainerSounds[_sName];
        }

        if (m_aoOwnContainerSounds.ContainsKey(_sName))
        {
            return m_aoOwnContainerSounds[_sName];
        }

        Deb.logWarning("Somebody is trying to get a sound from containers with the name " + _sName + " but it hasn't been found.");
        return null;
    }

    public AudioSource playSongByName(string _sName, float _fVolume = 1.0f, bool _bLoops = true, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER)
    {
        Object oClip = getAudioObjectFromContainers(_sName);
        if (oClip == null)
        {
            return null;
        }

        return playSong((AudioClip)oClip, _fVolume, _bLoops, _iDestroyTimer);
    }

    public AudioSource playSong(AudioClip _oClip, float _fVolume = 1.0f, bool _bLoops = true, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER)
    {
        if (m_oCurrentSong != null)
        {
            if (m_oCurrentSong.name.EndsWith(_oClip.name))
            {
                return m_oCurrentSong;
            }

            fadeOutAndStopCurrentSong(1);
            disableCurrentSong(false);
        }

        GameObject oSound = m_o2DSource.getInstance();
        oSound.name = _oClip.name;
        oSound.tag = MUSIC_TAG;

        oSound.GetComponent<PitchController>().init(1, false);

        AddedFloat oBaseVolume = oSound.GetComponent<AddedFloat>();
        oBaseVolume.value = _fVolume * m_fBaseSourceVolume;

        AudioSource oSource = oSound.GetComponent<AudioSource>();
        oSource.outputAudioMixerGroup = m_oMusicGroup;
        oSource.volume = oBaseVolume.value;
        oSource.clip = _oClip;
        oSource.loop = _bLoops;

        oSource.Play();
        _fadeInCooroutine(oSource, 1);
        if (_bLoops)
        {
            m_oCurrentSong = oSource;
            persistCurrentSong(true);
        }
        else
        {
            scheduleDestroy(oSound, oSource.clip.length, _iDestroyTimer); // destroy object after clip duration
        }

        return oSource;
    }

    public AudioSource playSoundUnitByName(string _sName, float _fVolume = 1.0f, bool _bLoops = false, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER)
    {
        Object oSoundUnit = getAudioObjectFromContainers(_sName);
        if (oSoundUnit == null)
        {
            return null;
        }

        return playSoundUnit(((GameObject)oSoundUnit).GetComponent<SoundUnit>(), _fVolume, _bLoops, _iDestroyTimer);
    }

    public AudioSource playSoundUnit(SoundUnit _oUnit, float _fVolume = 1.0f, bool _bLoops = false, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER, bool _bAdaptPitchToTimeScale = true, bool _bFadeIn = false)
    {
        if (_oUnit != null)
        {
            return playAudioClip(_oUnit.m_aoClips.getRandom(), _fVolume * _oUnit.m_vVolumeVariance.range(), _bLoops, _oUnit.m_vPitchVariance.range(), _oUnit.m_vDelay.range(), _iDestroyTimer, null, _bAdaptPitchToTimeScale, _bFadeIn);
        }

        return null;
    }

    public AudioSource playAudioClipByName(string _sName, float _fVolume = 1.0f, bool _bLoops = true, float _fDelayed = 0f, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER)
    {
        Object oClip = getAudioObjectFromContainers(_sName);
        if (oClip == null)
        {
            return null;
        }

        return playAudioClip((AudioClip)oClip, _fVolume, _bLoops, _fDelayed, _iDestroyTimer);
    }

    public AudioSource playAudioClip(AudioClip _oClip, float _fVolume = 1.0f, bool _bLoops = false, float _fPitch = 1.0f, float _fDelayed = 0f, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER, AudioMixerGroup _oGroup = null, bool _bAdaptPitchToTimeScale = true, bool _bFadeIn = false)
    {
        ////Localize clip
        //_oClip = _oClip.getLocalizedAudioClip();

        GameObject oSound = m_o2DSource.getInstance();
        oSound.name = _oClip.name;
        oSound.tag = SOUND_TAG;

        oSound.GetComponent<PitchController>().init(_fPitch, _bAdaptPitchToTimeScale);

        AddedFloat oBaseVolume = oSound.GetComponent<AddedFloat>();
        oBaseVolume.value = _fVolume * m_fBaseSourceVolume;

        AudioSource oSource = oSound.GetComponent<AudioSource>();
        if (_oGroup != null)
        {
            oSource.outputAudioMixerGroup = _oGroup;
        }
        else
        {
            oSource.outputAudioMixerGroup = m_oFXGroup;
        }

        oSource.volume = oBaseVolume.value;
        oSource.pitch = _bAdaptPitchToTimeScale ? _fPitch * Time.timeScale : _fPitch;
        oSource.clip = _oClip;
        oSource.loop = _bLoops;

        if (_fDelayed == 0)
        {
            if (_bFadeIn)
            {
                _fadeInCooroutine(oSource);
            }
            oSource.Play();
        }
        else
        {
            if (_bFadeIn)
            {
                _fadeInCooroutineDelayed(oSource, _fDelayed);
            }
            oSource.PlayDelayed(_fDelayed);
        }

        if (!_bLoops)
        {
            scheduleDestroy(oSound, oSource.clip.length, _iDestroyTimer);
        }
        return oSource;
    }

    public AudioSource playAudioClipXY(AudioClip _oClip, Vector2 _vPosition, float _fVolume = 1.0f, bool _bLoops = false, float _fPitch = 1.0f, int _iDestroyTimer = GameStateManager.APPLICATION_TIMER, AudioMixerGroup _oGroup = null, bool _bAdaptPitchToTimeScale = true)
    {
        GameObject oSound = m_o3DSource.getInstance();
        oSound.transform.posXY(_vPosition);
        oSound.name = _oClip.name;
        oSound.tag = SOUND_TAG;

        oSound.GetComponent<PitchController>().init(_fPitch, _bAdaptPitchToTimeScale);

        AddedFloat oBaseVolume = oSound.GetComponent<AddedFloat>();
        oBaseVolume.value = _fVolume * m_fBaseSourceVolume;

        AudioSource oSource = oSound.GetComponent<AudioSource>();
        if (_oGroup != null)
        {
            oSource.outputAudioMixerGroup = _oGroup;
        }
        else
        {
            oSource.outputAudioMixerGroup = m_oFXGroup;
        }

        oSource.volume = oBaseVolume.value;
        oSource.pitch = _bAdaptPitchToTimeScale ? _fPitch * Time.timeScale : _fPitch;
        oSource.clip = _oClip;
        oSource.loop = _bLoops;

        oSource.Play();

        if (!_bLoops)
        {
            scheduleDestroy(oSound, oSource.clip.length, _iDestroyTimer);
        }
        return oSource;
    }
    #endregion

    #region STOP
    public void scheduleDestroy(GameObject _oGO, float _fLength, int _iDestroyTimer)
    {
        DestroyAudioSource oDAS = new DestroyAudioSource();
        oDAS.fTargetTime = GameStateManager.Instance.m_aoTimers[_iDestroyTimer] + _fLength + 1.0f; // take current timer time, and add the length of this sound plus a small margin
        oDAS.iTimer = _iDestroyTimer;
        oDAS.oAudio = _oGO;
        m_aoScheduledDestroys.Add(oDAS);
    }

    public void stop(ref AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.gameObject.destroy();
            audioSource = null;
        }
    }

    public void stopAll()
    {
        Object[] aoSounds = FindObjectsByType(typeof(AudioSource), 0);
        for (int i = 0; i < aoSounds.Length; ++i)
        {
            ((GameObject)aoSounds[i]).destroy();
        }

        m_aoScheduledDestroys.Clear();
    }

    public void stopAllSounds()
    {
        GameObject[] aoSounds = GameObject.FindGameObjectsWithTag(SOUND_TAG);
        for (int i = 0; i < aoSounds.Length; ++i)
        {
            if (!aoSounds[i].name.StartsWith("DDC_"))
            {
                aoSounds[i].destroy();
            }
        }

        m_aoScheduledDestroys.Clear();
    }

    public void stopAllMusic()
    {
        GameObject[] aoSounds = GameObject.FindGameObjectsWithTag(MUSIC_TAG);
        for (int i = 0; i < aoSounds.Length; ++i)
        {
            if (!aoSounds[i].name.StartsWith("DDC_"))
            {
                aoSounds[i].destroy();
            }
        }

        disableCurrentSong(true);
    }
    #endregion

    #region FADE & VOLUMES
    System.Collections.IEnumerator _fadeOutAndStopCooroutine(AudioSource audioSource, float fFadeTime)
    {
        float fVolumeStart = audioSource.volume;
        fFadeTime = Mathf.Max(0.01f, fFadeTime);

        while (audioSource != null && audioSource.volume > 0)
        {
            audioSource.volume -= fVolumeStart * Time.deltaTime / fFadeTime;

            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.gameObject.destroy();
        }
    }
    System.Collections.IEnumerator _fadeInCooroutine(AudioSource audioSource, float fFadeTime = 0.1f)
    {
        float fVolumeStart = audioSource.volume;
        fFadeTime = Mathf.Max(0.01f, fFadeTime);
        audioSource.volume = 0;

        float fElapsedTime = 0f;
        while (audioSource != null && fElapsedTime < fFadeTime)
        {
            fElapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, fVolumeStart, fElapsedTime / fFadeTime);
            yield return null;
        }

        if (audioSource != null)
            audioSource.volume = fVolumeStart;
    }

    System.Collections.IEnumerator _fadeInCooroutineDelayed(AudioSource audioSource, float fDelay, float fFadeTime = 0.1f)
    {
        yield return new WaitForSeconds(Mathf.Max(0, fDelay));

        _fadeInCooroutine(audioSource, fFadeTime);
    }

    public void fadeOutAndStop(AudioSource audioSource, float fFadeTime = 0.1f)
    {
        if (audioSource != null)
        {
            StartCoroutine(_fadeOutAndStopCooroutine(audioSource, fFadeTime));
        }
    }

    public void fadeOutAndStopCurrentSong(float fFadeTime)
    {
        if (m_oCurrentSong != null)
        {
            StartCoroutine(_fadeOutAndStopCooroutine(m_oCurrentSong, fFadeTime));
            m_oCurrentSong = null;
        }
    }

    public void setMixerVolume(AudioMixerGroup _oGroup, float _fValue)
    {
        m_dsMixerVolume[_oGroup] = _fValue;
        updateMixerVolume(_oGroup);
    }

    public void setMixerVolumeMultiplier(AudioMixerGroup _oGroup, float _fValue)
    {
        if (_oGroup == m_oMusicGroup)
        {
            m_oMusicTimer.start(0.15f);
            m_bFading = true;
        }

        m_dsMixerVolumeMultiplier[_oGroup] = _fValue;
        updateMixerVolume(_oGroup);
    }

    private void updateMixerVolume(AudioMixerGroup _oGroup)
    {
        float fVolume = m_dsMixerVolume.ContainsKey(_oGroup) ? m_dsMixerVolume[_oGroup] : 1;
        float fVolumeMultiplier = m_dsMixerVolumeMultiplier.ContainsKey(_oGroup) ? m_dsMixerVolumeMultiplier[_oGroup] : 1;

        float fWantedVolume = fVolume * fVolumeMultiplier;
        float fCurrentVolume = m_dsMixerVolumeCurrent.ContainsKey(_oGroup) ? m_dsMixerVolumeCurrent[_oGroup] : fWantedVolume;

        float fVolumeFinal = fWantedVolume;
        if (_oGroup == m_oMusicGroup)
        {
            float fProgress = m_oMusicTimer == null ? 0 : m_oMusicTimer.getProgress();
            fVolumeFinal = Mathf.Lerp(fCurrentVolume, fWantedVolume, fProgress);
        }

        m_dsMixerVolumeCurrent[_oGroup] = fVolumeFinal;

        _oGroup.audioMixer.SetFloat(_oGroup.name + "Volume", linearToDecibel(fVolumeFinal));
    }

    public void modifyAllVolumes(float _fValue, string _sTag)
    {
        GameObject[] aoSounds = GameObject.FindGameObjectsWithTag(_sTag);
        for (int i = 0; i < aoSounds.Length; ++i)
        {
            modifyVolume(aoSounds[i], _fValue);
        }

        if (_fValue.approximately(0.0f) && _sTag == MUSIC_TAG)
        {
            disableCurrentSong(true);
        }
    }

    public void modifyVolume(GameObject _oSound, float _fValue)
    {
        AudioSource aSource = _oSound.GetComponent<AudioSource>();
        AddedFloat af = _oSound.GetComponent<AddedFloat>();
        aSource.volume = af.value * _fValue;
    }

    public void setAudioSpeedScale(float _fScale)
    {
        GameObject[] aoObjects = GameObject.FindGameObjectsWithTag(SOUND_TAG);
        for (int i = 0; i < aoObjects.Length; ++i)
        {
            aoObjects[i].GetComponent<AudioSource>().pitch = _fScale;
        }
        aoObjects = GameObject.FindGameObjectsWithTag(MUSIC_TAG);
        for (int i = 0; i < aoObjects.Length; ++i)
        {
            aoObjects[i].GetComponent<AudioSource>().pitch = _fScale;
        }
    }

    public void fadeOutAllSounds(float _fTime)
    {
        m_fFadeOutRatio = 1.0f / _fTime;
        m_fFadeOutVolume = 1.0f;
        m_bFadingOut = true;
        m_iAudioFade |= eAudioFade.Sound;
    }

    public void fadeOutAllMusic(float _fTime)
    {
        m_fFadeOutRatio = 1.0f / _fTime;
        m_fFadeOutVolume = 1.0f;
        m_bFadingOut = true;
        m_iAudioFade |= eAudioFade.Music;
    }

    public void fadeOutAll(float _fTime)
    {
        m_fFadeOutRatio = 1.0f / _fTime;
        m_fFadeOutVolume = 1.0f;
        m_bFadingOut = true;
        m_iAudioFade = eAudioFade.All;
    }
    #endregion

    #region HELPERS

    public void disableCurrentSong(bool _bDestroySong)
    {
        if (m_oCurrentSong != null)
        {
            m_oCurrentSong.transform.parent = null;
            if (_bDestroySong) Destroy(m_oCurrentSong.gameObject);
            m_oCurrentSong = null;
        }
    }

    public void persistCurrentSong(bool _b)
    {
        if (_b)
        {
            if (m_oCurrentSong != null)
            {
                m_oCurrentSong.transform.parent = m_oPersistantParent;
            }
        }
        else
        {
            if (m_oCurrentSong != null)
            {
                m_oCurrentSong.transform.parent = null;
            }
        }
    }

    float linearToDecibel(float linear)
    {
        float dB;

        if (linear != 0)
        {
            dB = 20.0f * Mathf.Log10(linear);
        }
        else
        {
            dB = -144.0f;
        }

        return dB;
    }

    float decibelToLinear(float dB)
    {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);

        return linear;
    }
    #endregion
}
