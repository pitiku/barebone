using Sirenix.OdinInspector;
using UnityEngine;

public class SetTimeScaleTimedEvent : TimedEvent
{
    public enum Type { Fixed, Animation }

    [SerializeField, ShowIf("@isFixed()")] float m_fValue;
    [SerializeField, ShowIf("@isAnimation()")] AnimationCurve m_oCurve;
    [SerializeField] Type m_eType;

    public override void play()
    {
        base.play();

        if(m_eType == Type.Fixed)
        {
            TimeManager.Instance.setTime(m_fValue);
        }
        else
        {
            TimeManager.Instance.setTime(m_oCurve);
        }
    }

    public bool isFixed()
    {
        return m_eType == Type.Fixed;
    }

    public bool isAnimation()
    {
        return !isFixed();
    }
}
