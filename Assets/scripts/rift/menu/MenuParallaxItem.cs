using UnityEngine;

public class MenuParallaxItem : MonoBehaviour
{
    public float m_fValue = 1;

    Vector3 m_vInitialPosition;

    public void initialize()
    {
        m_vInitialPosition = transform.localPosition;
    }

    public void updateMovement(Vector2 _vMouse)
    {
        transform.localPosition = m_vInitialPosition - (_vMouse.toVector3xy() * m_fValue);
    }
}
