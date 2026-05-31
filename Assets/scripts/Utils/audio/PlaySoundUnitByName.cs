[System.Serializable]
public class PlaySoundUnitByName : SoundEvent
{
    public string m_sUnit;
    public float m_fVolume = 1.0f;
    public bool m_bLoops = false;

    public override void play()
    {
        base.play();

        m_oSource = AudioManager.Instance.playSoundUnitByName(m_sUnit, m_fVolume, m_bLoops);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
