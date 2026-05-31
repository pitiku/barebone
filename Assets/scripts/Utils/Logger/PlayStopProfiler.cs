using System;
using UnityEditor;
using UnityEngine;

public class PlayStopProfiler : MonoBehaviour
{
#if UNITY_EDITOR
    static float ms_fTime1;
    static bool ms_bFlag = false;
    static bool ms_bStart = false;
    static DateTime ms_oDate;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void first()
    {
        EditorApplication.playModeStateChanged -= editorStart;
        ms_fTime1 = Time.realtimeSinceStartup;
        ms_bStart = true;
        ms_bFlag = false;
        ms_oDate = new DateTime();
    }

    void Start()
    {
        if (ms_bStart)
        {
            ms_bStart = false;
            Debug.Log("start: " + (Time.realtimeSinceStartup - ms_fTime1));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            EditorApplication.playModeStateChanged += editorStart;
            ms_oDate = DateTime.Now;
            ms_bFlag = true;
            EditorApplication.isPlaying = false;
        }
    }

    [ExecuteInEditMode]
    static void editorStart(PlayModeStateChange _oMode)
    {
        if (ms_bFlag && _oMode == PlayModeStateChange.EnteredEditMode)
        {
            ms_bFlag = false;
            Debug.Log("stop: " + (DateTime.Now - ms_oDate));
        }
    }
#endif
}
