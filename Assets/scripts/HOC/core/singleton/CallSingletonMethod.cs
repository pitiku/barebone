
public class CallSingletonMethod : TimedEvent
{
    public HOCSingletonCall m_oSingletonCall;

    public override void initialize()
    {
        base.initialize();

        m_oSingletonCall.initialize();
    }

    public override void play()
    {
        base.play();

        m_oSingletonCall.call();
    }
}
