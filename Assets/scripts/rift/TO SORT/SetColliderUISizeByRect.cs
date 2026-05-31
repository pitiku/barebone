using UnityEngine;

public class SetColliderUISizeByRect : MonoBehaviour
{
    [SerializeField] RectTransform m_oRectReference;
    BoxCollider m_oBox;
    float m_fWidth;
    private void Start()
    {
        m_oBox = GetComponent<BoxCollider>();
    }
    void LateUpdate()
    {
        if (m_oBox != null && !m_fWidth.approximately(m_oRectReference.rect.width))
        {
            m_fWidth = m_oRectReference.rect.width;
            m_oBox.size = new Vector3(m_fWidth, m_oRectReference.rect.height, m_oBox.size.z);
        }
    }
}
