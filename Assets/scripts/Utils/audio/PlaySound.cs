using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class PlaySound : SoundEvent
{
    [HorizontalGroup("first", Width = 250), LabelWidth(45), LabelText("audio")]
    public AudioClip m_oAudio;

    [HorizontalGroup("second", Width = 100), LabelWidth(45), LabelText("volume")]
    public float m_fVolume = 1.0f;
    [HorizontalGroup("second", Width = 100), LabelWidth(45), LabelText("delay")]
    public Vector2 m_vDelay = new Vector2(0f, 0f);
    [HorizontalGroup("second", Width = 100), LabelWidth(45), LabelText("loops")]
    public bool m_bLoops = false;
    [HorizontalGroup("second", Width = 250), LabelWidth(45), LabelText("mixer")]
    public AudioMixerGroup m_oMixerGroup;

    public override void play()
    {
        base.play();

        if (m_oAudio == null)
        {
            return;
        }

        m_oSource = AudioManager.Instance.playAudioClip(m_oAudio, m_fVolume, m_bLoops, 1.0f, m_vDelay.range(null, ""), GameStateManager.APPLICATION_TIMER, m_oMixerGroup);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
