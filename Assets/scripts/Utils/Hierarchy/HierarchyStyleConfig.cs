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

    private FontStyle m_eCachedFontStyle = FontStyle.Normal;
    private bool m_bFontStyleDirty = true;

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize() { m_bFontStyleDirty = true; }

    public FontStyle getFontStyle()
    {
        if (m_bFontStyleDirty)
        {
            if (m_bBold && m_bItalic)
            {
                m_eCachedFontStyle = FontStyle.BoldAndItalic;
            }
            else if (m_bBold)
            {
                m_eCachedFontStyle = FontStyle.Bold;
            }
            else if (m_bItalic)
            {
                m_eCachedFontStyle = FontStyle.Italic;
            }
            else
            {
                m_eCachedFontStyle = FontStyle.Normal;
            }
            m_bFontStyleDirty = false;
        }

        return m_eCachedFontStyle;
    }
}
