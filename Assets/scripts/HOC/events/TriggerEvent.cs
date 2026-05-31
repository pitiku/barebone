using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class TriggerEvent : TimedEvent
{
    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getEvents")]
    public string m_sPredefinedEvent = "Custom";
    public override void play()
    {
        base.play();

        EventManager.Instance.TriggerEvent(m_sPredefinedEvent == "Custom" ? m_sEvent : m_sPredefinedEvent);
    }

    IEnumerable<string> getEvents() { return new List<string>(EventsType.Events); }
    private bool isCustomSelected() { return m_sPredefinedEvent == "Custom"; }
}
