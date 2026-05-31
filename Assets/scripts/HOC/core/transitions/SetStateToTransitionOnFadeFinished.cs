using UnityEngine;

public class SetStateToTransitionOnFadeFinished : TimedEvent
{
    [SerializeField] StateTransition m_oTransition;
    [SerializeField] State m_oTransitionTo;

    public override void play()
    {
        base.play();

        m_oTransition.m_oTargetState = m_oTransitionTo;
    }
}
