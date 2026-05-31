using UnityEngine;

public class ChangeStateDelayed : Behavior
{
    bool m_bChange = false;
    float m_fTimer = 0.0f;
    float m_fTime = float.MaxValue;
    State m_oTarget;
    State m_oChanger;

    public void setup(float _fTime, State _oTarget, State _oChanger = null)
    {
        m_fTime = _fTime;
        m_oTarget = _oTarget;
        m_oChanger = _oChanger;

        // check if somehow the change is being scheduled for now, execute it immediately
        if (_fTime <= 0.0f)
        {
            executeChange();
            return;
        }

        m_bChange = true;
        m_fTimer = 0.0f;
    }

    public void clear()
    {
        m_bChange = false;
    }

    public override void update()
    {
        base.update();

        if (m_bChange)
        {
            m_fTimer += Time.deltaTime;

            if (m_fTimer > m_fTime)
            {
                executeChange();
            }
        }
    }

    void executeChange()
    {
        m_oState.m_oStateMachine.changeState(m_oTarget, m_oChanger);
        m_bChange = false;
    }
}
