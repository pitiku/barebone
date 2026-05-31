using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class FadeInOut : TimedEvent
{
    [HorizontalGroup("first", Width = 60, MarginRight = 20), LabelText("color"), LabelWidth(35)]
    public Color m_fadeColor = Color.black;
    [HorizontalGroup("first", Width = 65, MarginRight = 10), LabelText("in"), LabelWidth(15)]
    public float m_fadeInDuration = 1.0f;
    [HorizontalGroup("first", Width = 75, MarginRight = 10), LabelText("out"), LabelWidth(25)]
    public float m_fadeOutDuration = 1.0f;

    public bool m_bIsBehindUI = false;

    public override void play()
    {
        base.play();

        if (m_bIsBehindUI) { FadeManager.Instance.setCurrentBehindUI(); }
        else { FadeManager.Instance.setCurrentOverlay(); }

        FadeManager.Instance.fadeInOut(m_fadeInDuration, m_fadeOutDuration, m_fadeColor);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
