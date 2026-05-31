using Sirenix.OdinInspector;
using UnityEngine;

public class TransitionTimed : StateTransition
{
    [HorizontalGroup("first", Width = 100, MarginRight = 5), LabelText("time"), LabelWidth(39)]
    public float m_fTime;

    float m_fTimeLeft;

    public override void activate()
    {
        m_fTimeLeft = m_fTime;
    }

    public void setTime(float _fTime)
    {
        m_fTime = _fTime;
        activate();
    }

    public override bool update()
    {
        m_fTimeLeft -= Time.deltaTime;
        return m_fTimeLeft <= 0.0f;
    }

    public float getProgress()
    {
        if (m_fTime == 0.0f)
        {
            return 1.0f;
        }

        return 1.0f - (m_fTimeLeft / m_fTime);
    }
}
