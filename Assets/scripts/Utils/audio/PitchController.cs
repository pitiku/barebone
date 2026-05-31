using UnityEngine;

public class PitchController : MonoBehaviour
{
    [SerializeField] AudioSource m_oSource;
    float m_fPitch;
    bool m_bAdaptToTimeScale = false;

    public void init(float _fPitch, bool _bAdaptToTimeScale)
    {
        m_fPitch = _fPitch;
        m_bAdaptToTimeScale = _bAdaptToTimeScale;
    }

    private void Update()
    {
        if (m_bAdaptToTimeScale)
        {
            m_oSource.pitch = m_fPitch * Time.timeScale;
        }
    }
}
