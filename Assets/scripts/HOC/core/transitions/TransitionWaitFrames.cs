using System.Collections;
using UnityEngine;

public class TransitionWaitFrames : StateTransition
{
    [SerializeField, Range(0,10)] int m_iValue;
    int m_iFramesLeft;

    bool m_bActivated = false;
    Coroutine m_oCoroutine;
    public override void activate()
    {
        base.activate();

        m_bActivated = false;

        m_oCoroutine = StartCoroutine(waitForNextFrame());
    }

    public override void deactivate()
    {
        base.deactivate();

        stopCoroutine();
    }

    private void OnDisable()
    {
        stopCoroutine();
    }

    public override bool update()
    {
        return m_bActivated;
    }

    private IEnumerator waitForNextFrame()
    {
        m_iFramesLeft = m_iValue;

        while (m_iFramesLeft > 0)
        {
            yield return new WaitForEndOfFrame();
            m_iFramesLeft--;
        }

        m_bActivated = true;
    }

    private void stopCoroutine()
    {
        if (m_oCoroutine != null)
        {
            StopCoroutine(m_oCoroutine);
        }

        m_oCoroutine = null;
    }
}
