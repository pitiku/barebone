using Sirenix.OdinInspector;
using UnityEngine;

public class SetObjectLocalPosition : TimedEvent
{
    public enum eState { Tag, Reference, LastInstantiated }
    public enum ePositionType { Percentage, Absolute }
    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;
    [SerializeField] ePositionType m_ePositionType = ePositionType.Percentage;

    [HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText(""), ShowIf("@m_eType == eState.Reference")]
    public GameObject m_oObject;

    [SerializeField, ShowIf("@m_ePositionType == ePositionType.Absolute")] Vector3 m_vPosition;
    [SerializeField, ShowIf("@m_ePositionType == ePositionType.Absolute")] Transform m_vTransformPosition;
    [SerializeField, ShowIf("@m_ePositionType == ePositionType.Percentage")] float m_fPercentage;

    public override void play()
    {
        base.play();

        if (m_oObject == null && m_eType == eState.Tag)
        {
            m_oObject = GameplayManager.Instance.getGameobject(m_oTag.Value);
        }
        else if (m_eType == eState.LastInstantiated)
        {
            m_oObject = GameplayManager.Instance.m_oLastInstantiatedObject;
        }

        if (m_oObject == null) { return; }

        if (m_ePositionType == ePositionType.Percentage)
        {
            RectTransform oParentRectTr = m_oObject.transform.parent.GetComponent<RectTransform>();

            float xPos = -oParentRectTr.rect.width * 0.5f + m_fPercentage * oParentRectTr.rect.width;

            m_oObject.transform.localPosition = new Vector3(xPos, 0, 0);
        }
        else
        {
            if (m_vTransformPosition != null) { m_oObject.transform.localPosition = m_vTransformPosition.localPosition; }
            else { m_oObject.transform.localPosition = m_vPosition; }
        }
    }

    public float Percentage { set { m_fPercentage = value; } }
}
