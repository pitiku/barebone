using Sirenix.OdinInspector;
using UnityEngine;

public class SetParticlesEmissionRate : TimedEvent
{
    [HorizontalGroup("first", Width = 160, MarginLeft = 5), LabelText(""), LabelWidth(1)]
    public ParticleSystem m_oParticleSystem;
    [HorizontalGroup("first", Width = 65, MarginLeft = 5), LabelText("time"), LabelWidth(28)]
    public float m_fRateOverTime;
    [HorizontalGroup("first", Width = 90, MarginLeft = 5), LabelText("distance"), LabelWidth(52)]
    public float m_fRateOverDistance;

    public override void play()
    {
        base.play();

        ParticleSystem.EmissionModule oEmission = m_oParticleSystem.emission;
        oEmission.rateOverTime = m_fRateOverTime;
        oEmission.rateOverDistance = m_fRateOverDistance;
    }
}
