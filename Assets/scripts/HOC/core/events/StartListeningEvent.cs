using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class StartListeningEvent : TimedEvent
{
    [SerializeField] TimedEvent m_oEvent;

    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getCategories")]
    public string m_sPredefinedEvent = "Custom";

    public override void play()
    {
        base.play();
        EventManager.Instance.StartListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playEvent);
    }

    private void OnDisable()
    {
        if (EventManager.isNull()) { return; }

        EventManager.Instance.StopListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playEvent);
    }

    void playEvent()
    {
        m_oEvent.play();
        EventManager.Instance.StopListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playEvent);
    }

    IEnumerable<string> getEvents() { return new List<string>(EventsType.Events); }
    private bool isCustomSelected() { return m_sPredefinedEvent == "Custom"; }
}
