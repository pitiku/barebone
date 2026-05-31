using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StateChangeCollapse;


#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class PlayAnimationState : TimedEvent
{
    public enum eState { Tag, Reference, IGameEntity }

    [HorizontalGroup("first", Width = 150), HideLabel, ShowIf("@m_eType == eState.Reference")]
    public Animator m_oAnimator;
    [NonSerialized] Animator m_oAnimatorTemp;
    [HorizontalGroup("first", Width = 250), LabelWidth(18), LabelText("->")]
    [ValueDropdown("getAnimations", DropdownWidth = 250)]
    public string m_sAnimationState;

    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;

    public override void play()
    {
        base.play();

        if (getAnimator() == null)
        {
            Deb.logWarning("Animator is null for PlayAnimationState event. Please check the setup.");
            return;
        }
        getAnimator().setAnimationDelayed(m_sAnimationState);
    }

    Animator getAnimator()
    {
        if (m_oAnimatorTemp == null)
        {
            if (m_eType == eState.Tag)
            {
                GameObject oGO = GameplayManager.Instance.getGameobject(m_oTag.Value);
                m_oAnimatorTemp = oGO.GetComponent<Animator>();
                if (m_oAnimatorTemp == null)
                {
                    m_oAnimatorTemp = oGO.GetComponentInChildren<Animator>(true);
                }
            }
            else if (m_eType == eState.Reference)
            {
                m_oAnimatorTemp = m_oAnimator;
            }
        }
        return m_oAnimatorTemp;
    }

#if UNITY_EDITOR

    IEnumerable getAnimations()
    {
        List<string> aoStates = new List<string>();

        if (getAnimator() != null)
        {
            AnimatorController ac = getAnimator().runtimeAnimatorController as AnimatorController;

            foreach (AnimatorControllerLayer acl in ac.layers)
            {
                foreach (ChildAnimatorState cas in acl.stateMachine.states)
                {
                    aoStates.Add(cas.state.name);
                }

                foreach (ChildAnimatorStateMachine cas in acl.stateMachine.stateMachines)
                {
                    aoStates.Add(cas.stateMachine.name);
                }
            }
        }

        return aoStates;
    }
#endif
}
