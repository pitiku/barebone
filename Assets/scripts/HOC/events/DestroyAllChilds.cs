using Sirenix.OdinInspector;
using UnityEngine;

public class DestroyAllChilds : TimedEvent
{
    public enum eState { Tag, Reference }
    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;

    [HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText(""), ShowIf("@m_eType == eState.Reference")]
    public GameObject m_oObject;

    public override void play()
    {
        base.play();

        if (m_oObject == null && m_eType == eState.Tag)
        {
            m_oObject = GameplayManager.Instance.getGameobject(m_oTag.Value);
        }

        int iChildCount = m_oObject.transform.childCount;

        for (int i = 0; i <iChildCount; i++)
        {
            GameObject oGO = m_oObject.transform.GetChild(i).gameObject;
            Destroy(oGO);
        }
    }
}
