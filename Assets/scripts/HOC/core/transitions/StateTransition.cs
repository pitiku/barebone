using Sirenix.OdinInspector;
using UnityEngine;

public class StateTransition : MonoBehaviour
{
#if UNITY_EDITOR
    [LineHeaderGroup("green", 2.0f, "getLineColor", "getComment", "setComment")]
#endif

    [HorizontalGroup("first", Width = 220, MarginRight = 5), LabelText("to"), LabelWidth(15)]
    public State m_oTargetState;

    [HideInInspector][SerializeField] string m_sComment;

    // ONLY HERE TO ALLOW THE ENABLED CHECKBOX OF STATEBEHAVIORs
    void Start() { }

    public virtual void activate() { }

    public virtual void deactivate() { }

    public virtual bool update()
    {
        return false;
    }

    public virtual Color getLineColor()
    {
        return new Color(.1f, .9f, .1f);
    }

    public void setComment(string _sComment)
    {
        m_sComment = _sComment;
    }

    public string getComment()
    {
        return m_sComment;
    }
}
