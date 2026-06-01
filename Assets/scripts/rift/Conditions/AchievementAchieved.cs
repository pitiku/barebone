
public class AchievementAchieved : Condition
{
    public Achievement m_eAchievement;

    bool m_bFirstTime = true;
    bool m_bWasAchieved = false;

    public override bool isMet(ICondition iC = null)
    {
        bool bIsAcheived = SaveManager.Instance.isAchieved(m_eAchievement.m_sID);

        if (m_bFirstTime)
        {
            m_bFirstTime = false;
            m_bWasAchieved = bIsAcheived;
        }

        return bIsAcheived && !m_bWasAchieved;
    }

    public override object Clone()
    {
        return new AchievementAchieved
        {
            m_eAchievement = this.m_eAchievement,
            // reset runtime flags
            m_bFirstTime = true,
            m_bWasAchieved = false
        };
    }
}
