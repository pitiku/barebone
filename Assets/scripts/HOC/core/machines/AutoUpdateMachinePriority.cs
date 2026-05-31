using UnityEngine;

public class AutoUpdateMachinePriority : MonoBehaviour
{
    public HOCComponentStateMachine m_oTarget;

    void Start()
    {
        // activate this in Start so Awake of all states (including target machine) are executed and the states are initialized
        m_oTarget.get().activate();
    }

    void Update()
    {
        m_oTarget.get().update();
    }

    void FixedUpdate()
    {
        m_oTarget.get().fixedUpdate();
    }

    void LateUpdate()
    {
        m_oTarget.get().lateUpdate();
    }
}
