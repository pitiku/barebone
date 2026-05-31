using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SoundUnit : MonoBehaviour
{
    public SoundUnit m_oStackedUnit;

    [MaxValue(1)] public Vector2 m_vVolumeVariance = new Vector2(0.7f, 0.9f);
    public Vector2 m_vPitchVariance = new Vector2(0.9f, 1.1f);
    public Vector2 m_vDelay = new Vector2(0f, 0f);

    public const string PREVIEW_NAME = "preview sound unit - ";

    [ListDrawerSettings(ShowFoldout = true)] public List<ProbabilityPair<AudioClip>> m_aoClips = new List<ProbabilityPair<AudioClip>>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        calculateElementProbabilities();
    }

    void calculateElementProbabilities()
    {
        m_aoClips.normalizeProbabilities();
    }

    [HorizontalGroup("buttons")]
    [Button("preview")]
    public void preview()
    {
        AudioSource oAS = new GameObject().AddComponent<AudioSource>();
        oAS.name = PREVIEW_NAME + oAS.name;
        oAS.volume = m_vVolumeVariance.range(null, "");
        oAS.pitch = m_vPitchVariance.range(null, "");
        oAS.clip = m_aoClips.getRandom();
        oAS.Play();
    }

    [HorizontalGroup("buttons")]
    [Button("clean preview objects")]
    public void cleanPreviews()
    {
        AudioSource[] aoSources = FindObjectsByType<AudioSource>(0);
        for (int i = 0; i < aoSources.Length; ++i)
        {
            if (aoSources[i].name.StartsWith(PREVIEW_NAME))
            {
                DestroyImmediate(aoSources[i].gameObject);
            }
        }
    }

    [HorizontalGroup("buttons")]
    [Button("add selected clips")]
    public void addSelectedAudioClips()
    {
        Object[] aoSelected = Selection.GetFiltered(typeof(AudioClip), SelectionMode.Assets);

        if (aoSelected.Length == 0)
        {
            Debug.LogWarning("No audio clips selected. Please select audio clips in the Project window.");
            return;
        }

        int iAddedCount = 0;
        foreach (Object oSelected in aoSelected)
        {
            AudioClip oClip = oSelected as AudioClip;
            if (oClip != null)
            {
                // Check if clip already exists in the list
                bool bAlreadyExists = false;
                foreach (ProbabilityPair<AudioClip> oPair in m_aoClips)
                {
                    if (oPair.value == oClip)
                    {
                        bAlreadyExists = true;
                        break;
                    }
                }

                if (!bAlreadyExists)
                {
                    m_aoClips.Add(new ProbabilityPair<AudioClip>(oClip, 1));
                    iAddedCount++;
                }
            }
        }

        if (iAddedCount > 0)
        {
            calculateElementProbabilities();
            EditorUtility.SetDirty(this);
            Debug.Log($"Added {iAddedCount} audio clip(s) to {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No new audio clips were added (duplicates skipped).");
        }
    }
#endif

    public AudioSource play(float _fVolume = 1.0f, bool _bLoops = false, AudioMixerGroup _oMixer = null, bool m_bAdaptPitchToTimeScale = true, bool _bFadeIn = false)
    {
        if (_oMixer == null)
        {
            _oMixer = AudioManager.Instance.m_oFXGroup;
        }

        if (m_oStackedUnit != null)
        {
            m_oStackedUnit.play(_fVolume, _bLoops, _oMixer);
        }

        bool bFound = !m_aoClips.isNullOrEmpty();
        for (int i = 0; i < m_aoClips.Count; i++)
        {
            if (m_aoClips[i] == null || m_aoClips[i].value == null)
            {
                bFound = false;
                break;
            }
        }

        if (bFound)
        {
            AudioSource oSource = AudioManager.Instance.playSoundUnit(this, _fVolume, _bLoops, 0, m_bAdaptPitchToTimeScale, _bFadeIn);
            if (oSource != null)
            {
                oSource.outputAudioMixerGroup = _oMixer;
            }
            return oSource;
        }
        else
        {
            Deb.logWarning("Audio clips null on: " + gameObject.getParentNames());
        }
        return null;
    }

    public AudioSource playWithSpeed(float _fPitch = 1, bool _bLoops = false, AudioMixerGroup _oMixer = null)
    {
        if (_oMixer == null)
        {
            _oMixer = AudioManager.Instance.m_oFXGroup;
        }

        if (m_oStackedUnit != null)
        {
            m_oStackedUnit.play(1, _bLoops, _oMixer);
        }

        bool bFound = !m_aoClips.isNullOrEmpty();
        for (int i = 0; i < m_aoClips.Count; i++)
        {
            if (m_aoClips[i] == null || m_aoClips[i].value == null)
            {
                bFound = false;
                break;
            }
        }

        if (bFound)
        {
            AudioSource oSource = AudioManager.Instance.playAudioClip(m_aoClips.getRandom(), 1 * m_vVolumeVariance.range(null, ""), _bLoops, _fPitch * m_vPitchVariance.range(null, ""), m_vDelay.range(null, ""), GameStateManager.APPLICATION_TIMER, null, false);
            if (oSource != null)
            {
                oSource.outputAudioMixerGroup = _oMixer;
            }
            return oSource;
        }
        else
        {
            Deb.logWarning("Audio clips null on: " + gameObject.getParentNames());
        }
        return null;
    }
}