using StateChangeCollapse;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    [NonSerialized] public List<PooledObject> m_aoPool = new List<PooledObject>();
    public GameObject m_oObjectToPool;
    public int m_iAmountToPool;

    public virtual void fillPool<T>() where T : PooledObject
    {
        GameObject oObject;

        m_aoPool.RemoveAll(t => t == null);

        for (int i = 0; i < m_iAmountToPool; i++)
        {
            oObject = Instantiate(m_oObjectToPool, transform);
            oObject.name += " " + i.ToString();
            // add component pooledObject if don't have any
            if (oObject.GetComponent<T>() == null)
            {
                oObject.AddComponent<T>();
            }
            // initialize pooledObject
            m_aoPool.Add(oObject.GetComponent<T>());
            m_aoPool[^1].initialize();
            m_aoPool[^1].gameObject.SetActive(false);
        }
    }

    public GameObject getPoolObject(bool _bGetItActive = true)
    {
        for (int i = 0; i < m_aoPool.Count; i++)
        {
            if (m_aoPool[i] != null && !m_aoPool[i].m_bPooled)
            {
                m_aoPool[i].m_bPooled = true;
                if (_bGetItActive) m_aoPool[i].gameObject.setActiveDelayed(true);
                return m_aoPool[i].gameObject;
            }
        }
        fillPool<PooledObject>();
        return getPoolObject(_bGetItActive);
    }

    public List<GameObject> getAllInstantiatedObjects()
    {
        List<GameObject> aoObjects = new();
        for (int i = 0; i < m_aoPool.Count; i++)
        {
            if (m_aoPool[i] != null)
            {
                aoObjects.Add(m_aoPool[i].gameObject);
            }
        }
        return aoObjects;
    }

    public void resetPool()
    {
        for (int i = 0; i < m_aoPool.Count; i++)
        {
            if (m_aoPool[i] != null)
            {
                m_aoPool[i].GetComponent<PooledObject>().returnToPool();
            }
        }
        m_aoPool.RemoveAll(t => t == null);
    }
}