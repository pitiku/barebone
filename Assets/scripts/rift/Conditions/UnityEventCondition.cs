using System;
using UnityEngine.Events;

public class BoolContainer
{
    public bool m_bValue;
}

[Serializable]
public class UnityEventCondition : Condition
{
    public UnityEvent<BoolContainer> m_oEvent;

    public override bool isMet(ICondition iC = null)
    {
        BoolContainer b = new BoolContainer();
        m_oEvent.Invoke(b);
        return b.m_bValue;
    }

    public override object Clone()
    {
        var clone = new UnityEventCondition();
        clone.m_oEvent = m_oEvent; // keep reference
        return clone;
    }
}
