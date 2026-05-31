using UnityEngine;

public class RandomCondition : Condition
{
    [SerializeField] float m_fPercentage;

    public override bool isMet(ICondition iC = null)
    {
        return RandomUtils.value(GameplayManager.NODE_SEED, "[RandomCondition] Check") <= m_fPercentage;
    }

    public override object Clone()
    {
        var clone = new RandomCondition();
        clone.m_fPercentage = m_fPercentage;
        return clone;
    }
}