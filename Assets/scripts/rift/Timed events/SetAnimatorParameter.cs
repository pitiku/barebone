using Sirenix.OdinInspector;
using UnityEngine;

public class SetAnimatorParameter : TimedEvent
{
    public enum ParameterType { Integer }

    [SerializeField] Animator m_animator;
    [SerializeField] string m_sName;
    [SerializeField] ParameterType m_type;
    [SerializeField, ShowIf("@m_type == ParameterType.Integer")] int m_iValue;

    public override void play()
    {
        base.play();

        switch (m_type)
        {
            case ParameterType.Integer:
                m_animator.SetInteger(m_sName, m_iValue);
                break;
        }
    }
}
