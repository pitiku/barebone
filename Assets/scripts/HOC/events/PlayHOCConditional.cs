using UnityEngine;

public class PlayHOCConditional : TimedEvent
{
    [SerializeReference] Condition m_oCondition;
    [SerializeField] GameObject m_oOnTrue;
    [SerializeField] GameObject m_oOnFalse;

    public override void play()
    {
        base.play();

        if (m_oCondition.isMet()) { m_oOnTrue.playHOCEvents(); }
        else { m_oOnFalse.playHOCEvents(); }
    }

}
