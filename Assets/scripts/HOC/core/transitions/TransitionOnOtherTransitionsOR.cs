public class TransitionOnOtherTransitionsOR : StateTransition
{
    public StateTransition[] m_aoOthers;

    public override bool update()
    {
        for (int i = 0; i < m_aoOthers.Length; ++i)
        {
            if (m_aoOthers[i].update())
            {
                return true;
            }
        }
        return false;
    }
}
