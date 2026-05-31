using I2.Loc;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BasicLocalize : MonoBehaviour
{
    public string m_sCategory = I2Consts.CATEGORY_I2_MENU_UI;
    public string m_sTerm;
    public bool m_params = false;
    public bool m_bSplitInLines = false;

    [NonSerialized] public UnityEvent m_oOnUpdateText = new UnityEvent();

    void Start()
    {
        updateText();
        LocalizationManager.OnLocalizeEvent += updateText;
    }

    void OnDestroy()
    {
        LocalizationManager.OnLocalizeEvent -= updateText;
    }

    public void updateText()
    {
        if (enabled)
        {
            string sTranslation = Utils.getTranslation(m_sTerm, m_sCategory);

#if UNITY_EDITOR
            if (sTranslation == null) Debug.LogWarning("BasicLocalize without translation in " + transform.getPath());
#endif

            if (m_bSplitInLines)
            {
                sTranslation = sTranslation.Replace(" ", "\n");
            }
            setText(sTranslation);
            m_oOnUpdateText.Invoke();
        }
    }
    public virtual void setText(string _sText)
    {
        setText(gameObject, _sText);
    }

    protected void setText(GameObject _oGameObject, string _sText)
    {
        if (!_sText.isNullOrEmpty()) _sText = _sText.Replace("\\n", "\n");

        if (_oGameObject.TryGetComponent(out TextMeshPro oTextMeshPro))
        {
            oTextMeshPro.text = _sText;
        }
        else if (_oGameObject.TryGetComponent(out TextMeshProUGUI oTextMeshProUGUI))
        {
            oTextMeshProUGUI.text = _sText;
        }
        else if (_oGameObject.TryGetComponent(out TextMesh oTextMesh))
        {
            oTextMesh.text = _sText;
        }
        else if (_oGameObject.TryGetComponent(out Text oText))
        {
            oText.text = _sText;
        }
    }

    public string getText()
    {
        if (TryGetComponent(out TextMeshPro oTextMeshPro))
        {
            return oTextMeshPro.text;
        }
        else if (TryGetComponent(out TextMeshProUGUI oTextMeshProUGUI))
        {
            return oTextMeshProUGUI.text;
        }
        else if (TryGetComponent(out TextMesh oTextMesh))
        {
            return oTextMesh.text;
        }
        else if (TryGetComponent(out Text oText))
        {
            return oText.text;
        }

        return "";
    }
}

public static class I2Consts
{
    public const string CATEGORY_I2_MODIFIER_STATUS = "effects";
    public const string CATEGORY_I2_OTHER = "other";
    public const string CATEGORY_I2_COMBAT = "combat";
    public const string CATEGORY_I2_ITEMS = "items";
    public const string CATEGORY_I2_SKILLS = "skills";
    public const string CATEGORY_I2_STATS = "skills";
    public const string CATEGORY_I2_MENU_UI = "UI";
    public const string CATEGORY_I2_BARK = "barks";
    public const string CATEGORY_I2_COMPENDIUM = "compendium";
    public const string I2_STATUS = "status_";
    public const string I2_CONDITION = "condition_";
    public const string I2_CONDITION_UNAVAILABLE = "condition_unavailable_";
    public const string I2_SKILL = "skill_";
    public const string I2_ACTIONSL = "action_";
    public const string I2_MODIFIER_STAT = "modifier_stat_";
    public const string I2_ON_EVENT_SKILL = "onEventSkill_";
    public const string I2_CUSTOM_POPUP = "custom_popup_";
}