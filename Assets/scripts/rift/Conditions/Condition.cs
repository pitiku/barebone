using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICondition { }

[Serializable]
public class Condition : ICloneable
{
    protected GameObject m_oGameObject;
    public const string NULL_TEXT = "null";

    public virtual void initialize(GameObject _oGameObject)
    {
        m_oGameObject = _oGameObject;
    }

    public virtual bool isMet(ICondition iC = null)
    {
        return false;
    }

    public virtual string getUnavailableText()
    {
        return Utils.getTranslation(I2Consts.I2_CONDITION + "notMet", I2Consts.CATEGORY_I2_COMBAT);
    }

    public virtual string getConditionText()
    {
        return NULL_TEXT;
    }

    public string getConditionKeywordTranslation(string _sKeyword)
    {
        return Utils.getTranslation(I2Consts.I2_CONDITION + _sKeyword, I2Consts.CATEGORY_I2_COMBAT);
    }

    public string getUnavailableConditionKeywordTranslation(string _sKeyword)
    {
        return Utils.getTranslation(I2Consts.I2_CONDITION_UNAVAILABLE + _sKeyword, I2Consts.CATEGORY_I2_COMBAT);
    }

    public virtual void reset() { }

    // Base shallow clone. Derived classes should override and deep-copy their own fields.
    public virtual object Clone()
    {
        return new Condition();
    }

    public virtual bool contains(Type t)
    {
        return t.IsAssignableFrom(GetType()); // true also for derived types
    }

    public virtual List<Condition> getConditionsOfType(Type t)
    {
        if (t.IsAssignableFrom(GetType()))
        {
            return new List<Condition> { this };
        }
        return new List<Condition>(); // true also for derived types
    }
}