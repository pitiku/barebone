using UnityEngine;

[System.Serializable]
public class Behavior : MonoBehaviour
{
#if UNITY_EDITOR
    [LineHeaderGroup("blue", 2.0f, "getLineColor", "getComment", "setComment")]
#endif
    public bool m_bFinished = false; // TODO @mtorrecillas - LineHeaderGroup needs something public after it, but it won't be shown
    [HideInInspector] public State m_oState;

    private bool m_bInitialized = false;

    [HideInInspector][SerializeField] string m_sComment;

    protected virtual void Awake()
    {
        if (!m_bInitialized)
        {
            initialize();
        }
    }

    // ONLY HERE TO ALLOW THE ENABLED CHECKBOX OF STATEBEHAVIORs
    protected virtual void Start() { }

    public virtual void initialize() { m_bInitialized = true; }

    public virtual void update() { }

    public virtual void fixedUpdate() { }

    public virtual void lateUpdate() { }

    public virtual bool meetActivateConditions()
    {
        return true;
    }

    public virtual void activate()
    {
        if (!m_bInitialized)
        {
            initialize();
        }

        m_bFinished = false;
    }

    public virtual void deactivate()
    {
        m_bFinished = true;
    }

    // UPTODO HOC UPDATE? check if m_bFinished (used in TransitionOnBehaviorEnds) could be deleted using another method to simplify Behavior code. We have been only using this feature super few times
    public bool isEnabledAndRunning()
    {
        return enabled && !m_bFinished;
    }

    public virtual Color getLineColor()
    {
        return new Color(0.3f, 0.3f, 1f);
    }

    public void setComment(string _sComment)
    {
        m_sComment = _sComment;
    }

    public string getComment()
    {
        //if(GetComponent<SequencedState>() != null)
        //{
        //    return (m_bFinished ? "­­(X) " : "(O) ") + m_sComment;
        //}
        //else
        {
            return m_sComment;
        }
    }
}
