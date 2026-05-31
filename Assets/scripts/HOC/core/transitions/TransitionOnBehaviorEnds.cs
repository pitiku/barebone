public class TransitionOnBehaviorEnds : StateTransition
{
    public Behavior m_oBehavior;

    public override bool update()
    {
        return m_oBehavior.m_bFinished;
    }
}
