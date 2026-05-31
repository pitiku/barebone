using System.Collections.Generic;

public class PlayOtherEvents : TimedEvent
{
    public List<TimedEvent> m_aoEvents;

    public override void play()
    {
        base.play();

        for (int i = 0; i < m_aoEvents.Count; ++i)
        {
            m_aoEvents[i].play();
        }
    }
}
