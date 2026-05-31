using Sirenix.OdinInspector;

public class LockInputs : TimedEvent
{
    public bool m_bLockOrUnlock = true;
    [ShowIf("m_bLockOrUnlock", true)]
    public float m_time = 0.1f;

    public override void play()
    {
        base.play();

        if (m_bLockOrUnlock)
        {
            RewiredManager.Instance.LockInputs(m_time);

        }
        else
        {
            RewiredManager.Instance.UnlockInputs();
        }
    }
}
