
public class ActivateMachine : TimedEvent
{
    public HOCComponentStateMachine m_oMachine;

    public override void play()
    {
        base.play();

        m_oMachine.get().activate();
    }
}
