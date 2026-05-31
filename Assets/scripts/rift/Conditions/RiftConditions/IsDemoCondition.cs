using UnityEngine;

public class IsDemoCondition : Condition
{
    [SerializeField] bool m_bNegate;
    public override bool isMet(ICondition iC = null)
    {
#if VERSION_DEMO
        return !m_bNegate;
#else
        return m_bNegate;
#endif
    }

    public override object Clone()
    {
        return new IsDemoCondition
        {
            m_bNegate = this.m_bNegate
        };
    }
}
