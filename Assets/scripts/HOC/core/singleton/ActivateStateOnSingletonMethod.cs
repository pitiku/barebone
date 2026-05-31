public class ActivateStateOnSingletonMethod : ActivateStateTimedEvent
{
    public HOCSingletonBoolCall m_oSingletonCall;
    public bool m_bResult;

    public override void activateState()
    {
        m_oSingletonCall.initialize();
        if (m_oSingletonCall.call() == m_bResult)
        {
            base.activateState();
        }
    }
}
