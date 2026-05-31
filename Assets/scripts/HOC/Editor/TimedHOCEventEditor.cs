
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimedHOCEvent), true)]
public class TimedHOCEventEditor : Editor
{
    Color oColor = new Color(0.6f, 1.0f, 1.0f);

    SerializedProperty m_bStartOrEnd;
    SerializedProperty m_fTime;
    SerializedProperty m_oHOCComponent;
    SerializedProperty m_sMethod;
    //SerializedProperty m_aParams;

    void OnEnable()
    {
        m_bStartOrEnd = serializedObject.FindProperty("m_bStartOrEnd");
        m_fTime = serializedObject.FindProperty("m_fTime");
        m_oHOCComponent = serializedObject.FindProperty("m_oHOCComponent");
        m_sMethod = serializedObject.FindProperty("m_sMethod");
        //m_aParams = serializedObject.FindProperty("m_aParams");
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

        EditorGUILayout.PropertyField(m_bStartOrEnd);
        EditorGUILayout.PropertyField(m_fTime);
        EditorGUILayout.PropertyField(m_oHOCComponent);

        serializedObject.ApplyModifiedProperties();

        TimedHOCEvent tm = (TimedHOCEvent)target;
        Component c = tm.m_oHOCComponent.get();
        if (c != null)
        {
            MethodInfo[] methodInfos = c.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methodInfos.Length > 0)
            {
                List<string> methods = new List<string>();
                List<string> methodsAndParams = new List<string>();
                foreach (MethodInfo m in methodInfos)
                {
                    methods.Add(m.Name);
                    string methodName = m.Name + "(";
                    for (int paramIndex = 0; paramIndex < m.GetParameters().Length; ++paramIndex)
                    {
                        if (paramIndex > 0)
                        {
                            methodName += ", ";
                        }
                        ParameterInfo p = m.GetParameters()[paramIndex];
                        string param = p.ParameterType.ToString();
                        if (param.IndexOf(".") > -1)
                        {
                            int lastIndex = param.LastIndexOf(".") + 1;
                            methodName += param.Substring(lastIndex, param.Length - lastIndex) + " " + p.Name;
                        }
                        else
                        {
                            methodName += param + " " + p.Name;
                        }
                    }
                    methodName += ")";
                    methodsAndParams.Add(methodName);
                }
                methods.Sort();
                methodsAndParams.Sort();
                int selectedIndex = 0;
                if (m_sMethod != null)
                {
                    selectedIndex = methods.IndexOf(m_sMethod.stringValue);
                }
                int newIndex = EditorGUILayout.Popup("Method", selectedIndex, methodsAndParams.ToArray());
                if (newIndex != selectedIndex)
                {
                    m_sMethod.stringValue = methods[newIndex];
                }

                MethodInfo method = methodInfos[newIndex];
                for (int paramIndex = 0; paramIndex < method.GetParameters().Length; ++paramIndex)
                {
                    //ParameterInfo p = method.GetParameters()[paramIndex];

                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
