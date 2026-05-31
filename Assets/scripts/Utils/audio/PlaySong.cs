using UnityEngine;

[System.Serializable]
public class PlaySong : SoundEvent
{
    public AudioClip m_oClip;
    public float m_fVolume = 1.0f;
    public bool m_bLoops = true;

    public override void play()
    {
        base.play();

        if (m_oClip != null)
        {
            m_oSource = AudioManager.Instance.playSong(m_oClip, m_fVolume, m_bLoops);
        }
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
