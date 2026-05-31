[System.Serializable]
public class PlaySongByName : SoundEvent
{
    public string m_sClip;
    public float m_fVolume = 1.0f;

    public override void play()
    {
        base.play();

        m_oSource = AudioManager.Instance.playSongByName(m_sClip, m_fVolume);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
