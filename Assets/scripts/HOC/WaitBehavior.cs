using UnityEngine;

public class WaitBehavior : Behavior
{
    [SerializeField] protected float m_fDuration;
    [SerializeField] eType m_eType;

    Timer m_oTimer;

    public override void activate()
    {
        base.activate();

        m_oTimer = new Timer(m_fDuration, m_eType);
        m_oTimer.start();
    }

    public override void update()
    {
        if (m_oTimer.isFinished()) { m_bFinished = true; }
    }
}
