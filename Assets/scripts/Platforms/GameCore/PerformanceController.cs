using System.Collections.Generic;
using UnityEngine;

public class PerformanceController : MonoBehaviour
{
    [System.Serializable]
    public class PerformanceSettings
    {
        public int m_iWidth = 1920;
        public int m_iHeight = 1080;
        public int m_iHz = 120;
        public string m_sQualityName = "";
        public int m_iVSync = 1;
    }

    public List<PerformanceSettings> m_aoSettings = new List<PerformanceSettings>();
    public int m_iDefaultIndex = 0;

#if UNITY_GAMECORE && !UNITY_GAMECOREPC // && !UNITY_EDITOR
    private void Awake()
    {
        OnScreenLog.Add("Version: " + UnityEngine.GameCore.Hardware.version);

        int m_iTargetFrameRate = 120;

        if (UnityEngine.GameCore.Hardware.version == UnityEngine.GameCore.HardwareVersion.XboxSeriesS)
        {
            setQualityLevel("XSeriesS");
        }
        else if (UnityEngine.GameCore.Hardware.version == UnityEngine.GameCore.HardwareVersion.XboxOne)
        {
            m_iTargetFrameRate = 30;
        }

        QualitySettings.vSyncCount = 1;
        Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen, new RefreshRate() { numerator = (uint)m_iTargetFrameRate, denominator = 1 });
        Application.targetFrameRate = m_iTargetFrameRate;
    }
#endif

#if DEBUG
    private int m_iCurrentIndex;

    void Start()
    {
        if (m_iDefaultIndex >= 0)
        {
            setPerformanceMode(m_iDefaultIndex);
        }
    }

    void Update()
    {
        if (RewiredManager.Instance.GetActionAnyPlayer("LB") && RewiredManager.Instance.GetActionDownAnyPlayer("UIDown"))
        {
            setPerformanceMode((m_iCurrentIndex + 1) % m_aoSettings.Count);
        }
        if (RewiredManager.Instance.GetActionAnyPlayer("LB") && RewiredManager.Instance.GetActionDownAnyPlayer("UIUp"))
        {
            int iCurrent = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel((iCurrent + 1) % QualitySettings.count);
            QualitySettings.vSyncCount = 1;
            Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen, new RefreshRate() { numerator = (uint)120, denominator = 1 });
            Application.targetFrameRate = 120;
        }
    }

    private void setPerformanceMode(int _iIndex)
    {
        m_iCurrentIndex = _iIndex;

        OnScreenLog.Add($"PerformanceController: Set -> {m_iCurrentIndex}");

        // Quality settings
        setQualityLevel(m_aoSettings[m_iCurrentIndex].m_sQualityName);

        QualitySettings.vSyncCount = m_aoSettings[m_iCurrentIndex].m_iVSync;

        // Resolution
        int iWidth = m_aoSettings[m_iCurrentIndex].m_iWidth;
        int iHeight = m_aoSettings[m_iCurrentIndex].m_iHeight;
        int iHz = m_aoSettings[m_iCurrentIndex].m_iHz;

        if ((iWidth != 0) && (iHeight != 0) && (iHz != 0))
        {
            Screen.SetResolution(iWidth, iHeight, FullScreenMode.ExclusiveFullScreen, new RefreshRate() { numerator = (uint)iHz, denominator = 1 });
            Application.targetFrameRate = iHz;
        }
    }
#endif

    void setQualityLevel(string _sName)
    {
        if (!_sName.isNullOrEmpty())
        {
            bool bQualityFound = false;
            for (int iIndex = 0; iIndex < QualitySettings.names.Length; ++iIndex)
            {
                if (QualitySettings.names[iIndex].Equals(_sName))
                {
                    OnScreenLog.Add($"SetQualityLevel {(_sName)}");
                    QualitySettings.SetQualityLevel(iIndex, true);
                    bQualityFound = true;
                }
            }
            if (!bQualityFound)
            {
                OnScreenLog.Add($"QualitySettings {(_sName)} not found! (Check Unity Edit->Project Settings->Quality");
            }
        }

    }
}
