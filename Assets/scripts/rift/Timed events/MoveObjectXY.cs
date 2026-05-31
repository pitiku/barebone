using Sirenix.OdinInspector;
using UnityEngine;

public class MoveObjectXY : TimedEvent
{
    [HorizontalGroup("first", Width = 175), LabelWidth(30), LabelText("from")]
    public Transform m_oObject;
    [HorizontalGroup("first", Width = 175), LabelWidth(15), LabelText("to")]
    public Transform m_oTarget;

    public override void play()
    {
        base.play();

        // if it has animator, stop its execution
        float fZ = m_oTarget.position.z;
        m_oObject.copyDataFrom(m_oTarget);
        m_oObject.setZ(fZ);
    }
}
