using UnityEngine;

public class SetObjectAt : TimedEvent
{
    public GameObject m_oObject;
    public Transform m_oTransform;
    public Vector3 m_vPosition;

    public override void play()
    {
        base.play();

        if (m_oTransform != null)
        {
            m_oObject.transform.position = m_oTransform.position;
        }
        else
        {
            m_oObject.transform.position = m_vPosition;
        }
    }
}
