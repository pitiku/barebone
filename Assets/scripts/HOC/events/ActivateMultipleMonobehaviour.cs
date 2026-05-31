using UnityEngine;

public class ActivateMultipleMonobehaviour : TimedEvent
{
    public bool m_bActive = true;
    public MonoBehaviour[] m_aoComponent;

    public override void play()
    {
        base.play();

        foreach (MonoBehaviour itMonobehaviour in m_aoComponent)
        {
            itMonobehaviour.enabled = m_bActive;
        }
    }
}
