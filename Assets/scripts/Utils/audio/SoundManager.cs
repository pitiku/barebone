using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum eSceneMusicPlaying
{
    RELAX,
    SILENCE,
    PAD,
    CUSTOM,
    COMBAT
}

public enum eReceivedHitType
{
    Blood,
    Crystal,
    Wood,
    Metal,
    Bone,
    None
}

public enum eHitImpactType
{
    Impact,
    Projectile,
    Cut,
    Magic,
    Bite,
    Explosion,
    None
}

// Footstep enums and signals
public enum eFootStepType
{
    None,
    Dirt,
    Grass,
    Water,
    Sand,
    Rock,
    Underground
}

public enum eFootStepEntity
{
    None,
    Biped,
    Insectoid,
    Quadruped,
    Wing,
    Slither,
}

public enum eFootStepSize
{
    None,
    Small,
    Medium,
    Large
}

public enum eBarkSoundsType
{
    Pensive,
    Rage,
    Positive,
    Sad,
    Taunt,
    Triumph,
    RageMad,
    TauntMad,
    TriumphMad,
}

// Signal for the EventManager
public struct FootstepSignal
{
    public string m_iEntityId;
    public eFootStepEntity m_eEntity;
    public eFootStepSize m_eSize;
    public eFootStepType m_eSurface;
}

public class SoundManager : SceneSingleton<SoundManager>
{
    [Serializable]
    public class HitFXAndSounds
    {
        public Vector2 m_vDamageDealt;
        public SoundUnit m_oHitFXSound;
    }

    [Serializable]
    public struct HitTypeAndFX
    {
        public eReceivedHitType m_eHitType;
        public HitFXAndSounds[] m_aoHitFXSounds;
    }

    [Serializable]
    public struct HitImpactTypeAndFX
    {
        public eHitImpactType m_eHitType;
        public HitFXAndSounds[] m_aoHitFXSounds;
    }

    [Serializable]
    public class BarkSoundsAndEntity
    {
        public SoundUnit m_oFallbackBarkSound;
        public BarkSounds[] m_aoBarkSounds;
    }

    [Serializable]
    public class BarkSounds
    {
        public eBarkSoundsType m_eBarkSoundsType;
        public SoundUnit m_oBarkSound;
    }

    [Serializable]
    public class Song
    {
        public AudioClip m_oClip;
        public float m_fVolume = 1.0f;
        public bool m_bLoops = true;
    }

    [Serializable]
    public class SongDaytime
    {
        public eTimeOfDay m_eDaytime;
        public SoundUnit m_oSong;
    }

    [SerializeField] private float m_fFadeTime = 1;
    [FoldoutGroup("Scene"), SerializeField] private SoundUnit m_oPadSong;
    [FoldoutGroup("Combat"), SerializeField] private SoundUnit m_oWinSong;
    [FoldoutGroup("Combat"), SerializeField] private SoundUnit m_oDefeatSong;
    [FoldoutGroup("MainMenu"), SerializeField] private SoundUnit m_oMainMenuSong;
    [FoldoutGroup("Map"), SerializeField] private SoundUnit m_oMapSong;
    [FoldoutGroup("Map")] public SoundUnit m_oMapHiddenNodeAppear;
    [FoldoutGroup("Campfire"), SerializeField] private SoundUnit m_oCampfireSong;
    [FoldoutGroup("Campfire")] public SoundUnit m_sConsumeWoodSound;
    [FoldoutGroup("Campfire")] public SoundUnit m_sConsumeFoodSound;
    [FoldoutGroup("Campfire")] public SoundUnit m_oDarkEventSound;
    [FoldoutGroup("Items")] public SoundUnit m_oEquipSound;
    [FoldoutGroup("Items")] public SoundUnit m_oUnEquipSound;
    [FoldoutGroup("TeamScreen")] public SoundUnit m_oReleaseCharacterSound;
    [FoldoutGroup("Loot")] public SoundUnit m_oLootWindowAppearSound;
    [FoldoutGroup("Combat")] public SoundUnit m_oDodgeSound;
    [FoldoutGroup("Combat")] public SoundUnit m_oMissSound;
    [FoldoutGroup("Combat")] public SoundUnit m_oShredSound;
    [FoldoutGroup("Combat")] public SoundUnit m_oSkillError;
    [FoldoutGroup("Combat")] public SoundUnit m_oGlobalError;
    [FoldoutGroup("Combat")] public HitTypeAndFX[] m_aoHitSFXs;
    [FoldoutGroup("Combat")] public HitImpactTypeAndFX[] m_aoHitImpactSFXs;
    [FoldoutGroup("Combat")] public SoundUnit m_oHitBuffProjectile;
    [FoldoutGroup("Status")] public SoundUnit m_oGeneralStatusBuff;
    [FoldoutGroup("Status")] public SoundUnit m_oGeneralStatusDebuff;
    [FoldoutGroup("Footsteps"), SerializeField] private FootstepSystemManager.FootstepEntitySounds[] m_aoFootstepsByEntity;
    [FoldoutGroup("Zone")] public SoundUnit m_oZoneFireSmallStart;
    [FoldoutGroup("Zone")] public SoundUnit m_oZoneFireSmallLoop;
    [FoldoutGroup("Zone")] public SoundUnit m_oZoneFireBigLoop;
    [FoldoutGroup("Zone")] public SoundUnit m_oZoneFireEnd;
    [FoldoutGroup("Zone")] public SoundUnit m_oZoneFireExtend;
    [FoldoutGroup("Other")] public SoundUnit m_oControllerChange;

    [NonSerialized] public FootstepSystemManager m_oFootstepSystem;

    private AudioSource m_oSceneMusic;
    private AudioSource m_oSceneAmbienceMusic;
    private AudioSource m_oWeatherAmbienceMusic;

    private Dictionary<string, AudioSource> m_aoBarksPlaying = new();

    private void OnEnable()
    {
        m_oFootstepSystem = new FootstepSystemManager(this, m_aoFootstepsByEntity);
    }

    private void OnDisable()
    {
        m_oFootstepSystem.dispose();
    }

    public void playWinSound()
    {
        playWinOrDefeat(true);
    }

    public void playDefeatSound()
    {
        playWinOrDefeat(false);
    }

    void playWinOrDefeat(bool _bWin)
    {
        fadeOutCurrentSceneMusic();
        if (_bWin)
        {
            play(m_oWinSong, false, false);
        }
        else
        {
            play(m_oDefeatSong, false, false);
        }
    }

    public void playMapSong()
    {
        play(m_oMapSong, true, true);
    }

    public void playMainMenuSong()
    {
        play(m_oMainMenuSong, true, true);
    }

    public void playCampfireAmbience()
    {
        m_oSceneAmbienceMusic = play(m_oCampfireSong, false, true);
    }

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

/// <summary>
/// Manages the entire footstep audio system including setup, lookups, and playback
/// </summary>
public class FootstepSystemManager
{
    [Serializable]
    public struct FootstepSurfaceSounds
    {
        public eFootStepType m_eSurface;
        public SoundUnit m_oSound;
    }

    [Serializable]
    public struct FootstepSizeSounds
    {
        public eFootStepSize m_eSize;
        public SoundUnit m_oDefaultFallbackSound;
        public FootstepSurfaceSounds[] m_aoSurfaceSounds;
    }

    [Serializable]
    public struct FootstepEntitySounds
    {
        public eFootStepEntity m_eEntity;
        public FootstepSurfaceSounds[] m_aoEntityFallbackSounds;
        public SoundUnit m_oDefaultFallbackSound;
        public FootstepSizeSounds[] m_aoSizeSounds;
    }

    private Dictionary<(eFootStepEntity, eFootStepSize, eFootStepType), SoundUnit> m_dFootstepLookup;
    private CompositeDisposable m_oSubscriptions;
    private FootstepEntitySounds[] m_aoFootstepsByEntity;

    public FootstepSystemManager(SoundManager _oSoundManager, FootstepEntitySounds[] _aoFootstepsByEntity)
    {
        m_aoFootstepsByEntity = _aoFootstepsByEntity;
        m_oSubscriptions = new CompositeDisposable();

        initialize();
    }

    private void initialize()
    {
        buildFootstepLookup();
        subscribeToEvents();
    }

    private void buildFootstepLookup()
    {
        m_dFootstepLookup = new Dictionary<(eFootStepEntity, eFootStepSize, eFootStepType), SoundUnit>();

        foreach (var entitySounds in m_aoFootstepsByEntity)
        {
            foreach (var sizeSounds in entitySounds.m_aoSizeSounds)
            {
                foreach (var surfaceSound in sizeSounds.m_aoSurfaceSounds)
                {
                    if (surfaceSound.m_oSound != null)
                    {
                        var key = (entitySounds.m_eEntity, sizeSounds.m_eSize, surfaceSound.m_eSurface);
                        m_dFootstepLookup[key] = surfaceSound.m_oSound;
                    }
                }
            }
        }
    }

    private void subscribeToEvents()
    {
        EventManager.Subscribe<FootstepSignal>(onFootstepStart).AddTo(m_oSubscriptions);
    }

    private void onFootstepStart(FootstepSignal _oSignal)
    {
        if (_oSignal.m_eEntity == eFootStepEntity.None || _oSignal.m_eSize == eFootStepSize.None)
        {
            return;
        }

        SoundUnit oSoundUnit = getFootstepSound(_oSignal.m_eEntity, _oSignal.m_eSize, _oSignal.m_eSurface);

        if (oSoundUnit == null)
        {
            Deb.logWarning($"[Footsteps] No sound found for {_oSignal.m_eEntity} ({_oSignal.m_eSize}) on {_oSignal.m_eSurface}");
            return;
        }

        oSoundUnit.play();
    }

    private SoundUnit getFootstepSound(eFootStepEntity _eEntity, eFootStepSize _eSize, eFootStepType _eSurface)
    {
        // 1. Try to get the specific entity + size + surface sound
        var key = (_eEntity, _eSize, _eSurface);
        if (m_dFootstepLookup.TryGetValue(key, out var soundUnit))
        {
            return soundUnit;
        }

        // 2. Look for entity + size combination
        foreach (var entitySounds in m_aoFootstepsByEntity)
        {
            if (entitySounds.m_eEntity == _eEntity)
            {
                foreach (var sizeSounds in entitySounds.m_aoSizeSounds)
                {
                    if (sizeSounds.m_eSize == _eSize)
                    {
                        // Try size default fallback
                        if (sizeSounds.m_oDefaultFallbackSound != null)
                        {
                            Deb.log($"[Footsteps] Using size ({_eSize}) default fallback for {_eEntity}", eLogFlags.FOOTSTEPS);
                            return sizeSounds.m_oDefaultFallbackSound;
                        }
                    }
                }

                // 3. Try entity-level fallback list for this surface
                SoundUnit entityFallbackSound = getFromFallbackList(entitySounds.m_aoEntityFallbackSounds, _eSurface);
                if (entityFallbackSound != null)
                {
                    Deb.log($"[Footsteps] Using entity ({_eEntity}) fallback sound for {_eSurface}", eLogFlags.FOOTSTEPS);
                    return entityFallbackSound;
                }

                // 4. Try entity default fallback
                if (entitySounds.m_oDefaultFallbackSound != null)
                {
                    Deb.log($"[Footsteps] Using entity ({_eEntity}) default fallback", eLogFlags.FOOTSTEPS);
                    return entitySounds.m_oDefaultFallbackSound;
                }
            }
        }
        return null;
    }

    private SoundUnit getFromFallbackList(FootstepSurfaceSounds[] _aoFallbackSounds, eFootStepType _eSurface)
    {
        if (_aoFallbackSounds == null || _aoFallbackSounds.Length == 0)
            return null;

        foreach (var fallback in _aoFallbackSounds)
        {
            if (fallback.m_eSurface == _eSurface && fallback.m_oSound != null)
            {
                return fallback.m_oSound;
            }
        }

        return null;
    }

    private float calculatePitchFromVelocity(float velocity, float baseVelocity)
    {
        float fMinPitch = 0.5f;
        float fMaxPitch = 2.0f;
        float fPitchRatio = velocity / baseVelocity;
        return Mathf.Clamp(fPitchRatio, fMinPitch, fMaxPitch);
    }

    public void dispose()
    {
        m_oSubscriptions?.Dispose();
    }
}