using System.Text;
using UnityEngine;

public class OnScreenLog : SceneSingleton<OnScreenLog>
{
#if DEBUG

    [SerializeField] bool m_bVisible = false;

    StringBuilder m_sLog = new StringBuilder();

    void Start()
    {
        Deb.Instance.addShortcut("RB", switchVisibleScreenLogs);
        Deb.Instance.addShortcut("UIUp", clearLog);
    }

    public void clearLog()
    {
        m_sLog.Clear();
    }

    public void switchVisibleScreenLogs()
    {
        m_bVisible = !m_bVisible;
    }

    public void setVisible(bool _bValue)
    {
        m_bVisible = _bValue;
    }

    void OnGUI()
    {
        if (m_bVisible)
        {
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.LowerLeft;

            GUI.color = Color.black;
            GUI.Label(new Rect(1, 21, Screen.width - 1, Screen.height), m_sLog.ToString(), GUI.skin.label);
            GUI.color = Color.white;
            GUI.Label(new Rect(0, 20, Screen.width - 1, Screen.height), m_sLog.ToString(), GUI.skin.label);
        }
    }

    public static void Add(string _sLog)
    {
        Instance.add(_sLog);
    }

    public static void AddError(string _sLog)
    {
        Instance.add(_sLog);
    }

    public void add(string _sLog)
    {
        m_sLog.Append(_sLog + "\n");
    }

#else
    public static void Add(string msg){}
    public static void AddError(string _sLog){}
#endif
}
