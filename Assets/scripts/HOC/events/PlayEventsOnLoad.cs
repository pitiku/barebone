using UnityEngine;

public class PlayEventsOnLoad : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.playHOCEvents();
    }
}
