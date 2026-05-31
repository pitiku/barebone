public class TransitionOnSingletonMethod : StateTransition
{
    public HOCSingletonBoolCall m_oSingletonCall;
    public bool m_bResult;

    public override void activate()
    {
        base.activate();

        m_oSingletonCall.initialize();
    }

    public override bool update()
    {
        return m_oSingletonCall.call() == m_bResult;
    }
}
