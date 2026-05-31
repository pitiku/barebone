public class TransitionOnOtherTransitions : StateTransition
{
    public StateTransition[] m_aoOthers;

    public override void activate()
    {
        for (int i = 0; i < m_aoOthers.Length; ++i)
        {
            m_aoOthers[i].activate();
        }
    }

    public override bool update()
    {
        for (int i = 0; i < m_aoOthers.Length; ++i)
        {
            if (!m_aoOthers[i].update())
            {
                return false;
            }
        }
        return true;
    }
}
