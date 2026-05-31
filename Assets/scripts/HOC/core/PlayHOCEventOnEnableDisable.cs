using UnityEngine;

public class PlayHOCEventOnEnableDisable : MonoBehaviour
{
    [SerializeField] GameObject m_oOnEnable;
    [SerializeField] GameObject m_oOnDisable;

    private void OnEnable()
    {
        m_oOnEnable.playHOCEvents();
    }

    private void OnDisable()
    {
        m_oOnDisable.playHOCEvents();
    }

}
