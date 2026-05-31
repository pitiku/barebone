using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AndCondition : Condition
{
    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference]
    List<Condition> m_aoConditions = new List<Condition>();

    public List<Condition> Conditions => m_aoConditions;

    public AndCondition(Condition[] _aoConditions)
    {
        m_aoConditions.AddRange(_aoConditions);
    }

    public override bool isMet(ICondition iC = null)
    {
        if (m_aoConditions.isNullOrEmpty())
        {
            return true;
        }
        for (int iIndex = 0; iIndex < m_aoConditions.Count; ++iIndex)
        {
            if (!m_aoConditions[iIndex].isMet(iC))
            {
                return false;
            }
        }
        return true;
    }

    public override void initialize(GameObject _oGameObject)
    {
        base.initialize(_oGameObject);

        foreach (Condition oCondition in m_aoConditions)
        {
            oCondition.initialize(_oGameObject);
        }
    }

    public override string getConditionText()
    {
        string sText = "";
        for (int i = 0; i < m_aoConditions.Count; i++)
        {
            sText += m_aoConditions[i].getConditionText();
            if (i + 1 < m_aoConditions.Count)
            {
                sText += " and ";
            }
        }
        return sText;
    }

    public override object Clone()
    {
        var clone = new AndCondition(System.Array.Empty<Condition>());
        if (m_aoConditions != null)
        {
            clone.m_aoConditions = new List<Condition>(m_aoConditions.Count);
            for (int i = 0; i < m_aoConditions.Count; i++)
            {
                var c = m_aoConditions[i];
                clone.m_aoConditions.Add(c != null ? (Condition)c.Clone() : null);
            }
        }
        else
        {
            clone.m_aoConditions = new();
        }
        return clone;
    }

    public override bool contains(Type T)
    {
        for (int i = 0; i < m_aoConditions.Count; i++)
        {
            if (m_aoConditions[i].contains(T))
            {
                return true;
            }
        }
        return false;
    }

    public override List<Condition> getConditionsOfType(Type T)
    {
        List<Condition> aoConditions = new();
        for (int i = 0; i < m_aoConditions.Count; i++)
        {
            if (m_aoConditions[i].contains(T))
            {
                aoConditions.Add(m_aoConditions[i]);
            }
        }
        return aoConditions;
    }
}