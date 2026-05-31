using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsManager : StateMachineSingleton<AchievementsManager>
{
    [SerializeField] private TMP_Text m_oText;
    [SerializeField] private Image m_oSprite;

    private List<Achievement> m_oAchievementsToShow = new List<Achievement>();

    public List<Achievement> getAchievements() => m_oAchievementsToShow;
    public void checkAchievements()
    {
        //List<Achievement> aoAchievements = ContentManager.Instance.m_aoAllAchievements;

        //for (int iIndex = 0; iIndex < aoAchievements.Count; ++iIndex)
        //{
        //    Achievement oAchievement = aoAchievements[iIndex];
        //    oAchievement.checkUnlock();
        //}
    }

    public void showAchievement()
    {
        Achievement oAchievement = m_oAchievementsToShow[0];
        m_oText.text = oAchievement.getName();
        m_oSprite.sprite = oAchievement.m_oSpriteUnlocked;
        m_oAchievementsToShow.RemoveAt(0);
    }

    public void queueAchievement(Achievement achievement)
    {
#if UNITY_SWITCH || UNITY_EDITOR || (UNITY_STANDALONE && DISABLESTEAMWORKS)
        m_oAchievementsToShow.Add(achievement);
#endif
    }

    public bool hasAchievementsToShow()
    {
#if UNITY_SWITCH || UNITY_EDITOR || (UNITY_STANDALONE && DISABLESTEAMWORKS)
        return m_oAchievementsToShow.Count > 0;
#else
                return false;
#endif
    }
}