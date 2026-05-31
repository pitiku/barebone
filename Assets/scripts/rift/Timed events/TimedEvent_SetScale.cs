using UnityEngine;

public class TimedEvent_SetScale : TimedEvent
{
    public enum ScaleType { Local, Global }

    [SerializeField, Range(0, 10)] float m_fValue;
    [SerializeField] Transform m_oTransform;
    [SerializeField] ScaleType m_eScaleType;

    public override void play()
    {
        base.play();

        Vector3 vScale = m_fValue * Vector3.one;

        if (m_eScaleType == ScaleType.Local)
        {
            m_oTransform.localScale = vScale;
        }
        else
        {
            m_oTransform.setGlobalScale(vScale);
        }
    }
}
