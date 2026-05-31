[System.Serializable]
public class StopSound : TimedEvent
{
    public SoundEvent m_oSound;

    public override void play()
    {
        base.play();

        if (m_oSound.m_oSource != null)
        {
            AudioManager.Instance.fadeOutAndStop(m_oSound.m_oSource);
        }
    }
}
