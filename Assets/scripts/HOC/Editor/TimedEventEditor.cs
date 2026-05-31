
/*[CustomEditor(typeof(TimedEvent), true)]
[CanEditMultipleObjects]
public class TimedEventEditor : Editor
{
    Color oColor = new Color(0.6f, 1.0f, 1.0f);
    Color oColorDisabled = new Color(0.3f, 0.6f, 0.6f);
    private static readonly string[] dontIncludeMe = new string[] { "m_Script", "m_bStartOrEnd", "m_fTime" };

    SerializedProperty m_bStartOrEnd;
    SerializedProperty m_fTime;
    MonoBehaviour m_component;

    public virtual void OnEnable()
    {
        m_bStartOrEnd = serializedObject.FindProperty("m_bStartOrEnd");
        m_fTime = serializedObject.FindProperty("m_fTime");
        m_component = (MonoBehaviour)target;
    }

    public override void OnInspectorGUI()
    {
        var defColor = GUI.color;
        Rect oRect = EditorGUILayout.GetControlRect();
        oRect.height = 4;
        GUI.color = m_component.enabled ? oColor : oColorDisabled;
        GUI.DrawTexture(oRect, EditorGUIUtility.whiteTexture);
        GUI.color = defColor;

        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_bStartOrEnd, new GUIContent(m_bStartOrEnd.boolValue ? "Start" : "End"));
        EditorGUILayout.PropertyField(m_fTime);
        EditorGUILayout.EndHorizontal();

        DrawCustomProperties();

        serializedObject.ApplyModifiedProperties();
    }

    public virtual void DrawCustomProperties()
    {
        DrawPropertiesExcluding(serializedObject, dontIncludeMe);
    }
}*/
