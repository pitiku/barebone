using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public class HierarchyStyleConfig
{
    [HorizontalGroup("first"), LabelWidth(40), LabelText("type")]
    public string m_sType;
    [HorizontalGroup("first"), LabelWidth(70), LabelText("background")]
    public Color m_oBackground = new Color(0.22f, 0.22f, 0.22f, 1.0f);

    [HorizontalGroup("second"), LabelWidth(70), LabelText("enabled")]
    public Color m_oEnabled = new Color(0.78f, 0.78f, 0.78f, 1.0f);
    [HorizontalGroup("second"), LabelWidth(70), LabelText("disabled")]
    public Color m_oDisabled = new Color(0.22f, 0.22f, 0.22f, 1.0f);

    [HorizontalGroup("third"), LabelWidth(40), LabelText("bold")]
    public bool m_bBold = false;
    [HorizontalGroup("third"), LabelWidth(40), LabelText("italic")]
    public bool m_bItalic = false;

    public FontStyle getFontStyle()
    {
        if (m_bBold && m_bItalic)
        {
            return FontStyle.BoldAndItalic;
        }

        if (m_bBold)
        {
            return FontStyle.Bold;
        }

        if (m_bItalic)
        {
            return FontStyle.Italic;
        }

        return FontStyle.Normal;
    }
}
