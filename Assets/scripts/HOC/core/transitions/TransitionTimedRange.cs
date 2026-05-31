using UnityEngine;

public class TransitionTimedRange : StateTransition
{
    public Vector2 m_vRange;
    float m_fTimeLeft;
    float m_fTotalTime;

    public override void activate()
    {
        m_fTotalTime = m_vRange.range(null, "");
        m_fTimeLeft = m_fTotalTime;
    }

    public override bool update()
    {
        m_fTimeLeft -= Time.deltaTime;
        return m_fTimeLeft <= 0.0f;
    }

    public float getProgress()
    {
        if (m_fTotalTime == 0.0f)
        {
            return 1.0f;
        }

        return 1.0f - (m_fTimeLeft / m_fTotalTime);
    }
}
