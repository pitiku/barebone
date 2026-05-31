using UnityEngine;

public class SetAnimationBoolParameter : TimedEvent
{
    [SerializeField] Animator m_oAnimator;
    [SerializeField] string m_sName;
    [SerializeField] bool m_bValues;


    public override void play()
    {
        base.play();

        m_oAnimator.SetBool(m_sName, m_bValues);
    }

}