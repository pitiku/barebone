using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProbabilityPair<T>
{
    [OnInspectorGUI("@$property.State.Expanded = true")]
    public Probability p;
    [OnInspectorGUI("@$property.State.Expanded = true"), HorizontalGroup("2", Width = 400, MarginLeft = 15, MarginRight = 5), LabelText("value"), LabelWidth(50), ListDrawerSettings(ShowFoldout = true)]
    public T value;

    public ProbabilityPair() { }

    public ProbabilityPair(T _oT, float _fWeight)
    {
        value = _oT;
        p = new Probability();
        p.m_fWeight = _fWeight;
    }
}

public static class ProbabilityUtils
{
    #region CUSTOM PAIRS
    public static T getRandom<T>(this List<ProbabilityPair<T>> _ao, string _sSeed, string _sContext, float _fAddedProb = 0)
    {
        float fRandom = RandomUtils.range(_sSeed, 0, 1, _sContext);

        float fAddedValue = _fAddedProb;
        for (int i = 0; i < _ao.Count; ++i)
        {
            fAddedValue += _ao[i].p.m_fProbability;
            if (fRandom <= fAddedValue)
            {
                T oItem = _ao[i].value;
#if RANDOM_DEBUG
                if (_sContext != "")
                {
                    string sText = $"[Random] [Seed:{_sSeed} | Item: {oItem}] {_sContext}";
                    RandomDebugger.Instance.Log(sText);
                }
#endif
                return oItem;
            }
        }

        return default(T);
    }

    public static T getRandom<T>(this List<ProbabilityPair<T>> _ao)
    {
        return _ao.getRandom<T>(null, "", 0);
    }

    public static int getRandomIndex<T>(this List<ProbabilityPair<T>> _ao, string _sSeed, string _sContext, float _fAddedProb)
    {
        float fRandom = RandomUtils.range(_sSeed, 0, 1, _sContext);

        float fAddedValue = _fAddedProb;
        for (int i = 0; i < _ao.Count; ++i)
        {
            fAddedValue += _ao[i].p.m_fProbability;
            if (fRandom <= fAddedValue)
            {
                return i;
            }
        }

        return -1;
    }
    public static int getRandomIndex<T>(this List<ProbabilityPair<T>> _ao)
    {
        return _ao.getRandomIndex(null, "", 0);
    }

    public static int getRandomIndex<T>(this List<ProbabilityPair<T>> _ao, string _sSeed, string _sContext)
    {
        return _ao.getRandomIndex(_sSeed, _sContext, 0);
    }
    #region Luck
    // Luck is a number between 0 and 1 that represents a percentage
    // 0,1 luck means that the every item besides the most basic object will have an increase 10% to be choosen
    // the percentage gained will be substracted from the most basic object
    public static T getRandomWithLuck<T>(this List<ProbabilityPair<T>> _aoOrderedProbabilities, string _sSeed, string _sContext, float _fLuck = 0)
    {
        float fLuck = Mathf.Clamp01(_fLuck);

        if (fLuck == 0) { return _aoOrderedProbabilities.getRandom(_sSeed, _sContext); }

        List<ProbabilityPair<T>> aoOrderedProbabilities = _aoOrderedProbabilities.copy();

        for (int i = 0; i <  aoOrderedProbabilities.Count - 1; ++i)
        {
            float fValueToModify = aoOrderedProbabilities[i].p.m_fProbability * fLuck;

            if (fValueToModify == 0) { continue; }

            aoOrderedProbabilities.applyLuckModifier(aoOrderedProbabilities[i].value, fValueToModify);
        }

        return aoOrderedProbabilities.getRandom(_sSeed, _sContext);
    }

    public static void applyLuckModifier<T>(this List<ProbabilityPair<T>> _aoPairs, T _eRarity, float _fValueToModify)
    {
        // when applying modifiers to the rarity, whe add and substract the amount to the lowest rank rarity
        // we assume that the modifiers always have a skillRarity type different from common

        float fUnmodifiedValue = _fValueToModify;

        for (int i = _aoPairs.Count - 1; i >= 0 && fUnmodifiedValue > 0; i--)
        {
            T itSkillRarity = _aoPairs[i].value;

            if (itSkillRarity.Equals(_eRarity)) { break; } // only substract from lower tiers

            fUnmodifiedValue = removeProbabilityFrom(_aoPairs, itSkillRarity, fUnmodifiedValue);
        }

        // find the index of the probability pair to modify in the list
        int iIndex = -1;

        for (int i = 0; i < _aoPairs.Count && iIndex == -1; i++)
        {
            T itRarity = _aoPairs[i].value;

            if (itRarity.Equals(_eRarity)) { iIndex = i; ; }
        }

        // modify the probability to the found index
        // the value to modify cannot be bigger than the one modified in lower tiers rarities
        Probability oProbability = new Probability();
        float fModifiedValue = _fValueToModify - fUnmodifiedValue;
        oProbability.m_fProbability = _aoPairs[iIndex].p.m_fProbability + fModifiedValue;
        _aoPairs[iIndex].p = oProbability;
    }

    // returns 0 if all the probability was substracted for the given probability, returns a positive number if there is any probability that could not be substracted
    public static float removeProbabilityFrom<T>(this List<ProbabilityPair<T>> _aoPairs, T _eRarity, float _fValue)
    {
        // find the index of the probability pair to modify in the list
        int iIndex = -1;

        for (int i = 0; i < _aoPairs.Count && iIndex == -1; i++)
        {
            T itRarity = _aoPairs[i].value;

            if (itRarity.Equals(_eRarity)) { iIndex = i; ; }
        }

        Probability oProbability = new Probability();

        // negatives values means that not all the probability could be substracted
        float fNewPercentage = _aoPairs[iIndex].p.m_fProbability - _fValue;

        // negatives values assign a value of probability 0 since negative probability have no sense
        oProbability.m_fProbability = fNewPercentage <= 0 ? 0 : fNewPercentage;
        _aoPairs[iIndex].p = oProbability;

        // only different from zero when not all the probability could be substracted
        float fLeftOverPercentage = fNewPercentage < 0 ? -fNewPercentage : 0;

        return fLeftOverPercentage;
    }

    #endregion Luck

    public static void Add<T>(this List<ProbabilityPair<T>> _ao, T _o)
    {
        _ao.Add(new ProbabilityPair<T>(_o, 1));
    }
    public static void Add<T>(this List<ProbabilityPair<T>> _ao, T _o, float _fWeight)
    {
        _ao.Add(new ProbabilityPair<T>(_o, _fWeight));
    }

    public static void Remove<T>(this List<ProbabilityPair<T>> _ao, T _o)
    {
        for (int i = 0; i < _ao.Count; i++)
        {
            if (_ao[i].value.Equals(_o))
            {
                _ao.RemoveAt(i);
                return;
            }
        }
    }

    public static T getRandomWithoutRepeating<T>(this List<ProbabilityPair<T>> _ao, List<T> _aoCensored, string _sSeed, string _sContext, float _fAddedProb)
    {
        List<ProbabilityPair<T>> aoListToSelectFrom = _ao.copy();

        // remove censored elements from the list
        for (int i = 0; i < aoListToSelectFrom.Count;)
        {
            if (_aoCensored.Contains(aoListToSelectFrom[i].value))
            {
                aoListToSelectFrom.RemoveAt(i);
            }
            else { i++; }
        }

        // if all items are censored return the 
        if (aoListToSelectFrom.isNullOrEmpty()) { return default(T); }

        // get a random element of the list
        aoListToSelectFrom.normalizeProbabilities();
        return aoListToSelectFrom.getRandom(_sSeed, _sContext, _fAddedProb);
    }

    public static void normalizeProbabilities<T>(this List<ProbabilityPair<T>> _ao)
    {
        float fTotal = 0.0f;

        if (_ao.Count == 1 && _ao[0].p != null)
        {
            _ao[0].p.m_fWeight = 1;
        }

        for (int i = 0; i < _ao.Count; ++i)
        {
            if (_ao[i].p == null)
            {
                return;
            }

            fTotal += _ao[i].p.m_fWeight;
        }

        if (fTotal == 0.0f)
        {
            return;
        }

        for (int i = 0; i < _ao.Count; ++i)
        {
            _ao[i].p.m_fProbability = _ao[i].p.m_fWeight / fTotal;
        }
    }

    public static void normalizeProbabilities<T>(this ProbabilityPair<T>[] _ao)
    {
        float fTotal = 0.0f;

        if (_ao.Length == 1 && _ao[0].p != null)
        {
            _ao[0].p.m_fWeight = 1;
        }

        for (int i = 0; i < _ao.Length; ++i)
        {
            if (_ao[i].p == null)
            {
                return;
            }

            fTotal += _ao[i].p.m_fWeight;
        }

        if (fTotal == 0.0f)
        {
            return;
        }

        for (int i = 0; i < _ao.Length; ++i)
        {
            _ao[i].p.m_fProbability = _ao[i].p.m_fWeight / fTotal;
        }
    }

    public static List<T> getValues<T>(this List<ProbabilityPair<T>> _ao)
    {
        List<T> oList = new List<T>();
        foreach (var oPair in _ao)
        {
            oList.Add(oPair.value);
        }
        return oList;
    }
#endregion
}
