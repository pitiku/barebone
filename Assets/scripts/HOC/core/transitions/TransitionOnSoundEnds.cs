using UnityEngine;

public class TransitionOnSoundEnds : StateTransition
{
    public SoundEvent m_oSoundEvent;
    public float m_fAddedTime = 0.3f;
    float m_fTimer;
    bool m_bStartedPlaying;

    public override void activate()
    {
        base.activate();

        m_fTimer = m_fAddedTime;
        m_bStartedPlaying = false;
    }

    public override bool update()
    {
        if (m_bStartedPlaying || m_oSoundEvent.m_oSource != null)
        {
            m_bStartedPlaying = true;
            if (m_oSoundEvent.m_oSource == null || !m_oSoundEvent.m_oSource.isPlaying)
            {
                m_fTimer -= Time.deltaTime;
                if (m_fTimer < 0.0f)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
