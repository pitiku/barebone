using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderPlus : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    public string[] texts;
    public int defaultIndex;
    public bool m_localized = false;
    public SoundUnit m_oOnChangedSound;

    protected bool m_justEnabled = true;

    public virtual void Awake()
    {
        if (m_localized)
        {
            I2.Loc.LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
        }

        slider.minValue = 0;
        slider.maxValue = texts.Length - 1;

        slider.onValueChanged.AddListener(delegate
        {
            OnSliderValueChanged();
        });

        if (defaultIndex > -1)
        {
            slider.value = defaultIndex;
        }
    }

    public virtual void OnDestroy()
    {
        if (m_localized)
        {
            I2.Loc.LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
        }
    }

    public virtual void OnEnable()
    {
        m_justEnabled = true;
        slider.onValueChanged.Invoke(slider.value);
    }

    public virtual void OnSliderValueChanged()
    {
        int iIndex = (int)slider.value;
        if (iIndex < 0 || iIndex >= texts.Length)
        {
            Deb.logWarning($"SliderPlus: slider value {iIndex} is out of bounds for texts array (length {texts.Length}).");
            return;
        }

        if (m_localized)
        {
            text.text = texts[iIndex].getTranslation(I2Consts.CATEGORY_I2_MENU_UI);
        }
        else
        {
            text.text = texts[iIndex];
        }

        if (m_justEnabled)
        {
            m_justEnabled = false;
        }
        else
        {
            if (m_oOnChangedSound != null) m_oOnChangedSound.play();
        }
    }

    public void OnLanguageChanged()
    {
        if (m_localized)
        {
            int iIndex = (int)slider.value;
            if (iIndex < 0 || iIndex >= texts.Length)
            {
                Deb.logWarning($"SliderPlus: slider value {iIndex} is out of bounds for texts array (length {texts.Length}).");
                return;
            }
            text.text = texts[iIndex].getTranslation(I2Consts.CATEGORY_I2_MENU_UI);
        }
    }
}
