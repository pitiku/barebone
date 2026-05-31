using System;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Achievements/Achievement Preset", fileName = "Achievement")]
public class Achievement : ScriptableObject
{
    [ReadOnly] public string m_sID;
    public string m_sKeyI2;
    public Sprite m_oSpriteUnlocked;
    public Sprite m_oSpriteLocked;
    [SerializeReference] public Condition m_oCondition;

    public string ID
    {
        get { return m_sID; }
        set { m_sID = value; }
    }

    public string InstanceID
    {
        get { return ID; }
        set { }
    }

    public void checkUnlock()
    {
        bool bIsUnlocked = isUnlocked();
        if (bIsUnlocked)
        {
            // unlock achievement in every platform (do this outside our check to avoid problems of bad synchronization)
            PlatformManager.Current.UnlockChallenge(this);

            if (!isCompleted())
            {
                DataManager.SaveDataProgression.m_asAchievementsCompleted.Add(m_sID);
                AchievementsManager.Instance.queueAchievement(this);
            }
        }
    }

    public bool isCompleted()
    {
        return DataManager.SaveDataProgression.m_asAchievementsCompleted.Contains(m_sID);
    }

    public bool isUnlocked()
    {
        return m_oCondition == null || m_oCondition.isMet();
    }

    public float getProgress()
    {
        if (m_oCondition is IProgressiveAchievement oProgression)
        {
            return oProgression.getProgress();
        }
        return isCompleted() ? 1 : 0;
    }

    public bool tryGetProgressString(out string _sProgression)
    {
        _sProgression = String.Empty;
        
        if (m_oCondition is IProgressiveAchievement oProgression)
        {
            _sProgression = oProgression.getProgressString();
            return true;
        }

        return false;
    }

    public bool hasProgress()
    {
        return true;
    }

    public string getName()
    {
        return Utils.getTranslationSafe($"achievementTitle_{m_sKeyI2}", "achievements");
    }
    public string getDescription()
    {
        return Utils.getTranslationSafe($"achievementDescription_{m_sKeyI2}", "achievements");
    }
}

public interface IProgressiveAchievement
{
    public float getProgress();
    public string getProgressString();
    
}