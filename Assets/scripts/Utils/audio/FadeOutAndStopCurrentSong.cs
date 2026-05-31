[System.Serializable]
public class FadeOutAndStopCurrentSong : TimedEvent
{
    public float m_fFadeTime = 1.0f;

    public override void play()
    {
        base.play();

        AudioManager.Instance.fadeOutAndStopCurrentSong(m_fFadeTime);
    }
}
