using UnityEngine;

public class TransitionOnStateEnabled : StateTransition
{
    [SerializeField] bool m_bEnabled;
    public State m_oState;

    public override bool update()
    {
        return m_oState.isActiveAndEnabled == m_bEnabled;
    }
}