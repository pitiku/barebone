using System.Collections.Generic;
using UnityEngine;

public class LanguageSlider : SliderPlus
{
    private List<string> m_asLanguages = new List<string>();

    public override void Awake()
    {
        defaultIndex = -1;

        int selectedIndex = -1;

        m_asLanguages = Localization.getAllLanguages();

        texts = new string[m_asLanguages.Count];
        for (int index = 0; index < texts.Length; ++index)
        {
            texts[index] = Localization.getLanguageName(m_asLanguages[index]);
            if (m_asLanguages[index] == SaveManager.SaveData.m_sLanguageCode)
            {
                selectedIndex = index;
            }
        }

        if (selectedIndex == -1)
        {
            selectedIndex = m_asLanguages.IndexOf(Localization.getCurrentLanguage());
            selectedIndex = Mathf.Clamp(selectedIndex, 0, m_asLanguages.Count - 1);
        }

        base.Awake();

        slider.value = selectedIndex;
    }

    public override void OnSliderValueChanged()
    {
        base.OnSliderValueChanged();

        int index = (int)slider.value;
        Localization.setLanguage(m_asLanguages[index]);
        SaveManager.SaveData.m_sLanguageCode = m_asLanguages[index];
    }
}