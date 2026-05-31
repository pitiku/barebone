using Sirenix.OdinInspector;

public class ModifySound : SoundEvent
{
    [HorizontalGroup("first", Width = 250), LabelWidth(45), LabelText("audio")]
    public SoundEvent m_oSound;

    [HorizontalGroup("first", Width = 100), LabelWidth(45), LabelText("volume")]
    public float m_fVolume = 1.0f;

    public override void play()
    {
        base.play();

        if (m_oSound.m_oSource != null)
        {
            m_oSound.m_oSource.volume = m_fVolume;
        }
    }
}
