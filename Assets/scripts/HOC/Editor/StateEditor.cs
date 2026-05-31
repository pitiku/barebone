/*
[CustomEditor(typeof(State), true)]
[CanEditMultipleObjects]
public class StateEditor : OdinEditor
{
    Color oColor = new Color(0.7f, 0.0f, 0.0f);
    Color oColorDisabled = new Color(0.4f, 0.0f, 0.0f);
    protected static readonly string[] m_asDontIncludeMe = new string[] { "m_Script", "m_iInterruptPriority", "m_bUpdateImmediately", "m_bDebugState", "m_bDebugSaveStates", "m_bPaused" };

    MonoBehaviour m_component;
    SerializedProperty m_oInterruptPriority;
    SerializedProperty m_oUpdateImmediately;
    SerializedProperty m_oDebugState;
    SerializedProperty m_oDebugSaveStates;

    public virtual void OnEnable()
    {
        m_component = (MonoBehaviour)target;
        m_oInterruptPriority = serializedObject.FindProperty("m_iInterruptPriority");
        m_oUpdateImmediately = serializedObject.FindProperty("m_bUpdateImmediately");
        m_oDebugState = serializedObject.FindProperty("m_bDebugState");
        m_oDebugSaveStates = serializedObject.FindProperty("m_bDebugSaveStates");
    }

    public override void OnInspectorGUI()
    {
        var defColor = GUI.color;
        Rect oRect = EditorGUILayout.GetControlRect();
        if (m_component is StateMachine)
            oRect.height = 6;
        else
            oRect.height = 4;
        GUI.color = m_component.enabled ? oColor : oColorDisabled;
        GUI.DrawTexture(oRect, EditorGUIUtility.whiteTexture);
        GUI.color = defColor;

        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("priority", GUILayout.Width(55f));
        EditorGUILayout.PropertyField(m_oInterruptPriority, GUIContent.none, GUILayout.Width(30f));
        EditorGUILayout.LabelField("update immediately", GUILayout.Width(120f));
        EditorGUILayout.PropertyField(m_oUpdateImmediately, GUIContent.none, GUILayout.Width(20f));
        EditorGUILayout.LabelField("paused", GUILayout.Width(45f));
        EditorGUILayout.Toggle(((State)target).m_bPaused, GUILayout.Width(20f));
        EditorGUILayout.LabelField("debug", GUILayout.Width(40f));
        EditorGUILayout.PropertyField(m_oDebugState, GUIContent.none, GUILayout.Width(20f));
        EditorGUILayout.LabelField("Save", GUILayout.Width(40f));
        EditorGUILayout.PropertyField(m_oDebugSaveStates, GUIContent.none, GUILayout.Width(20f));
        EditorGUILayout.EndHorizontal();

        InspectorUtilities.DrawPropertiesInTree()
        DrawPropertiesExcluding(serializedObject, GetDontInclude());
        serializedObject.ApplyModifiedProperties();
    }

    public void DrawSerializedOdinAttributes(string[] IgnoreList = null)
    {
        var tree = this.Tree;
        var obj = this.target as vMonoBehaviour;

        InspectorUtilities.BeginDrawPropertyTree(tree, true);

        List<string> IgnoreProperties = IgnoreList.vToList();

        SerializedProperty prop = serializedObject.GetIterator();
        if (prop.NextVisible(true))
        {
            do
            {
                var prop_I = tree.GetPropertyAtPath(prop.name);

                if (IgnoreProperties != null)
                {
                    if (prop_I != null && IgnoreProperties.Contains(prop.name) == false)
                        prop_I.Draw();
                }
                else
                {
                    if (prop_I != null)
                        prop_I.Draw();
                }
            }
            while (prop.NextVisible(false));
        }

        InspectorUtilities.EndDrawPropertyTree(tree);
    }

    protected virtual string[] GetDontInclude()
    {
        return m_asDontIncludeMe;
    }

    [MenuItem("GameObject/HOC/State", false, 1)]
    static void CreateState(MenuCommand menuCommand)
    {
        GameObject go = new GameObject("STATE");
        go.AddComponent<State>();
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
}

*/