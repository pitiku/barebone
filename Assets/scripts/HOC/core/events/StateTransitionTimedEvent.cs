using UnityEngine;

public class StateTransitionTimedEvent : TimedEvent
{
    [SerializeField] State m_oTransitionTo;

    public override void play()
    {
        base.play();
        m_oTransitionTo.changeToMe();
    }
}
