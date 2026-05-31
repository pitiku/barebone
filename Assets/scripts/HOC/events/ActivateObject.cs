using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ActivateObject : TimedEvent
{
    public enum eState { Tag, Reference, LastInstantiated, IGameEntity, IGameObject }
    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;

    [HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText(""), ShowIf("@m_eType == eState.Reference")]
    public GameObject m_oObject;

    [HorizontalGroup("first", Width = 35), LabelText("->"), LabelWidth(15)]
    public bool m_bActive = true;

    [HorizontalGroup("first", Width = 50), LabelText("destroy"), LabelWidth(42)]
    public bool m_bDestroy = false;

    [SerializeField] bool m_bIgnoreCanvasAnimation;
    [SerializeReference, ShowIf("@m_eType == eState.IGameObject")] MonoBehaviour m_oIGameObject;

    public override void play()
    {
        base.play();

        //if (m_eType == eState.IGameObject)
        //{
        //    m_oObject = ((IGameObject)m_oIGameObject)?.GameObject;
        //}
        //else
        if (m_oObject == null && m_eType == eState.Tag)
        {
            m_oObject = GameplayManager.Instance.getGameobject(m_oTag.Value);
        }
        else if (m_eType == eState.LastInstantiated)
        {
            m_oObject = GameplayManager.Instance.m_oLastInstantiatedObject;
        }

        if (m_oObject == null) { return; }

        if (m_bDestroy) { Destroy(m_oObject); }
        else
        {
            m_oObject.setActive(m_bActive, !m_bIgnoreCanvasAnimation);
        }
    }
}