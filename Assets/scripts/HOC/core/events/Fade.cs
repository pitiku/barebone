using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class Fade : TimedEvent
{
    [HorizontalGroup("first", Width = 60, MarginRight = 5), LabelText("to black"), LabelWidth(50)]
    public bool m_fadeToBlack;
    [HorizontalGroup("first", Width = 60, MarginRight = 5), LabelText("color"), LabelWidth(35)]
    public Color m_fadeColor = Color.black;
    [HorizontalGroup("first", Width = 105, MarginRight = 20), LabelText("duration"), LabelWidth(55)]
    public float m_fadeDuration = 0.3f;

    public bool m_bIsBehindUI = false;

    public override void play()
    {
        base.play();

        if (m_bIsBehindUI) { FadeManager.Instance.setCurrentBehindUI(); }
        else { FadeManager.Instance.setCurrentOverlay(); }

        FadeManager.Instance.Fade(m_fadeToBlack, m_fadeColor, m_fadeDuration);
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
