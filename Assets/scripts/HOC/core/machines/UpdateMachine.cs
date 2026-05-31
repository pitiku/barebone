
[System.Serializable]
public class UpdateMachine : Behavior
{
    public bool m_bActivateMachine;
    public HOCComponentStateMachine m_oMachine;
    public State m_oToStateWhenMachineDisabled;

    public override void activate()
    {
        base.activate();

        if (m_bActivateMachine)
        {
            m_oMachine.get().gameObject.SetActive(true);
            m_oMachine.get().activate();
        }
    }

    public override void update()
    {
        base.update();

        m_oMachine.get().update();
        if (!m_oMachine.get().isActiveAndEnabled)
        {
            toNextState();
        }
    }

    public override void fixedUpdate()
    {
        base.update();

        m_oMachine.get().fixedUpdate();
    }

    public override void lateUpdate()
    {
        base.update();

        m_oMachine.get().lateUpdate();
    }


    protected void toNextState()
    {
        if (m_oToStateWhenMachineDisabled != null)
        {
            m_oToStateWhenMachineDisabled.changeToMe();
        }
    }
}
