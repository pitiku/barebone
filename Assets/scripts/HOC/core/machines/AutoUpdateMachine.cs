using UnityEngine;

public class AutoUpdateMachine : MonoBehaviour
{
    public HOCComponentStateMachine m_oTarget;

    void Awake()
    {
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
