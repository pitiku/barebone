using UnityEngine;

public class TransitionOnAnimationFinished : StateTransition
{
    public Animator m_oAnimator;

    public override bool update()
    {
        return m_oAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !m_oAnimator.IsInTransition(0);
    }
}
