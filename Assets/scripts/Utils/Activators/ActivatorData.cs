using System;
using UnityEngine;

[Serializable]
public class ActivatorData
{
    public GameObject[] m_aoObjects;
    public Behaviour[] m_aoBehaviours;

    public void activate(bool _b)
    {
        for (int i = 0; m_aoObjects.Length > i; i++)
        {
            if (m_aoObjects[i] != null)
                m_aoObjects[i].SetActive(_b);
        }
        for (int i = 0; m_aoBehaviours.Length > i; i++)
        {
            if (m_aoBehaviours[i] != null)
                m_aoBehaviours[i].enabled = _b;
        }
    }

    public void destroy(bool _bImmediately)
    {
        for (int i = 0; m_aoObjects.Length > i; i++)
        {
            if (m_aoObjects[i] != null)
            {
                if (_bImmediately)
                    GameObject.DestroyImmediate(m_aoObjects[i]);
                else
                    GameObject.Destroy(m_aoObjects[i]);
            }
        }
        for (int i = 0; m_aoBehaviours.Length > i; i++)
        {
            if (m_aoBehaviours[i] != null)
            {
                if (_bImmediately)
                    GameObject.DestroyImmediate(m_aoBehaviours[i]);
                else
                    GameObject.Destroy(m_aoBehaviours[i]);
            }
        }
    }
}