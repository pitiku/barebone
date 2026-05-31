using Sirenix.OdinInspector;
using UnityEngine;

public class HierarchyIconConfig
{
    [HorizontalGroup("first"), LabelWidth(40), LabelText("type")]
    public string m_sType;
    [HorizontalGroup("first"), LabelWidth(45), LabelText("right")]
    public bool m_bRightOrLeft = false;

    [HorizontalGroup("first"), LabelWidth(50), LabelText("texture")]
    public Texture2D m_oIcon;
}
