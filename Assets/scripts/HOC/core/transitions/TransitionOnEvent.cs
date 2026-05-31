
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class TransitionOnEvent : StateTransition
{
    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getEvents")]
    public string m_sPredefinedEvent = "Custom";

    private bool m_bEventActivated = false;

    public override void activate()
    {
        base.activate();

        m_bEventActivated = false;
        EventManager.Instance.StartListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, OnEvent);
    }

    public override bool update()
    {
        return m_bEventActivated;
    }

    public void OnEvent()
    {
        m_bEventActivated = true;
    }

    public override void deactivate()
    {
        base.deactivate();

        EventManager.Instance.StopListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, OnEvent);
    }

    IEnumerable<string> getEvents() { return new List<string>(EventsType.Events); }
    private bool isCustomSelected() { return m_sPredefinedEvent == "Custom"; }
}
