using System.Collections.Generic;

[System.Serializable]
public class InterruptionData
{
    public State m_oFrom;
    public State m_oTarget;
}

[System.Serializable]
public class StateMachine : State
{
    public Dictionary<State, InterruptionData> m_aoInterruptionQueue = new Dictionary<State, InterruptionData>();

    protected virtual void onUninterruptableAfterTransition(State _oDeactivatedState)
    {
        if (!m_aoInterruptionQueue.ContainsKey(_oDeactivatedState))
        {
            return;
        }

        InterruptionData oData = m_aoInterruptionQueue[_oDeactivatedState];
        m_aoInterruptionQueue.Remove(_oDeactivatedState);
        HOCUtils.changeState(oData.m_oFrom, oData.m_oTarget, null, true);
    }

    public void setUninterruptableTransition(State _oState, InterruptionData _oData)
    {
        _oState.onTransition += onUninterruptableAfterTransition;
        m_aoInterruptionQueue[_oState] = _oData;
    }
}
