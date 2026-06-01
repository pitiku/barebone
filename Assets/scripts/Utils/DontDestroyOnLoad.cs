using UnityEngine;
using System.Collections.Generic;

public class DontDestroyOnLoad : MonoBehaviour
{
    public string m_sTag;

    private static readonly Dictionary<string, DontDestroyOnLoad> m_oInstanceMap = new Dictionary<string, DontDestroyOnLoad>();

    void Awake()
    {
        if (m_sTag.isNullOrEmpty())
        {
            Destroy(gameObject);
            return;
        }

        if (m_oInstanceMap.TryGetValue(m_sTag, out _))
        {
            Destroy(gameObject);
            return;
        }

        m_oInstanceMap[m_sTag] = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // Unregister from tracking if this was the registered instance
        if (!m_sTag.isNullOrEmpty() && m_oInstanceMap.TryGetValue(m_sTag, out var oInstance) && oInstance == this)
        {
            m_oInstanceMap.Remove(m_sTag);
        }
    }
}
