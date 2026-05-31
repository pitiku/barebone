public class SequentialBehavior : Behavior
{
    public State m_oNextState;

    protected void toNextState()
    {
        if (m_oNextState != null)
        {
            m_oNextState.changeToMe();
        }

        m_bFinished = true;
    }
}
