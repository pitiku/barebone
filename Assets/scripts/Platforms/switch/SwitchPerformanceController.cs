using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_SWITCH
using UnityEngine.Switch;
#endif

public class SwitchPerformanceController : MonoBehaviour
{
    [FoldoutGroup("Docked Settings", expanded: true)] public int m_iWidth_Docked = 1920;
    [FoldoutGroup("Docked Settings", expanded: true)] public int m_iHeight_Docked = 1080;
    [FoldoutGroup("Docked Settings", expanded: true)] public int m_iHz_Docked = 60;
    [FoldoutGroup("Docked Settings", expanded: true)] public string m_sQualityName_Docked = "Switch";

    [FoldoutGroup("Handheld Settings", expanded: true)] public int m_iWidth_Handheld = 1280;
    [FoldoutGroup("Handheld Settings", expanded: true)] public int m_iHeight_Handheld = 720;
    [FoldoutGroup("Handheld Settings", expanded: true)] public int m_iHz_Handheld = 60;
    [FoldoutGroup("Handheld Settings", expanded: true)] public string m_sQualityName_Handheld = "Switch_Low";

#if UNITY_SWITCH //&& !UNITY_EDITOR
    private bool m_bDocked;
    private static bool ms_bDockedWanted;

    void Start()
    {
        ms_bDockedWanted = Operation.mode == Operation.OperationMode.Console;
        setPerformanceMode(ms_bDockedWanted);
    }

    void Update()
    {
#if DEBUG
        if (RewiredManager.Instance.GetActionAnyPlayer("LB") && RewiredManager.Instance.GetActionDownAnyPlayer("UIDown"))
        {
            ms_bDockedWanted = !m_bDocked;
        }
#endif

        if (ms_bDockedWanted != m_bDocked)
        {
            setPerformanceMode(ms_bDockedWanted);
        }
    }

    private void setPerformanceMode(bool _bDocked)
    {
        m_bDocked = _bDocked;
        Debug.Log($"SwitchPerformanceController: Update Boost -> {m_bDocked}");

        if (m_bDocked)
        {
            Performance.SetConfiguration(Performance.PerformanceMode.Boost, Performance.PerformanceConfiguration.Cpu1020MhzGpu768MhzEmc1600Mhz);
        }
        else
        {
            Performance.SetConfiguration(Performance.PerformanceMode.Normal, Performance.PerformanceConfiguration.Cpu1020MhzGpu384MhzEmc1331Mhz);
        }
        SwitchManager.Instance.setCurrentFastLoad();

        // Quality settings
        string sQualityName = m_bDocked ? m_sQualityName_Docked : m_sQualityName_Handheld;

        bool bQualityFound = false;
        for (int iIndex = 0; iIndex < QualitySettings.names.Length; ++iIndex)
        {
            if (QualitySettings.names[iIndex].Equals(sQualityName))
            {
                QualitySettings.SetQualityLevel(iIndex, true);
                bQualityFound = true;
            }
        }

        if (!bQualityFound)
        {
            Debug.Log($"QualitySettings {(sQualityName)} not found! (Check Unity Edit->Project Settings->Quality");
        }

        // Resolution
        int iWidth = m_bDocked ? m_iWidth_Docked : m_iWidth_Handheld;
        int iHeight = m_bDocked ? m_iHeight_Docked : m_iHeight_Handheld;
        int iHz = m_bDocked ? m_iHz_Docked : m_iHz_Handheld;

        if ((iWidth != 0) && (iHeight != 0) && (iHz != 0))
        {
            Screen.SetResolution(iWidth, iHeight, FullScreenMode.ExclusiveFullScreen, new RefreshRate() { numerator = (uint)iHz, denominator = 1 });
        }
    }

    #region NOTIFICATION
    static void notificationMessageReceived(Notification.Message message)
    {
        if (message == Notification.Message.OperationModeChanged)
        {
            Operation.OperationMode opMode = Operation.mode;
            Debug.Log($"OperationMode: {opMode}");
            ms_bDockedWanted = opMode == Operation.OperationMode.Console;
        }

        if (message == Notification.Message.PerformanceModeChanged)
        {
            Debug.Log($"PerformanceMode: {Performance.mode}");
        }
    }

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        // Set notification modes
        Notification.SetPerformanceModeChangedNotificationEnabled(true);
        Notification.SetOperationModeChangedNotificationEnabled(true);
        Notification.SetResumeNotificationEnabled(true);

        // Add notification handler
        Notification.notificationMessageReceived += notificationMessageReceived;
    }
    #endregion
#endif
}
