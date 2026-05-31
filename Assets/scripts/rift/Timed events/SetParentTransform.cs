using UnityEngine;

public class SetParentTransform : TimedEvent
{
    public Transform m_oChildTransform;
    public Transform m_oParentTransform;

    public override void play()
    {
        base.play();

        m_oChildTransform.SetParent(m_oChildTransform);
    }
}
