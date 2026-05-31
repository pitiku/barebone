public class FadeEndNode : TimedEvent
{
    public float m_fFadeTime = .5f;
    public override void play()
    {
        base.play();
        SoundManager.Instance.fadeCurrentSongAndAmbience(m_fFadeTime);
        FadeManager.Instance.FadeToBlack(m_fFadeTime);
    }
}