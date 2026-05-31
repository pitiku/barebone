using Sirenix.OdinInspector;
using System;
using UnityEngine;
using static HOCUtils;

#if UNITY_EDITOR
#endif

[Serializable]
public class TimedEvent : MonoBehaviour
{
#if UNITY_EDITOR
    [LineHeaderGroup("blue", 2.0f, "getLineColor", "getComment", "setComment")]
#endif

    [HorizontalGroup("first", Width = 48, MarginRight = 3), LabelText("@m_bStartOrEnd?\"Start\":\"End\""), LabelWidth(28)]
    public bool m_bStartOrEnd = true;
    [HorizontalGroup("first", Width = 65, MarginRight = 3), LabelText("time"), LabelWidth(28)]
    public float m_fTime;

    [NonSerialized] public bool m_bPlayed;
    [NonSerialized] public State m_oState;

    [HideInInspector][SerializeField] string m_sComment;

    private bool m_bInitialized = false;

    void Awake()
    {
        initialize();
    }

    // ONLY HERE TO ALLOW THE ENABLED CHECKBOX OF STATEBEHAVIORs
    void Start() { }

    public virtual void initialize() { m_bInitialized = true; }

    public virtual void play()
    {
        if (!m_bInitialized)
        {
            initialize();
        }

        m_bPlayed = true;
    }
    
    public virtual Color getLineColor()
    {
        return new Color(.3f, .9f, 1.0f);
    }

    public virtual bool isPlayOnLoad()
    {
        return true;
    }

    public virtual void playLoading()
    {
        play();
    }

    public void setComment(string _sComment)
    {
        m_sComment = _sComment;
    }

    public string getComment()
    {
        //if (GetComponent<SequencedState>() is SequencedState)
        //{
        //    return (m_bPlayed ? "­­(X) " : "(O) ")+ m_sComment;
        //}
        //else
        {
            return m_sComment;
        }
    }
    public virtual void playWithArgs<T>(T timedEventParams) where T : class, ICustomEventParams
    {
        play();

        if (!m_bInitialized) initialize();

        m_bPlayed = true;
    }
}
