using UnityEngine;

[System.Serializable]
public class DebugTrigger : TimedEvent
{
    public bool m_bPauseEditor;

    public override void play()
    {
        base.play();

#if UNITY_EDITOR
        if (m_bPauseEditor)
        {
            Debug.Break();
        }
#endif

    }
}
