using System.Collections.Generic;
using UnityEngine;

public static class HOCUtils
{
    // This function only gets a target, searches for the only state at the higher level that is active and makes a transition from it to the new one
    public static void changeLonelyState(State _oTarget, State _oChanger = null, bool _bForce = false, bool _bUpdateImmediately = false)
    {
        //check if parent is active
        State oTargetStateParent = _oTarget.m_oParentState;
        while (oTargetStateParent != null && (!oTargetStateParent.gameObject.activeSelf || !oTargetStateParent.enabled))
        {
            oTargetStateParent = oTargetStateParent.m_oParentState;
        }

        if (oTargetStateParent != null)
        {
            State oFrom = null;
            foreach (State oBrother in oTargetStateParent.getStates())
            {
                if (oBrother.enabled)
                {
                    if (oFrom != null)
                    {
                        Deb.logError("changeLonelyState (" + _oTarget.m_oStateMachine.name + ") to " + _oTarget + " with parent " + _oTarget.m_oParentState + " has more than one active state! Those states are " + oFrom.name + " and " + oBrother.name);
                        return;
                    }
                    else
                    {
                        oFrom = oBrother;
                    }
                }
            }

            if (oFrom == null)
            {
                Deb.logWarning("changeLonelyState to " + _oTarget + " with parent " + _oTarget.m_oParentState + " currently has no active state!");
                changeState(oTargetStateParent, _oTarget, _oChanger, _bForce, _bUpdateImmediately);
            }
            else
            {
                changeState(oFrom, _oTarget, _oChanger, _bForce, _bUpdateImmediately);
            }
        }
        else
        {
            Deb.logWarning("changeLonelyState to " + _oTarget + " with parent " + _oTarget.m_oParentState + " currently has no active parent!");
        }
    }

    static List<State> m_aoStatesToActivate = new List<State>(10);
    public static void changeState(State _oFrom, State _oTarget, State _oChanger = null, bool _bForce = false, bool _bUpdateImmediately = false)
    {
#if DEBUG
        DebugChangeState(_oFrom, _oTarget);
#endif

        // check if this state is currently uninterruptible, and save it to a queue to add it later
        if (!_bForce)
        {
            State oUninterruptable = _oFrom.isUninterruptable(_oTarget.m_iInterruptPriority);
            // if some state inside the uninterruptable state is calling the change state, lets do it
            if (oUninterruptable != null && (_oChanger == null || !_oChanger.isOrHasAsParent(oUninterruptable)))
            {
                oUninterruptable.cleanOnTransition();

                InterruptionData oData = new InterruptionData();
                oData.m_oFrom = _oFrom;
                oData.m_oTarget = _oTarget;
                oUninterruptable.m_oStateMachine.setUninterruptableTransition(oUninterruptable, oData);
                return;
            }
        }

        // special check
        if (!_oFrom.enabled)
        {
            Deb.logError("changeState from " + _oFrom + " to " + _oTarget + " with parent " + _oTarget.m_oParentState + " has FROM state disabled, it may lead to having 2 states active at same time where only 1 should be!");
        }

        // sometimes from state is not at the same hierarchy level as the target, so check the most high hierarchy state affected by from
        State oHighestFrom = highestHierarchyFromAffectedByTransition(_oFrom, _oTarget);

        if (oHighestFrom == null) // if highestFrom is null...
        {
            if (_oTarget.isOrHasAsParent(_oFrom)) // ...and target is child of from it means there are no activated childs of from (otherwise highest form would have returned something)
            {
                if (_oFrom == _oTarget.m_oParentState) // if from is the parent of target, we dont need to activate a entire hierarchy, so finish here
                {
                    _oTarget.activate();
                }
                // set the target as a son of highest from to activate the hierarchy if needed
                oHighestFrom = _oFrom.getStates()[0];
            }
            else
            {
                Deb.logError("trying to change state from " + _oFrom + " to " + _oTarget + " but they are not in the same hierarchy.");
            }
        }
        else
        {
            // check if there is a interruption planned when exiting this state
            if (oHighestFrom.checkTransitionEvent())
            {
                return;
            }

            oHighestFrom.deactivate();
        }

        // check if we need to activate a hierarchy
        if (_oTarget.isBrother(_oFrom))
        {
            _oTarget.activate();
        }
        else
        {
            m_aoStatesToActivate.Clear();

            State oTargetState = _oTarget;
            while (oTargetState.m_oParentState != oHighestFrom.m_oParentState)
            {
                m_aoStatesToActivate.Add(oTargetState);
                oTargetState = oTargetState.m_oParentState;
            }

            // activate all the states in reverse order, from root to leaves
            oTargetState.activate(false);
            for (int i = m_aoStatesToActivate.Count - 1; i >= 0; --i)
            {
                m_aoStatesToActivate[i].activate(false);
            }
        }

        if (_bUpdateImmediately || _oTarget.m_bUpdateImmediately)
        {
            _oTarget.update();
            _oTarget.fixedUpdate();
        }
    }

    static void checkUpdateImmediately(State _oTarget, bool _bUpdateImmediately)
    {
        if (_bUpdateImmediately || _oTarget.m_bUpdateImmediately)
        {
            _oTarget.update();
            _oTarget.fixedUpdate();
        }
    }

    public static State highestHierarchyFromAffectedByTransition(State _oFrom, State _oTarget)
    {
        // look for the common parent shared by the two states
        State oFrom = _oFrom;
        while (oFrom.m_oParentState != null)
        {
            State oTarget = _oTarget;
            while (oTarget.m_oParentState != null)
            {
                if (oTarget.m_oParentState == oFrom.m_oParentState)
                {
                    return oFrom;
                }
                oTarget = oTarget.m_oParentState;
            }
            oFrom = oFrom.m_oParentState;
        }

        return null;
    }

    public static void changeLonelyState(this StateMachine _oMachine, string _sState, bool _bForce = false)
    {
        changeLonelyState(_oMachine.getStateByName(_sState), null, _bForce);
    }

    private static void DebugChangeState(State _oFrom, State _oTarget)
    {
        if (_oTarget.m_oParentState.m_bDebugState && _oFrom != _oTarget)
        {
            Deb.log("[" + Time.realtimeSinceStartup.toString() + "] " + _oTarget.m_oParentState.name + ": " + _oFrom.name + " -> " + _oTarget.name);
        }
    }

    // gets all the components of the type T that are children of parent with T2, not other T2 parents
    public static void getComponentsInChildrenOfThisParent<T, T2>(Transform _oParent, ref List<T> _ao) where T : Component where T2 : Component
    {
        T[] oComponents = _oParent.transform.GetComponents<T>();
        for (int i = 0; i < oComponents.Length; ++i)
        {
            _ao.Add(oComponents[i]);
        }

        // then do a pass to call recursively
        foreach (Transform t in _oParent.transform)
        {
            T2 oComponent2 = t.GetComponent<T2>();
            if (oComponent2 == null)
            {
                getComponentsInChildrenOfThisParent<T, T2>(t, ref _ao);
            }
        }
    }

    // same but instead of filling the list, return an array (this method uses the previous one recursively)
    public static T[] getComponentsInChildrenOfThisParent<T, T2>(Transform _oParent) where T : Component where T2 : Component
    {
        List<T> ao = new List<T>();
        getComponentsInChildrenOfThisParent<T, T2>(_oParent, ref ao);
        return ao.ToArray();
    }

    // play all the events found in target game object
    public static void playHOCEvents(this GameObject _o, bool _bIncludeChildren = false)
    {
        if (_o == null)
        {
            return;
        }

        TimedEvent[] aoTE;
        if (_bIncludeChildren)
        {
            aoTE = _o.GetComponentsInChildren<TimedEvent>();
        }
        else
        {
            aoTE = _o.GetComponents<TimedEvent>();
        }

        for (int i = 0; i < aoTE.Length; ++i)
        {
            if (aoTE[i].enabled)
            {
                if (aoTE[i].m_bStartOrEnd && aoTE[i].m_fTime > 0.0f)
                {
                    GameStateManager.Instance.addDelayedEvent(aoTE[i], aoTE[i].m_fTime);
                }
                else
                {
                    aoTE[i].play();
                }
            }
        }
    }

    public interface ICustomEventParams { }

    // By Diego, quizas ya hay una manera para hacer esto, pero tampoco he visto UN evento usando parametros externos, es decir, quizas necesitamos disparar un evento que dependa que necesite saber unos valores
    // ej: cuando el jugador recive un pase se activa el evento de paso y yo necesito acceder a la distancia que ha recorrido la bola, es mas sencillo enviar este dato como parametro que rebuscar entre las entidades,
    // igualmente playWithArgs(); activará tambien los eventos que sean normales, es decir, play();

    // ReSharper disable Unity.PerformanceAnalysis
    public static void PlayEventsWithArgs<T>(this GameObject _o, T timedEventParams, bool _bIncludeChildren = false) where T : class, ICustomEventParams
    {
        if (timedEventParams == null) Deb.log("TimedEventParams is null");
        if (_o == null)
        {
            return;
        }

        TimedEvent[] aoTE;
        if (_bIncludeChildren)
        {
            aoTE = _o.GetComponentsInChildren<TimedEvent>();
        }
        else
        {
            aoTE = _o.GetComponents<TimedEvent>();
        }

        for (int i = 0; i < aoTE.Length; ++i)
        {
            if (aoTE[i].enabled)
            {
                if (aoTE[i].m_bStartOrEnd && aoTE[i].m_fTime > 0.0f)
                {
                    GameStateManager.Instance.addDelayedEvent(aoTE[i], aoTE[i].m_fTime);
                }
                else
                {
                    //aoTE[i].play();
                    aoTE[i].playWithArgs<T>(timedEventParams);
                }
            }
        }
    }


    public static void play<T>(this T[] _aoEvents) where T : TimedEvent
    {
        for (int i = 0; i < _aoEvents.Length; ++i)
        {
            _aoEvents[i].play();
        }
    }

    public static void play<T>(this List<T> _aoEvents) where T : TimedEvent
    {
        for (int i = 0; i < _aoEvents.Count; ++i)
        {
            _aoEvents[i].play();
        }
    }

    public static State isUninterruptable(this State _oState, int _iPriority)
    {
        // check if the state has more priority than the new interruption priority
        if (_oState.m_iInterruptPriority > _iPriority)
        {
            return _oState;
        }

        // else check in children states
        List<State> aoChildren = _oState.getStates();
        for (int i = 0; i < aoChildren.Count; ++i)
        {
            if (aoChildren[i].enabled)
            {
                State oUninterruptable = aoChildren[i].isUninterruptable(_iPriority);
                if (oUninterruptable != null)
                {
                    return oUninterruptable;
                }
            }
        }

        return null;
    }

    public static void linkStates(this State _oParent, GameObject _oContainer, bool _bActivate = true, bool _bSetAsChildTransform = false)
    {
        // check if container is a state
        State oState = _oContainer.GetComponent<State>();
        bool bCheckBrothers = false;
        if (oState == null)
        {
            // if null, try to find a state in its children
            oState = _oContainer.transform.GetComponentInChildren<State>();
        }

        if (oState != null)
        {
            // check if the parent is a state to add its brothers too
            if (oState.transform.parent != null && oState.transform.parent.GetComponent<State>() != null)
            {
                bCheckBrothers = true;
            }

            linkStates(_oParent, oState, _bActivate, _bSetAsChildTransform, bCheckBrothers);
            return;
        }
    }

    public static void linkStates(this State _oParent, State _oChild, bool _bActivate = true, bool _bSetAsChildTransform = false, bool _bCheckBrothers = true)
    {
        // if child state doesnt have brothers
        if (_oChild.transform.parent == null || !_bCheckBrothers)
        {
            _oParent.addState(_oChild, _bSetAsChildTransform);
        }
        // else it has brothers to also link them to the parent
        else
        {
            State[] aoStates = _oChild.transform.parent.getComponentsInChildrenLevels<State>(1);
            for (int i = 0; i < aoStates.Length; ++i)
            {
                _oParent.addState(aoStates[i], _bSetAsChildTransform);
                _oParent.m_abActiveStatesInUpdate.Add(false);
                //_oParent.getStates()[i].initialize(_bForceInitialize);
            }
        }

        if (_bActivate)
        {
            _oChild.activate();
        }
    }

    public static void unlinkStates(this State _oParent, bool _bDeactivate = true, bool _bMoveTransform = false, Transform _oUnlinkTo = null)
    {
        List<State> aoStates = _oParent.getStates();
        for (int i = 0; i < aoStates.Count; ++i)
        {
            if (_bDeactivate)
            {
                aoStates[i].deactivate();
            }

            if (_bMoveTransform)
            {
                aoStates[i].transform.SetParent(_oUnlinkTo);
            }
        }

        aoStates.Clear();
    }

    public static State getState(string _sName)
    {
        return Utils.getComponent<State>(_sName);
    }
}