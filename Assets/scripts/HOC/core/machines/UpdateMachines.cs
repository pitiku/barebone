
[System.Serializable]
public class UpdateMachines : Behavior
{
    public bool m_bActivateMachine = false;
    public HOCComponentStateMachine[] m_oMachines;

    public override void activate()
    {
        base.activate();

        if (m_bActivateMachine)
        {
            for (int iIndex = 0; iIndex < m_oMachines.Length; ++iIndex)
            {
                m_oMachines[iIndex].get().activate();
            }
        }
    }

    public override void update()
    {
        base.update();

        for (int iIndex = 0; iIndex < m_oMachines.Length; ++iIndex)
        {
            m_oMachines[iIndex].get().update();
        }
    }

    public override void fixedUpdate()
    {
        base.update();

        for (int iIndex = 0; iIndex < m_oMachines.Length; ++iIndex)
        {
            m_oMachines[iIndex].get().fixedUpdate();
        }
    }

    public override void lateUpdate()
    {
        base.update();

        for (int iIndex = 0; iIndex < m_oMachines.Length; ++iIndex)
        {
            m_oMachines[iIndex].get().lateUpdate();
        }
    }
}
