using UnityEngine;

public class AddToDisableListTimedEvent : TimedEvent
{
    [SerializeField] TagReference[] m_aoReference;

    public override void play()
    {
        base.play();

        for (int i = 0; i < m_aoReference.Length; i++)
        {
            GameplayManager.Instance.addGameobjectToDisable(m_aoReference[i].getGameobject());
        }
    }
}
