using System;

public class PlaySoundUnitOneTime : PlaySoundUnit
{
    [NonSerialized] public bool m_bPlaySFX = true;

    public override void play()
    {
        if (!m_bPlaySFX)
        {
            return;
        }

        base.play();
        m_bPlaySFX = false;
    }
}
