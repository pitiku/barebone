using UnityEngine;

public class TransitionOnGameobjectDisabled : StateTransition
{
    [SerializeField] GameObject m_oGameObject;

    public override bool update()
    {
        return !m_oGameObject.activeSelf;
    }
}
