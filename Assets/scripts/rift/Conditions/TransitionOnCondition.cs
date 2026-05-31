using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class TransitionOnCondition : StateTransition
{
    public bool m_bAllConditions = true;

    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference]
    public List<Condition> m_aoConditions = new List<Condition>();

    public override bool update()
    {
        return m_aoConditions.areConditionsMet(m_bAllConditions);
    }
}
