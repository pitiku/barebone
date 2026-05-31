using Sirenix.OdinInspector;
using UnityEngine;

public class AttachToTransform : TimedEvent
{
    [HorizontalGroup("first", Width = 150, MarginLeft = 5), LabelText(""), LabelWidth(1)]
    public Transform m_oTarget;
    [HorizontalGroup("first", Width = 175), LabelWidth(15), LabelText("->")]
    public Transform m_oParent;

    public override void play()
    {
        base.play();

        m_oTarget.parent = m_oParent;
    }
}
