using Sirenix.OdinInspector;
using UnityEngine;

public enum eParticleBehaviour
{
    FREEZE,
    KILL
}

public enum eCheckParticleSystem
{
    KILL
}

[System.Serializable]
public class ParticleBehaviour
{
    [HorizontalGroup("0", Width = -1, MarginRight = 3), LabelText("Particle"), LabelWidth(50)]
    public GameObject m_oParticles;

    [HorizontalGroup("1", Width = 150, MarginRight = 3), LabelText("Behaviour"), LabelWidth(70), ShowIf("@this.m_oParticles.GetComponent<ParticleSystem>() != null")]
    public eParticleBehaviour m_oParticleBehaviour;

    [HorizontalGroup("1", Width = 150, MarginRight = 3), LabelText("Behaviour"), LabelWidth(70), ShowIf("@this.m_oParticles.GetComponent<ParticleSystem>() == null")]
    public eCheckParticleSystem m_oParticleBehaviourr = eCheckParticleSystem.KILL;

    [HorizontalGroup("1", Width = 100, MarginRight = 3), LabelText("Seconds"), LabelWidth(50)]
    public float m_fTime = 2.0f;
    public Timer m_oTimer;

    public void start()
    {
        m_oTimer = new Timer(m_fTime, eType.StopWhenPaused);
        m_oTimer.start();
    }
    public void doBehaviour()
    {
        if (m_oParticles.GetComponent<ParticleSystem>() != null)
        {
            if (m_oParticleBehaviour == eParticleBehaviour.FREEZE)
            {
                m_oParticles.GetComponent<ParticleSystem>().Pause();
            }
            else
            {
                GameObject.Destroy(m_oParticles.gameObject);
            }
        }
        else
        {
            GameObject.Destroy(m_oParticles.gameObject);
        }
        m_oTimer.deactivate();
    }
}
