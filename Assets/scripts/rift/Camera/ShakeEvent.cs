using Sirenix.OdinInspector;
using UnityEngine;

public class ShakeEvent : TimedEvent
{
    [HorizontalGroup("first", Width = 70, MarginRight = 5), LabelText("shake"), LabelWidth(35)]
    [SerializeField] float m_fShakeAmount;

    public override void play()
    {
        base.play();

        GameplayCamera.Instance.shake(m_fShakeAmount);
    }
}
