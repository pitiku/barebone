using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class DebugToolsWindow : EditorWindow
{
    void OnGUI()
    {
        // Visual components
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("OnScreenLog", GUILayout.Width(81));
        EditorPrefs.SetBool(Deb.B_ONSCREENLOG, EditorGUILayout.Toggle(EditorPrefs.GetBool(Deb.B_ONSCREENLOG), GUILayout.Width(25)));
        EditorGUILayout.LabelField("FPS", GUILayout.Width(25));
        EditorPrefs.SetBool(HUDFPS.B_SHOWFPS, EditorGUILayout.Toggle(EditorPrefs.GetBool(HUDFPS.B_SHOWFPS), GUILayout.Width(25)));
        EditorGUILayout.EndHorizontal();

        // apply stuff
        if (EditorPrefs.GetBool(Deb.B_DEBUG_TIME_SCALE))
        {
            Deb.Instance.setTimeScale(EditorPrefs.GetInt(Deb.I_DEBUG_TIME_SCALE));
        }

        if (OnScreenLog.Instance)
        {
            OnScreenLog.Instance.setVisible(EditorPrefs.GetBool(Deb.B_ONSCREENLOG));
        }

        //Hierarchy update time
        //if (HierarchyManager.Instance != null)
        //{
        //    EditorGUILayout.BeginHorizontal();
        //    EditorGUILayout.LabelField("Hierarchy", GUILayout.Width(81));
        //    string sValue = EditorGUILayout.TextField(HierarchyManager.Instance.m_fUpdateComponentsFrequency.ToString(), GUILayout.Width(30));

        //    float fValue;
        //    if (float.TryParse(sValue, out fValue))
        //    {
        //        HierarchyManager.Instance.m_fUpdateComponentsFrequency = fValue;
        //    }

        //    if (HierarchyManager.Instance.m_bActive)
        //    {
        //        if (GUILayout.Button("Disable"))
        //        {
        //            HierarchyManager.disableHierarchyManager();
        //        }
        //    }
        //    else
        //    {
        //        if (GUILayout.Button("Enable"))
        //        {
        //            HierarchyManager.enableHierarchyManager();
        //        }
        //    }

        //    EditorGUILayout.EndHorizontal();
        //}
    }

    //float fLastTime = 0;
    //void Update()
    //{
    //    float fps = 1.0f / (Time.realtimeSinceStartup - fLastTime);
    //    Deb.log("FPS: " + fps);
    //    fLastTime = Time.realtimeSinceStartup;
    //}

    [MenuItem("UPTools/Debug Tools", false, 80)]
    public static void ShowTools()
    {
        EditorWindow.GetWindow<DebugToolsWindow>();
    }
}
#endif
