using UnityEngine;

[System.Serializable]
public class ActivateMonobehaviour : TimedEvent
{
    public bool m_bActive = true;
    public MonoBehaviour m_oComponent;

    public override void play()
    {
        base.play();

        m_oComponent.enabled = m_bActive;
    }
}
