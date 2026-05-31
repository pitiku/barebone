using UnityEngine;

//Event to activate/deactivate a collider (ie.: can be used to disable buttons interactavility)
[System.Serializable]
public class ActivateCollider : TimedEvent
{
    public bool m_bActive = true;
    public Collider m_oCollider;

    public override void play()
    {
        base.play();

        m_oCollider.enabled = m_bActive;
    }
}
