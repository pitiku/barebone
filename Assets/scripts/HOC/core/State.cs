using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void StateHandler(State _oState);

[System.Serializable]
public class State : MonoBehaviour
{
    [LineHeaderGroup("red", 3.0f, 1f, 0f, 0f)]
    [HorizontalGroup("base", Width = 86, MarginRight = 8), LabelText("immediate"), LabelWidth(61)]
    public bool m_bUpdateImmediately = false;

    [HorizontalGroup("base", Width = 70, MarginRight = 8), LabelText("paused"), LabelWidth(45)]
    public bool m_bPaused = false;

    [HorizontalGroup("base", Width = 66, MarginRight = 8), LabelText("debug"), LabelWidth(41)]
    public bool m_bDebugState = false;

    [HorizontalGroup("base", Width = 95, MarginRight = 8), LabelText("save debug"), LabelWidth(70)]
    public bool m_bDebugSaveStates = false;

    [HorizontalGroup("base", Width = 68, MarginRight = 8), LabelText("priority"), LabelWidth(43)]
    public int m_iInterruptPriority = 10;

    [Space]
    public List<State> m_aoDefaultActive = new List<State>();

    protected List<State> m_aoSavedStates = new List<State>();

    public event StateHandler onTransition;

    protected bool m_bStateInitialized = false;

    [HideInInspector] public State m_oParentState;
    [HideInInspector] public State m_oChildStateToActivate = null;

    protected State m_oStackedState;

    [NonSerialized, HideInEditorMode, ShowInInspector] public List<State> m_aoStates = new List<State>();
    [HideInInspector] public List<bool> m_abActiveStatesInUpdate = new List<bool>();
    [HideInInspector] public Behavior[] m_aoBehaviors;
    StateTransition[] m_aoTransitions;
    [HideInInspector] public TimedEvent[] m_aoTimedEvents;
    [HideInInspector] public int m_iTimedEventsLeft = 0;

    [HideInInspector] public float m_fTimer = 0.0f;
    [HideInInspector] public float m_fFixedTimer = 0.0f;

    [HideInInspector] public StateMachine m_oStateMachine;

    [HorizontalGroup("flags", Width = 120, MarginRight = 8), LabelText("DontInitOnAwake"), LabelWidth(110)]
    public bool m_bDontInitializeOnAwake;

    string m_sPath = "";

    // ONLY HERE TO ALLOW THE ENABLED CHECKBOX OF STATEs
    void Start()
    {
#if UNITY_EDITOR
        // when deleting states in awake (when applying version gameobjects/componentes when not in build),
        // the references from those states are stored before they are deleted resulting un null references that need to be handled
        for (int i = 0; i < m_aoStates.Count;)
        {
            if (m_aoStates[i] == null) { m_aoStates.RemoveAt(i); }
            else { i++; }
        }
#endif
    }

    public virtual void Awake()
    {
        //Deb.log("INITIALIZE STATE - " + gameObject.name);
        if (!m_bDontInitializeOnAwake) initialize();
    }

    public void cleanOnTransition()
    {
        onTransition = null;
    }

    public void forceInitialize()
    {
        initialize(true);
    }

    public virtual bool initialize(bool _bForce = false)
    {
#if UNITY_EDITOR
        if (m_bDebugState)
        {
            string breakpoint = gameObject.name;
            Transform tr = transform;
            Transform parent = transform.parent;
        }
#endif

        if (m_bStateInitialized && !_bForce)
        {
            return false;
        }

        m_bStateInitialized = true;

        // deactivate state, as it will be activated during an activate()
        enabled = false;
        m_oStateMachine = transform.GetComponentInParent<StateMachine>();
        m_oParentState = transform.getComponentInParents<State>(false);
        m_aoStates = new List<State>();
        // get child states
        // if it has a default active, link it
        if (!m_aoDefaultActive.isNullOrEmpty() && m_aoDefaultActive[0] != null)
        {
            HOCUtils.linkStates(this, m_aoDefaultActive[0], false);
            // also activate all other default active states
            for (int i = 1; i < m_aoDefaultActive.Count; ++i)
            {
                m_aoDefaultActive[i].activate();
            }
        }
        // else, try to get an immediate children state, and link it
        else
        {
            State[] aoChildren = transform.getComponentsInChildrenLevels<State>(1);
            if (!aoChildren.isNullOrEmpty())
            {
                HOCUtils.linkStates(this, aoChildren[0], false);
            }
        }

        // get state behaviors
        m_aoBehaviors = GetComponents<Behavior>();
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            m_aoBehaviors[i].m_oState = this;
        }

        // get all timed events
        setAllTimedEvents();

        // get transitions
        m_aoTransitions = GetComponents<StateTransition>();

        m_sPath = gameObject.getParentNames();

        return true;
    }

    // stack state stacks a state over this state. That means this state won't be updated, will update the stacked state instead.
    public void stackState(State _oState, bool _bActivate = true)
    {
        m_oStackedState = _oState;
        if (_bActivate)
        {
            _oState.activate();
        }
    }

    public void popState()
    {
        if (m_oStackedState != null)
        {
            m_oStackedState.deactivate();
            m_oStackedState = null;
        }
    }

    public void resetStates()
    {
        // disable all states
        disableStates();

        // enable default children states
        activateDefaultChildren();
    }

    protected void activateDefaultChildren()
    {
        if (m_oChildStateToActivate != null)
        {
            m_oChildStateToActivate.activate();
            m_oChildStateToActivate = null;
        }
        else
        {
            for (int i = 0; i < m_aoDefaultActive.Count; ++i)
            {
                Debug.Assert(m_aoDefaultActive[i] != null, "the state " + name + " has a NULL default state");
                if (m_aoDefaultActive[i].gameObject.activeSelf)
                {
                    m_aoDefaultActive[i].activate();
                }
            }
        }
    }

    public void disableStates()
    {
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            m_aoStates[i].enabled = false;
            m_aoStates[i].disableStates();
        }
    }

    public virtual void activateTimeline(bool _bActivateChildren = true)
    {
        //NOTE: When a State is deactivated before all of its TimedEvents are played m_iTimedEventsLeft and later activated, m_iTimedEventsLeft becomes inconsistent.
        //This is a problem for Timelines, as the value of m_iTimedEventsLeft is checked to find-out whether they have finished.
        //This method exists to fix Timeline-related bugs without impacting other systems that might relay on this behavior.
        m_iTimedEventsLeft = 0;
        activate(_bActivateChildren);
        // Check for disabled events in the timeline
        for (int i = 0; i < m_aoTimedEvents.Length; i++)
        {
            if (!m_aoTimedEvents[i].enabled)
            {
                m_iTimedEventsLeft--;
            }
        }
    }

    public virtual void activate(bool _bActivateChildren = true)
    {
#if UNITY_EDITOR
        if (m_oParentState != null && m_oParentState.m_bDebugSaveStates)
        {
            m_oParentState.m_aoSavedStates.Insert(0, this);
        }

        if (m_bDebugState)
        {
            string breakpoint = gameObject.name;
            Transform tr = transform;
            Transform parent = transform.parent;
        }

#endif

        Deb.log("Activated State: " + m_sPath, eLogFlags.HOC);

        initialize();

        enabled = true;
        m_fTimer = 0.0f;
        m_fFixedTimer = 0.0f;

        // prepare stuff for transitions on this state
        for (int i = 0; i < m_aoTransitions.Length; ++i)
        {
            if (m_aoTransitions[i].enabled)
            {
                m_aoTransitions[i].activate();
            }
        }

        // activate all the behaviors that were not already active
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (m_aoBehaviors[i].enabled)
            {
                m_aoBehaviors[i].activate();
            }
        }

        // reset all timed plays and check if there is any to be played during activation
        for (int i = 0; i < m_aoTimedEvents.Length; ++i)
        {
            if (m_aoTimedEvents[i].enabled && m_aoTimedEvents[i].m_bStartOrEnd && m_aoTimedEvents[i].m_fTime.approximately(0.0f))
            {
                m_aoTimedEvents[i].play();
            }
            else
            {
                ++m_iTimedEventsLeft;
                m_aoTimedEvents[i].m_bPlayed = false;
            }
        }

        // activate its default child states
        if (_bActivateChildren)
        {
            activateDefaultChildren();
        }
    }

    public virtual void deactivate()
    {
#if UNITY_EDITOR
        if (m_bDebugState)
        {
            //int breakpoint = 1;
        }
#endif

        enabled = false;

        // deactivate transitions
        for (int i = 0; i < m_aoTransitions.Length; ++i)
        {
            if (m_aoTransitions[i].enabled)
            {
                m_aoTransitions[i].deactivate();
            }
        }

        // deactivate all the state's behaviors
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (m_aoBehaviors[i].enabled)
            {
                m_aoBehaviors[i].deactivate();
            }
        }

        // check for timed events that should be played at the end, if they aren't played, play them
        for (int i = 0; i < m_aoTimedEvents.Length; ++i)
        {
            if (m_aoTimedEvents[i].enabled && !m_aoTimedEvents[i].m_bStartOrEnd && !m_aoTimedEvents[i].m_bPlayed && m_aoTimedEvents[i].m_fTime.approximately(0.0f))
            {
                m_aoTimedEvents[i].play();
            }
        }

        // if this state is a parent state, deactivate its current child state
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            if (m_aoStates[i].enabled)
            {
                m_aoStates[i].deactivate();
            }
        }
    }

    public virtual void lateUpdate()
    {
        if (m_oStackedState != null)
        {
            m_oStackedState.lateUpdate();
            return;
        }

        if (m_bPaused)
        {
            return;
        }

        // update state behaviors
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (m_aoBehaviors[i].isEnabledAndRunning())
            {
                m_aoBehaviors[i].lateUpdate();
            }
        }

        // check states that are activated now and update them (do it like this to avoid updating states that are activated this frame during any of the updates of the other child states)
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            m_abActiveStatesInUpdate[i] = m_aoStates[i].enabled;
        }
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            if (m_abActiveStatesInUpdate[i])
            {
                m_aoStates[i].lateUpdate();
            }
        }
    }

    public bool isBrother(State _oState)
    {
        Transform oParent = transform.parent;
        foreach (Transform oT in oParent)
        {
            State oMS = oT.GetComponent<State>();
            if (oMS == _oState)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void fixedUpdate()
    {
        if (m_oStackedState != null)
        {
            m_oStackedState.fixedUpdate();
            return;
        }

        if (m_bPaused)
        {
            return;
        }

        m_fFixedTimer += Time.fixedDeltaTime;

        // update state behaviors
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (m_aoBehaviors[i].isEnabledAndRunning())
            {
                m_aoBehaviors[i].fixedUpdate();
            }
        }

        // check states that are activated now and update them (do it like this to avoid updating states that are activated this frame during any of the updates of the other child states)
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            m_abActiveStatesInUpdate[i] = m_aoStates[i].enabled;
        }
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            if (m_abActiveStatesInUpdate[i])
            {
                m_aoStates[i].fixedUpdate();
            }
        }
    }

    public virtual void updateActiveChildren()
    {
        // check states that are activated now and update them (do it like this to avoid updating states that are activated this frame during any of the updates of the other child states)
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            m_abActiveStatesInUpdate[i] = m_aoStates[i].enabled;
        }
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            if (m_abActiveStatesInUpdate[i])
            {
                m_aoStates[i].update();
            }
        }
    }

    public virtual void updateComponents()
    {
        // update state behaviors
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (m_aoBehaviors[i].isEnabledAndRunning())
            {
                m_aoBehaviors[i].update();
            }
        }

        // update timed events
        if (m_iTimedEventsLeft > 0)
        {
            for (int i = 0; i < m_aoTimedEvents.Length; ++i)
            {
                TimedEvent oTA = m_aoTimedEvents[i];
                if (!oTA.m_bPlayed)
                {
                    if (oTA.m_bStartOrEnd)
                    {
                        if (oTA.enabled && m_fTimer >= oTA.m_fTime)
                        {
                            oTA.play();
                            --m_iTimedEventsLeft;
                        }
                    }
                }
            }
        }

        updateActiveChildren();

        // check for state transitions
        for (int i = 0; i < m_aoTransitions.Length; ++i)
        {
            // check if the transition happens...
            if (m_aoTransitions[i].enabled && m_aoTransitions[i].update())
            {
                Debug.Assert(m_aoTransitions[i].m_oTargetState, "A transition in object " + gameObject.name + " is being executed but doesn't have a target state!");
                changeState(m_aoTransitions[i].m_oTargetState, this, false, m_aoTransitions[i].m_oTargetState.m_bUpdateImmediately);
                return;
            }
        }
    }

    public virtual void update()
    {
#if UNITY_EDITOR
        if (m_bDebugState)
        {
            string breakpoint = gameObject.name;
            Transform tr = transform;
            Transform parent = transform.parent;
        }
#endif

        if (m_oStackedState != null)
        {
            m_oStackedState.update();
            return;
        }

        if (m_bPaused)
        {
            return;
        }

        m_fTimer += Time.deltaTime;

        updateComponents();
    }

    public void addState(State _o, bool _bSetAsChild = true)
    {
        if (m_aoStates.Contains(_o)) { return; }

        m_aoStates.Add(_o);
        m_abActiveStatesInUpdate.Add(false);
        if (_bSetAsChild)
        {
            _o.transform.SetParent(transform);
        }
    }

    public void removeState(State _o)
    {
        m_aoDefaultActive.Remove(_o);
        m_aoStates.Remove(_o);
        m_abActiveStatesInUpdate.RemoveAt(m_abActiveStatesInUpdate.Count - 1);
    }

    public List<State> getStates()
    {
        return m_aoStates;
    }

    public State getStateByName(string _sName)
    {
        if (name == _sName)
        {
            return this;
        }

        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            State oResult = m_aoStates[i].getStateByName(_sName);
            if (oResult != null)
            {
                return oResult;
            }
        }

        return null;
    }

    public bool isOrHasAsParent(State _oState)
    {
        if (_oState == this)
        {
            return true;
        }

        if (m_oParentState != null)
        {
            return m_oParentState.isOrHasAsParent(_oState);
        }

        return false;
    }

    public bool checkTransitionEvent()
    {
        // if this state or any of its child had a onTransition event queued, check it
        if (onTransition != null)
        {
            StateHandler oBackup = onTransition;
            onTransition = null;
            oBackup.Invoke(this);
            oBackup = null;
            return true;
        }

        // if this state didn't have any transition set, check for any of its child
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            bool bTransition = m_aoStates[i].checkTransitionEvent();
            if (bTransition)
            {
                return true;
            }
        }

        return false;
    }

    public virtual bool changeState(State state, State _oChanger = null, bool force = false, bool updateInmediately = false)
    {
        if (state.isBrother(this) && isActiveAndEnabled)
        {
            HOCUtils.changeState(this, state, _oChanger, force, updateInmediately);
        }
        else
        {
            HOCUtils.changeLonelyState(state, _oChanger, force, updateInmediately);
        }
        return true;
    }

    public void changeToMe(bool _bForce = false)
    {
        HOCUtils.changeLonelyState(this, this, _bForce);
    }

    public State getActiveChildState()
    {
        State oActive = null;
        for (int i = 0; i < m_aoStates.Count; ++i)
        {
            if (m_aoStates[i].enabled)
            {
                if (oActive != null)
                {
                    Debug.Break();
                    Deb.logError("getActiveChildState found that object " + this + "  has more than one active state being " + oActive + " and " + m_aoStates[i]);
                }
                else
                {
                    oActive = m_aoStates[i];
                }
            }
        }

        if (oActive == null)
        {
            //Deb.logError("getActiveChildState found that " + this + " has no child states!");
        }

        return oActive;
    }

    public virtual void BackToPreviousState()
    {
    }

    public void removeStateFrom(State oS)
    {
        if (m_aoStates.Contains(oS))
        {
            m_aoStates.Remove(oS);
        }
    }

    public bool isInitialized()
    {
        return m_bStateInitialized;
    }

    public virtual void recalculateStates()
    {
        List<State> aoNewStateList = new List<State>();

        State[] aoStates = GetComponentsInChildren<State>();

        for (int i = 0; i < aoStates.Length; i++)
        {
            State itState = aoStates[i];

            if (m_aoStates.Contains(itState))
            {
                aoNewStateList.Add(itState);
            }
        }

        m_aoStates = aoNewStateList;
    }

    public void setAllTimedEvents()
    {
        m_aoTimedEvents = GetComponents<TimedEvent>();
        for (int i = 0; i < m_aoTimedEvents.Length; ++i)
        {
            m_aoTimedEvents[i].m_oState = this;
        }
    }
}