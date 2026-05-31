public class TransitionOnMethod : StateTransition
{
    public HOCComponentGeneric m_oHOCComponent;
    public string m_sMethod;
    public bool m_bResult;

    public override bool update()
    {
        return (bool)m_oHOCComponent.get().GetType().GetMethod(m_sMethod).Invoke(m_oHOCComponent.get(), null) == m_bResult;
    }
}

