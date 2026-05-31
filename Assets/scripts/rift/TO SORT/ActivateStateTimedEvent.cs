using UnityEngine;

public class ActivateStateTimedEvent : TimedEvent
{
    [SerializeField] private State m_oStateReference;
    [SerializeField] private bool m_bActivate;

    public override void play()
    {
        base.play();
        activateState();
    }

    public virtual void activateState()
    {
        if (m_oStateReference != null)
        {
            if (m_bActivate)
            {
                m_oStateReference.activate();
            }
            else
            {
                m_oStateReference.deactivate();
            }
        }
    }
}
