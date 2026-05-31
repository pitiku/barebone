using Sirenix.OdinInspector;
using UnityEngine;

public class SetAnimationTrigger : TimedEvent
{
    [HorizontalGroup("first", Width = 170.0f), HideLabel]
    public Animator m_oAnimator;
    [HorizontalGroup("first", Width = 170.0f), LabelWidth(16), LabelText("->")]
    public string m_sAnimationTrigger;

    public override void play()
    {
        base.play();
        m_oAnimator.SetTrigger(m_sAnimationTrigger);
    }

    public override void playLoading()
    {
        base.play();
        m_oAnimator.SetTrigger(m_sAnimationTrigger);
    }
}
