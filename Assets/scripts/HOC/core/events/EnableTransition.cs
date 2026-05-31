[System.Serializable]
public class EnableTransition : TimedEvent
{
    public bool m_bEnable = true;
    public StateTransition m_oTransition;

    public override void play()
    {
        base.play();

        m_oTransition.enabled = m_bEnable;
    }
}
