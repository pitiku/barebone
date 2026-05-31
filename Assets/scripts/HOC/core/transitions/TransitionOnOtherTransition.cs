public class TransitionOnOtherTransition : StateTransition
{
    public StateTransition m_oOther;

    public override bool update()
    {
        return m_oOther.update();
    }
}
