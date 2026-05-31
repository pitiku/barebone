using StateChangeCollapse;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    Transform m_oParent;

    [Sirenix.OdinInspector.ReadOnly]
    public bool m_bPooled = false;

    public virtual void initialize()
    {
        m_oParent = transform.parent;
    }

    public virtual void returnToPool()
    {
        transform.SetParent(m_oParent);
        transform.setLocalPosXZ(Vector2.zero);
        m_bPooled = false;
        gameObject.setActiveDelayed(false);
    }

    public void resetPos()
    {
        transform.setLocalPosXZ(Vector2.one * -500);
    }
}
