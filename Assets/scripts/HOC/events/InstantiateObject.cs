using UnityEngine;

public class InstantiateObject : TimedEvent
{
    [SerializeField] GameObject m_oPrefab;
    [SerializeField] Transform m_trParent;

    public override void play()
    {
        base.play();

        GameObject oGO = Instantiate(m_oPrefab, m_trParent);

        GameplayManager.Instance.m_oLastInstantiatedObject = null;
        GameplayManager.Instance.m_oLastInstantiatedObject = oGO;
    }
}
