using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalTransitionOnEvent : StateTransition
{
    [ShowIf("@isCustomSelected()")]
    public string m_sEvent = "";

    [ValueDropdown("getCategories")]
    public string m_sPredefinedEvent = "Custom";

    private bool m_bEventActivated = false;

    [SerializeReference] Condition m_oCondition;

    [SerializeField] State m_oOnTrueState;
    [SerializeField] State m_oOnFalseState;

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
        if (m_oCondition.isMet()) { m_oTargetState = m_oOnTrueState; }
        else { m_oTargetState = m_oOnFalseState; }

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
