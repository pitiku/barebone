using UnityEngine;

[System.Serializable]
public class Velocity2
{
    public float m_fFriction;
    [System.NonSerialized] public Vector2 m_vValue;

    public void setVelocity(Vector2 _v)
    {
        m_vValue = _v;
    }

    public Vector2 updateIncrement(float _fDT, Vector2 _vAcceleration)
    {

        //float E = Mathf.Exp(-m_fFrictionMass * _fDT) - 1;
        //return _vAcceleration * _fDT / m_fFrictionMass - (m_vValue - _vAcceleration / m_fFrictionMass) * E / m_fFrictionMass;

        m_vValue += _vAcceleration * _fDT;
        m_vValue -= m_vValue * m_fFriction;
        return m_vValue * _fDT;
    }
}
