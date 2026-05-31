using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransitionOnMethod), true)]
public class TransitionOnMethodEditor : Editor
{
    Color oColor = new Color(0.0f, 0.7f, 0.0f);

    SerializedProperty m_oState;
    SerializedProperty m_oHOCComponent;
    SerializedProperty m_sMethod;
    SerializedProperty m_bResult;

    void OnEnable()
    {
        m_oState = serializedObject.FindProperty("m_oTargetState");
        m_oHOCComponent = serializedObject.FindProperty("m_oHOCComponent");
        m_sMethod = serializedObject.FindProperty("m_sMethod");
        m_bResult = serializedObject.FindProperty("m_bResult");
    }

    public override void OnInspectorGUI()
    {
        var defColor = GUI.color;
        Rect oRect = EditorGUILayout.GetControlRect();
        oRect.height = 4;
        GUI.color = oColor;
        GUI.DrawTexture(oRect, EditorGUIUtility.whiteTexture);
        GUI.color = defColor;

        serializedObject.Update();

        EditorGUILayout.PropertyField(m_oState);
        EditorGUILayout.PropertyField(m_oHOCComponent);

        serializedObject.ApplyModifiedProperties();

        TransitionOnMethod tm = (TransitionOnMethod)target;
        Component c = tm.m_oHOCComponent.get();
        if (c != null)
        {
            MethodInfo[] methodInfos = c.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methodInfos.Length > 0)
            {
                List<string> methods = new List<string>();
                foreach (MethodInfo m in methodInfos)
                {
                    methods.Add(m.Name);
                }
                methods.Sort();
                int selectedIndex = methods.IndexOf(m_sMethod.stringValue);
                int newIndex = EditorGUILayout.Popup("Method", selectedIndex, methods.ToArray());
                if (newIndex != selectedIndex)
                {
                    m_sMethod.stringValue = methods[newIndex];
                }

            }

            EditorGUILayout.PropertyField(m_bResult);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
