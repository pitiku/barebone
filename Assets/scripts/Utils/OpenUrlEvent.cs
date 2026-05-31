using UnityEngine;

public class OpenUrlEvent : TimedEvent
{
    public string m_sUrl;

    public override void play()
    {
        base.play();
        Application.OpenURL(m_sUrl);
    }
}
