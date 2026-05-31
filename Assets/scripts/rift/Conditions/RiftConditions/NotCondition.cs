using UnityEngine;

public class NotCondition : Condition
{
    [SerializeReference] Condition m_oCondition;
    public override bool isMet(ICondition iC = null)
    {
        return !m_oCondition.isMet(iC);
    }

    public override object Clone()
    {
        var clone = new NotCondition();
        clone.m_oCondition = m_oCondition != null ? (Condition)m_oCondition.Clone() : null;
        return clone;
    }
}
