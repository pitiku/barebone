using Sirenix.OdinInspector;
using UnityEngine;

public class PlayEvents : TimedEvent
{
    [HorizontalGroup("first", Width = 260, MarginRight = 8), LabelWidth(55), LabelText("container")]
    public GameObject m_o;
    [HorizontalGroup("first", Width = 250, MarginRight = 8), LabelWidth(48), LabelText("children")]
    public bool m_bIncludeChildren = false;

    public override void play()
    {
        base.play();
        m_o.playHOCEvents(m_bIncludeChildren);
    }
}
