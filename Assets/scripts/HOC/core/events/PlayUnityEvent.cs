using UnityEngine.Events;

[System.Serializable]
public class PlayUnityEvent : TimedEvent
{
    public UnityEvent m_oEvent;

    public override void play()
    {
        base.play();

        m_oEvent.Invoke();
    }
}
