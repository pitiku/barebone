using UnityEngine;

public class PlayHOCEventsOnAnimationStart : MonoBehaviour
{
    public Animator m_oAnimator;
    public string m_sAnimation = "in";
    bool m_bPlayed = false;

    private void OnEnable()
    {
        m_bPlayed = false;
    }

    private void Update()
    {
        if (m_bPlayed)
            return;

        AnimatorStateInfo currentInfo = m_oAnimator.GetCurrentAnimatorStateInfo(0);
        // Check if we've just entered the target state
        if (currentInfo.IsName(m_sAnimation)
            && currentInfo.normalizedTime >= 0f
            && !m_oAnimator.IsInTransition(0))
        {
            play();
        }
    }

    private void play()
    {
        if (!m_bPlayed)
        {
            gameObject.playHOCEvents();
            m_bPlayed = true;
        }
    }
}
