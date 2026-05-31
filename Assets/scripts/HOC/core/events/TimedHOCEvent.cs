
using UnityEngine;

public class TimedHOCEvent : TimedEvent
{
    public HOCComponent<Component> m_oHOCComponent;
    public string m_sMethod;
    public object[] m_aParams;

    public override void play()
    {
        base.play();

        m_oHOCComponent.get().GetType().GetMethod(m_sMethod).Invoke(m_oHOCComponent.get(), m_aParams);
    }
}
