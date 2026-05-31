using UnityEngine;

[System.Serializable]
public class SetFadeToBlack : TimedEvent
{
    [SerializeField] bool m_bIsBehindUI;
    public override void play()
    {
        base.play();

        FadeManager.Instance.setToBlack(m_bIsBehindUI);
    }
}
