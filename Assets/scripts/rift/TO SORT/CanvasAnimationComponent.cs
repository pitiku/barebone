using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class CanvasAnimationComponent : MonoBehaviour
{
    private enum eState { Enabling, Enabled, Disabling, Disabled }

    [SerializeField] Animator m_oAnimator;

    [ValueDropdown("getAnimations", DropdownWidth = 250)]
    [SerializeField] string m_sOnEnableAnimationState;

    [ValueDropdown("getAnimations", DropdownWidth = 250)]
    [SerializeField] string m_sOnDisableAnimationState;

    [SerializeField] bool m_bActivateOnAwake = false;
    [SerializeField] bool m_bActivateOnEnable = false;
    [SerializeField] bool m_bDeactivateChilds = false;

    public bool m_bIsBeingDisabled;

    private void Start()
    {
        if (m_bActivateOnAwake)
        {
            activate();
        }
    }

    private void OnEnable()
    {
        if (m_bActivateOnEnable)
        {
            activate();
        }
    }

    public void activate()
    {
        m_bIsBeingDisabled = false;
        m_oAnimator.play(m_sOnEnableAnimationState);
    }

    private void Update()
    {
        if (!m_bIsBeingDisabled) { return; }

        if (m_oAnimator.GetCurrentAnimatorStateInfo(0).IsName(m_sOnDisableAnimationState) &&
    m_oAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.999999f)
        {
            // Animation has finished or is about to finish
            gameObject.SetActive(false);
        }
    }

    [Button("deactivate")]
    public void deactivate()
    {
        if (m_bDeactivateChilds)
        {
            CanvasAnimationComponent[] aoChilds = GetComponentsInChildren<CanvasAnimationComponent>();
            for (int i = 0; i < aoChilds.Length; i++)
            {
                if (aoChilds[i] != this)
                {
                    aoChilds[i].deactivate();
                }
            }
        }

        m_bIsBeingDisabled = true;
        float fCurrentTime = 0;
        if (IsCurrentStateOnEnable())
        {
            fCurrentTime = 1 - Mathf.Min(m_oAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1);
        }

        m_oAnimator.play(m_sOnDisableAnimationState, false, 0, fCurrentTime);
    }

    bool IsCurrentStateOnEnable()
    {
        if(m_oAnimator.runtimeAnimatorController == null)
        {
            Deb.log("[CanvasAnimationComponent] Animator has no animator component assigned.", eLogFlags.SYSTEM, gameObject);
        }
        return m_oAnimator.gameObject.activeInHierarchy && m_oAnimator.GetCurrentAnimatorStateInfo(0).IsName(m_sOnEnableAnimationState);
    }

    [Button("activate")]
    public void setActive()
    {
        gameObject.setActive(true);
    }

#if UNITY_EDITOR
    IEnumerable getAnimations()
    {
        List<string> aoStates = new List<string>();

        if (m_oAnimator != null)
        {
            AnimatorController ac = m_oAnimator.runtimeAnimatorController as AnimatorController;

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

    public bool IsBeingDisabled { get { return m_bIsBeingDisabled; } }
}
