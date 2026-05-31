using UnityEngine;

public class TransitionToConditionedTimedEvent : TimedEvent
{
    [SerializeReference] Condition m_oCondition;

    [SerializeField] State m_oTransitionToIfTrue;
    [SerializeField] State m_oTransitionToIfFalse;

    public override void play()
    {
        base.play();

        if (m_oCondition.isMet())
        {
            if (m_oTransitionToIfTrue != null) { m_oTransitionToIfTrue.changeToMe(); }
        }
        else if (m_oTransitionToIfFalse != null) { m_oTransitionToIfFalse.changeToMe(); }
    }
}
