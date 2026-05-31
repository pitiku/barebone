using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlayHOCEventsOnEvent : MonoBehaviour
{
    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getCategories")]
    public string m_sPredefinedEvent;

    [SerializeField] GameObject m_oEvents;


    private void OnEnable()
    {
        EventManager.Instance.StartListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playHOCEvents);
    }

    private void OnDisable()
    {
        if (EventManager.isNull()) { return; }

        EventManager.Instance.StopListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playHOCEvents);
    }

    void playHOCEvents()
    {
        m_oEvents.playHOCEvents();
    }

    IEnumerable<string> getEvents() { return new List<string>(EventsType.Events); }
    private bool isCustomSelected() { return m_sPredefinedEvent == "Custom"; }
}
