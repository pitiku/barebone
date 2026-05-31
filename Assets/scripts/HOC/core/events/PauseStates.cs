
[System.Serializable]
public class PauseStates : TimedEvent
{
    public bool m_pause = true;
    public State[] m_states;

    public override void play()
    {
        base.play();

        for (int index = 0; index < m_states.Length; ++index)
        {
            m_states[index].m_bPaused = m_pause;
        }
    }
}
