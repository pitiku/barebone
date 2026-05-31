/*
public class IsCountComparison : ValueComparison
{
    [SerializeReference] Count m_oCount;
    [SerializeField] eCountGroup m_eCountType;

    public override float getRealValue()
    {
        float fValue;

        if(m_eCountType == eCountGroup.Run) { fValue = GameCommon.getRunCount(m_oCount); }
        else { fValue = GameCommon.getTotalCount(m_oCount); }

        return fValue;
    }
}
*/