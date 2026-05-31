using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollToTop : MonoBehaviour
{
    ScrollRect m_oScroll;

    private void Awake()
    {
        m_oScroll = GetComponent<ScrollRect>();
    }

    IEnumerator SetToTopNextFrame()
    {
        m_oScroll.verticalNormalizedPosition = 1f;
        yield return null;          // Frame 1
        m_oScroll.verticalNormalizedPosition = 1f;
        yield return null;          // Frame 2 (extra frame to ensure layout/update)
        m_oScroll.verticalNormalizedPosition = 1f;
    }

    void OnEnable()
    {
        StartCoroutine(SetToTopNextFrame());
    }
}