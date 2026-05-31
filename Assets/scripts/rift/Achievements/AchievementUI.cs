using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private ScriptableObject m_oContentStateProivder;
    [SerializeField] private GameObject m_oToBeSeenIcon;
    [SerializeField] private Image m_oIcon;
    [SerializeField] private TMP_Text m_oTitle , m_oDescription , m_oProgression;
    [SerializeField] private Slider m_oProgressionSlider;
    
    public void setTitleColor(Color _oColor) => m_oTitle.color = _oColor;
    public void setAchivement(Achievement _oAchivement)
    {
        if (_oAchivement.isCompleted())
        {
            m_oIcon.sprite = _oAchivement.m_oSpriteUnlocked;
        }
        else
        {
            m_oIcon.sprite = _oAchivement.m_oSpriteLocked;
        }
        
        m_oTitle.text = _oAchivement.getName();
        m_oDescription.text = _oAchivement.getDescription();
        m_oProgressionSlider.value = _oAchivement.getProgress();
        m_oProgression.text = _oAchivement.tryGetProgressString(out string _stringProgress) ? _stringProgress : string.Empty;
    }
}
