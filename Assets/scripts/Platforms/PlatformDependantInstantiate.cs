using UnityEngine;
using UnityEngine.Serialization;

public class PlatformDependantInstantiate : MonoBehaviour
{
    [FormerlySerializedAs("m_pc")] public bool m_bPc;
    public bool m_bPs5;
    [FormerlySerializedAs("m_ps4")] public bool m_bPs4;
    [FormerlySerializedAs("m_switch")] public bool m_bSwitch;
    [FormerlySerializedAs("m_xboxone")] public bool m_bXboxOne;
    public bool m_bXboxSeries;

    public GameObject m_oPrefab;
    public string m_sResource = "";
    public bool m_bWaitForPlatform = false;

    void Awake()
    {
        bool bKeepAlive = false;

#if UNITY_GAMECORE_XBOXSERIES
        if (m_bXboxSeries)
        {
            bKeepAlive = Instantiate();
        }
#elif UNITY_GAMECORE_XBOXONE || UNITY_XBOXONE
        if (m_bXboxOne)
        {
            bKeepAlive = Instantiate();
        }
#elif UNITY_PS5
        if (m_bPs5)
        {
            bKeepAlive = Instantiate();
        }
#elif UNITY_PS4
        if (m_bPs4)
        {
            bKeepAlive = Instantiate();
        }
#elif UNITY_SWITCH
        if (m_bSwitch)
        {
            bKeepAlive = Instantiate();
        }
#else
        if (m_bPc)
        {
            bKeepAlive = Instantiate();
        }
#endif

        enabled = bKeepAlive;
    }

    bool Instantiate()
    {
        if (!m_bWaitForPlatform || PlatformManager.Current.IsInitialized())
        {
            GameObject oGo = null;
            if (m_oPrefab != null)
            {
                oGo = Instantiate(m_oPrefab);
            }
            else if (!m_sResource.isNullOrEmpty())
            {
                Object oResource = Resources.Load(m_sResource);
                if (oResource != null)
                {
                    oGo = Instantiate((GameObject)oResource);
                }
            }
            if (oGo != null)
            {
                oGo.transform.SetParent(transform, false);
                return false;
            }
        }
        return m_bWaitForPlatform;
    }

    void Update()
    {
        enabled = Instantiate();
    }
}
