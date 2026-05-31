using UnityEngine;

public class ParticlesFreezer : MonoBehaviour
{
    public ParticleSystem m_oParticles;
    public float m_fTime = 2.0f;

    void Update()
    {
        m_fTime -= Time.deltaTime;
        if (m_fTime <= 0.0f)
        {
            m_oParticles.Pause();
            Destroy(this);
        }
    }
}
