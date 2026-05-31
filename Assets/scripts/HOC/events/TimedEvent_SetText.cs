using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class TimedEvent_SetText : TimedEvent
{
    enum eDisplay { NonLocalized, Localized }

    [SerializeField] TMPro.TextMeshProUGUI m_oTextTMP;
    [SerializeField] string m_sText;

    [SerializeField] eDisplay m_eDisplay;
    [SerializeField, ShowIf("@m_eDisplay == eDisplay.Localized")] string m_sCategory;

    public override void play()
    {
        base.play();

        string sText = m_eDisplay == eDisplay.NonLocalized ? m_sText : Utils.getTranslation(m_sText, m_sCategory);
        m_oTextTMP.text = sText;
    }
}
