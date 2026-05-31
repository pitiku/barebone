
using UnityEngine;
using UnityEngine.Serialization;

public class PlatformDependantGameObject : MonoBehaviour
{
    [FormerlySerializedAs("m_pc")] public bool m_bPc;
    [FormerlySerializedAs("m_ps5")] public bool m_bPs5;
    [FormerlySerializedAs("m_ps4")] public bool m_bPs4;
    [FormerlySerializedAs("m_switch")] public bool m_bSwitch;
    [FormerlySerializedAs("m_xboxone")] public bool m_bXboxOne;
    [FormerlySerializedAs("m_xboxseries")] public bool m_bXboxSeries;

    void Awake()
    {
#if UNITY_GAMECORE_XBOXSERIES
        gameObject.SetActive(m_bXboxSeries);
#elif UNITY_GAMECORE_XBOXONE || UNITY_XBOXONE
        gameObject.SetActive(m_bXboxOne);
#elif UNITY_PS5
        gameObject.SetActive(m_bPs5);
#elif UNITY_PS4
        gameObject.SetActive(m_bPs4);
#elif UNITY_SWITCH
        gameObject.SetActive(m_bSwitch);
#else
        gameObject.SetActive(m_bPc);
#endif
    }
}
