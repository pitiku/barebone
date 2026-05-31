using System;
using System.Collections.Generic;
using UnityEngine;

public class ShakeManager : SceneSingleton<ShakeManager>
{
    List<ShakeBehaviour> m_aoShakeBehaviours = new List<ShakeBehaviour>();
    [NonSerialized] public Vector3 m_vInitialManagementCameraPos;
    [NonSerialized] public Vector3 m_vInitialCombatCameraPos;

    public void addShake(ShakeBehaviour oS)
    {
        m_aoShakeBehaviours.Add(oS);
    }

    public void cancelShakes()
    {
        for (int i = 0; i < m_aoShakeBehaviours.Count; i++)
        {
            m_aoShakeBehaviours[i].cancelShake();
            m_aoShakeBehaviours.RemoveAt(i);
            i--;
        }
    }
}
