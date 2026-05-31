using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ActivateMultipleObjects : TimedEvent
{
    public bool m_bActive = true;
    [ShowIf("@m_eType == eState.Reference")] public GameObject[] m_oObjects;
    [SerializeField] bool m_bIgnoreCanvasAnimation;

    public enum eState { Tag, Reference }
    [SerializeField] eState m_eType = eState.Reference;
    [SerializeField, ShowIf("@m_eType == eState.Tag")] CustomTag[] m_aoTag;

    public override void play()
    {
        base.play();

        if (m_eType == eState.Tag)
        {
            m_oObjects = new GameObject[m_aoTag.Length];

            for (int i = 0; i < m_oObjects.Length; i++)
            {
                m_oObjects[i] = GameplayManager.Instance.getGameobject(m_aoTag[i].Value);
            }
        }

        if (m_oObjects.isNullOrEmpty()) { return; }

        for (int i = 0; i < m_oObjects.Length; ++i)
        {
            GameObject itObject = m_oObjects[i];

            if (itObject == null) { continue; }

            itObject.setActive(m_bActive, !m_bIgnoreCanvasAnimation);
        }
    }
}