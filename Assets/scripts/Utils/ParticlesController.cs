using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    public List<ParticleBehaviour> m_oParticlesBehaviours = new List<ParticleBehaviour>();


    private void Start()
    {
        foreach (ParticleBehaviour item in m_oParticlesBehaviours)
        {
            item.start();
        }
    }

    private void Update()
    {
        for (int i = 0; i < m_oParticlesBehaviours.Count; i++)
        {
            if (m_oParticlesBehaviours[i].m_oParticles == null)
            {
                m_oParticlesBehaviours.RemoveAt(i);
                i--;
            }
            else if (m_oParticlesBehaviours[i].m_oTimer.isActive() && m_oParticlesBehaviours[i].m_oTimer.isFinished())
            {
                m_oParticlesBehaviours[i].doBehaviour();
            }
        }
    }
}