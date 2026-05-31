/*using UnityEngine;

public class CountAchievedCondition : Condition
{
    [SerializeReference] Count m_oCount;
    [SerializeField] bool m_bIsSpecific = false;
    [SerializeField] uint m_iValue;

    public override void initialize(GameObject _oGameObject)
    {
        if (_oGameObject == null) { return; }
        if (!m_bIsSpecific) { return; }

        IDCount oCount = m_oCount as IDCount;
        if (oCount == null) { return; }

        IIdentifiable oIdentifiable = _oGameObject.GetComponent<IIdentifiable>();
        if (oIdentifiable == null) { return; }

        oCount.setID(oIdentifiable.ID);
    }

    public override bool isMet(ICondition iC = null)
    {
        return GameCommon.getTotalCount(m_oCount) >= m_iValue;
    }

    public Count Count { get { return m_oCount; } }
    public int Value { get { return (int)m_iValue; } }
}
*/