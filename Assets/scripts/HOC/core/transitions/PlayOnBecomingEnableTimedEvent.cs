using UnityEngine;

public class PlayOnBecomingEnableTimedEvent : MonoBehaviour
{
    [SerializeField] TimedEvent m_oEvent;

    private void OnEnable()
    {
        m_oEvent.play();
    }

}
