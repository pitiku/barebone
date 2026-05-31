using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayUnityEventOnEvent : MonoBehaviour
{
    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getCategories")]
    public string m_sPredefinedEvent;

    [SerializeField] UnityEvent m_oUnityEvent;

    private void OnEnable()
    {
        EventManager.Instance.StartListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playUnityEvents);
    }

    private void OnDisable()
    {
        if (EventManager.isNull()) { return; }

        EventManager.Instance.StopListening(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent, playUnityEvents);
    }

    void playUnityEvents()
    {
        m_oUnityEvent.Invoke();
    }

    IEnumerable<string> getEvents() { return new List<string>(EventsType.Events); }
    private bool isCustomSelected() { return m_sPredefinedEvent == "Custom"; }
}
