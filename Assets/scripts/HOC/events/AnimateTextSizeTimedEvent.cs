using System.Collections;
using UnityEngine;

public class AnimateTextSizeTimedEvent : TimedEvent
{
    [SerializeField] float m_fMinScalePercentage;
    [SerializeField] float m_fMaxScalePercentage;

    [SerializeField] Transform m_oObject;
    [SerializeField] float m_fDuration;
    [SerializeField] AnimationCurve m_oCurve;

    float m_fAnimationTime = 0;

    Coroutine m_oCoroutine;

    float m_fInitialSize;

    private void Start()
    {
        m_fInitialSize = m_oObject.transform.localScale.x;
    }

    public override void play()
    {
        base.play();

        if (m_oCoroutine != null) { StopCoroutine(m_oCoroutine); }

        m_oCoroutine = StartCoroutine("animationCoroutine");
    }

    IEnumerator animationCoroutine()
    {
        m_fAnimationTime = 0;

        while (m_fAnimationTime < m_fDuration)
        {
            float fScale = m_fMinScalePercentage + (m_fMaxScalePercentage - m_fMinScalePercentage) * m_oCurve.Evaluate(m_fAnimationTime / m_fDuration);

            m_oObject.transform.localScale = fScale * Vector3.one;

            m_fAnimationTime += Time.deltaTime;

            yield return null;
        }

        m_oObject.transform.localScale = m_fInitialSize * Vector3.one;
    }

    private void OnDisable()
    {
        if (m_oCoroutine != null) { StopCoroutine(m_oCoroutine); }
    }

}
