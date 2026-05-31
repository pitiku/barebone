using UnityEngine;

public class PlayHOCEventsOnAnimationFinished : MonoBehaviour
{
    public Animator m_oAnimator;
    public string m_sAnimation = "out";
    bool m_bPlayed = false;

    private void OnEnable()
    {
        m_bPlayed = false;
    }

    public void Update()
    {
        if (m_bPlayed) return;

        AnimatorStateInfo oCurrentInfo = m_oAnimator.GetCurrentAnimatorStateInfo(0);
        if (oCurrentInfo.IsName(m_sAnimation) && oCurrentInfo.normalizedTime > 0.999f && !m_oAnimator.IsInTransition(0))
        {
            play();
        }
    }

    void play()
    {
        if (!m_bPlayed)
        {
            gameObject.playHOCEvents();
            m_bPlayed = true;
        }
    }
}