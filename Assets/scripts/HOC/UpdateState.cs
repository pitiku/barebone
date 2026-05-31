public class UpdateState : Behavior
{
    public bool m_bActivateState;
    public State m_oReferencedState;

    public override void activate()
    {
        base.activate();

        if (m_bActivateState)
        {
            m_oReferencedState.activate();
        }
    }

    public override void update()
    {
        base.update();

        m_oReferencedState.update();
    }

    public override void fixedUpdate()
    {
        base.update();

        m_oReferencedState.fixedUpdate();
    }

    public override void lateUpdate()
    {
        base.update();

        m_oReferencedState.lateUpdate();
    }
}
