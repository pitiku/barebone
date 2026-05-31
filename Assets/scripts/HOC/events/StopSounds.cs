[System.Serializable]
public class StopSounds : TimedEvent
{
    public SoundEvent[] m_aoSounds;

    public override void play()
    {
        base.play();

        foreach (var oSound in m_aoSounds)
        {
            if (oSound != null && oSound.m_oSource != null)
            {
                AudioManager.Instance.fadeOutAndStop(oSound.m_oSource);
            }
        }
    }
}