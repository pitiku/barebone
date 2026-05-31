using UnityEngine;

[System.Serializable]
public class PlayLegacyAnimation : TimedEvent
{
    public string m_sAnimation;
    public Animation m_oAnimation;

    public Vector2 m_vInitialNormalizedOffset = Vector2.zero;
    public Vector2 m_vPlaybackSpeed = Vector2.one;

    public override void play()
    {
        base.play();

        m_oAnimation.Play(m_sAnimation);

        foreach (AnimationState state in m_oAnimation)
        {
            state.normalizedTime = m_vInitialNormalizedOffset.range(null, "");
            state.speed = m_vPlaybackSpeed.range(null, "");
        }
    }
}
