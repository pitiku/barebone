using Sirenix.OdinInspector;
using UnityEngine;

public class MoveObject : TimedEvent
{
    [HorizontalGroup("first", Width = 150, MarginLeft = 5), LabelText(""), LabelWidth(1)]
    public Transform m_oObject;
    [HorizontalGroup("first", Width = 175), LabelWidth(15), LabelText("->")]
    public Transform m_oTarget;

    public override void play()
    {
        base.play();

        // if it has animator, stop its execution
        m_oObject.copyDataFrom(m_oTarget);
    }
}
