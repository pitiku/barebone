public class EventCondition : Condition
{
    public string m_sEvent;

    bool m_bEventDone = false;
    bool m_bListenerAdded = false;

    public override bool isMet(ICondition iC = null)
    {
        if (!m_bListenerAdded)
        {
            EventManager.Instance.StartListening(m_sEvent, onEvent);
            m_bListenerAdded = true;
        }

        if (m_bEventDone)
        {
            EventManager.Instance.StopListening(m_sEvent, onEvent);
            m_bListenerAdded = false;
        }

        bool bEventDone = m_bEventDone;
        m_bEventDone = false;
        return bEventDone;
    }

    void onEvent()
    {
        m_bEventDone = true;
    }

    public override void reset()
    {
        base.reset();

        m_bListenerAdded = false;
    }

    public override object Clone()
    {
        var clone = new EventCondition();
        clone.m_sEvent = m_sEvent;
        clone.m_bEventDone = false;
        clone.m_bListenerAdded = false;
        return clone;
    }
}
