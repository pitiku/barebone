using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class PlaySoundUnit : SoundEvent
{
    public SoundUnit m_oUnit;
    public float m_fVolume = 1.0f;
    public bool m_bLoops = false;
    public AudioMixerGroup m_oMixerGroup;
    [SerializeField] bool m_bAdaptPitchToTimeScale = true;
    [SerializeField] bool m_bFadeIn = false;

    private void Awake()
    {
        if (m_oMixerGroup == null)
        {
            m_oMixerGroup = AudioManager.Instance.m_oFXGroup;
        }
    }

    public override void play()
    {
        base.play();

        if (m_oUnit == null)
        {
            Deb.logWarning("No SoundUnit on: " + gameObject.getParentNames());
            return;
        }

        m_oSource = m_oUnit.play(m_fVolume, m_bLoops, m_oMixerGroup, m_bAdaptPitchToTimeScale, m_bFadeIn);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
