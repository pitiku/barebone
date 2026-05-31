using UnityEngine;

public class PlayHOCEventsOnEndDialogue : TimedEvent
{
    [SerializeField] GameObject m_oGameObject;

    public override void play()
    {
        base.play();

        EventManager.Instance.StartListening(EventsType.EndDialogue, onEndDialogue);
    }

    void onEndDialogue()
    {
        EventManager.Instance.StopListening(EventsType.EndDialogue, onEndDialogue);
        m_oGameObject?.playHOCEvents();
    }
}
