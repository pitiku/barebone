using System.Collections.Generic;

public class RemoveDelayedEvents : TimedEvent
{
    public List<TimedEvent> m_aoEvents;

    public override void play()
    {
        base.play();

        for (int i = 0; i < m_aoEvents.Count; ++i)
        {

            if (m_aoEvents[i].m_bStartOrEnd && m_aoEvents[i].m_fTime > 0.0f)
            {
                GameStateManager.Instance.removeDelayedEvent(m_aoEvents[i]);
            }
        }
    }
}
