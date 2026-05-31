using Sirenix.OdinInspector;
using UnityEngine;

public class PlayHOCEventsTimedEvent : TimedEvent
{
    enum eState { Tag, Reference }

    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;

    [HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText(""), ShowIf("@m_eType == eState.Reference")]
    [SerializeField] GameObject m_oParent;

    public override void play()
    {
        base.play();

        if (m_oParent == null && m_eType == eState.Tag)
        {
            m_oParent = GameplayManager.Instance.getGameobject(m_oTag.Value);
        }

        m_oParent.playHOCEvents();
    }
}
