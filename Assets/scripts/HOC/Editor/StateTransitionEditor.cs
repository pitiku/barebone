
/*[CustomEditor(typeof(StateTransition), true)]
[CanEditMultipleObjects]
public class StateTransitionEditor : Editor
{
    Color oColor = new Color(0.0f, 0.7f, 0.0f);
    Color oColorDisabled = new Color(0.0f, 0.35f, 0.0f);
    private static readonly string[] dontIncludeMe = new string[] { "m_Script", "m_iType", "m_oTargetState" };

    SerializedProperty m_targetState;
    MonoBehaviour m_component;

    void OnEnable()
    {
        m_targetState = serializedObject.FindProperty("m_oTargetState");
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
        EditorGUILayout.PropertyField(m_targetState);
        EditorGUILayout.EndHorizontal();

        DrawPropertiesExcluding(serializedObject, dontIncludeMe);

        serializedObject.ApplyModifiedProperties();
    }
}*/
