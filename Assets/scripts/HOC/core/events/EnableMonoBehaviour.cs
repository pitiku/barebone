using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EnableMonoBehaviour : TimedEvent
{
    [HorizontalGroup("first", Width = 200), LabelWidth(1), LabelText("")]
    public MonoBehaviour m_oBehaviour;
    [HorizontalGroup("first", Width = 50), LabelText("->"), LabelWidth(15)]
    public bool m_bEnable = true;

    public override void play()
    {
        base.play();

        m_oBehaviour.enabled = m_bEnable;
    }
}
