using System;
using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
    private string[] m_asLanguages;

    public void Awake()
    {
        m_asLanguages = Localization.getAllLanguages().ToArray();
    }

    public virtual void OnEnable()
    {
    }

    private void onLanguageSelected(int _iLanguageIndex)
    {
        Localization.setLanguage(m_asLanguages[_iLanguageIndex]);
        DataManager.SaveDataOptions.m_sLanguageCode = m_asLanguages[_iLanguageIndex];
    }
}