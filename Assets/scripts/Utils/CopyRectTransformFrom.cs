using System.Collections.Generic;
using UnityEngine;

public class CopyRectTransformFrom : MonoBehaviour
{
    public List<RectTransform> m_aoRectToCopyFrom = new();
    public bool m_bWidth;
    public bool m_bHeigth;


    RectTransform m_oThisRect;

    private void Awake()
    {
        m_oThisRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        for (int i = 0; i < m_aoRectToCopyFrom.Count; i++)
        {
            if (m_aoRectToCopyFrom[i].gameObject.activeInHierarchy)
            {
                if (m_bWidth)
                {
                    m_oThisRect.sizeDelta = new Vector2(m_aoRectToCopyFrom[i].sizeDelta.x, m_oThisRect.sizeDelta.y);
                }
                if (m_bHeigth)
                {
                    m_oThisRect.sizeDelta = new Vector2(m_oThisRect.sizeDelta.y, m_aoRectToCopyFrom[i].sizeDelta.y);
                }
                return;
            }
        }
    }
}
