using Sirenix.OdinInspector;
using UnityEngine;

public class ChangeState : TimedEvent
{
    [HorizontalGroup("first", Width = 50), LabelWidth(30), LabelText("force")]
    public bool m_bForce = false;
    [Tooltip("if fromState is null, the parent of targetState object will changeState() to it")]
    [HorizontalGroup("2", Width = 180), LabelWidth(28), LabelText("from")]
    public State m_oFromState;
    [HorizontalGroup("2", Width = 180), LabelWidth(12), LabelText("to")]
    [Required]
    public State m_oTargetState;
    [HorizontalGroup("first", Width = 75), LabelWidth(25), LabelText("start")]
    public float m_fStartTime = 0.0f;

    public override void play()
    {
        base.play();

        //DebugManager.Instance.assert(m_oTargetState != null, "state " + m_oState.gameObject.name + " of StateMachine " + m_oOwner.gameObject.name + " has a ForceTransition component with targetState set to null!!!");

        if (m_oFromState != null)
        {
            HOCUtils.changeState(m_oFromState, m_oTargetState, m_oState, m_bForce);
        }
        else
        {
            HOCUtils.changeLonelyState(m_oTargetState, m_oState, m_bForce);
        }

        // add stat time (in case update immediately was activated and a first update has been executed)
        m_oTargetState.m_fTimer += m_fStartTime;
        m_oTargetState.m_fFixedTimer += m_fStartTime;
    }

    public override bool isPlayOnLoad()
    {
        return false;
    }
}
