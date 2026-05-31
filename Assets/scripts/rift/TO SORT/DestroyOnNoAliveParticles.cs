using UnityEngine;
using UnityEngine.VFX;

public class DestroyOnNoAliveParticles : MonoBehaviour
{
    private VisualEffect m_oVisualEffect;

    void Start()
    {
        m_oVisualEffect = GetComponentInChildren<VisualEffect>();
    }

    void Update()
    {
        if (m_oVisualEffect != null && m_oVisualEffect.aliveParticleCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
