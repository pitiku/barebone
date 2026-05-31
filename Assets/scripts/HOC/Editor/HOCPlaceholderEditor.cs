using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HOCPlaceholder), true)]
[CanEditMultipleObjects]
public class HOCPlaceholderEditor : Editor
{
    Color oColorState = new Color(0.7f, 0.0f, 0.0f);
    Color oColorBehavior = new Color(0.3f, 0.3f, 1.0f);
    Color oColorTransition = new Color(0.0f, 0.7f, 0.0f);
    Color oColorEvent = new Color(0.6f, 1.0f, 1.0f);
    private static readonly string[] dontIncludeMe = new string[] { "m_Script" };

    public override void OnInspectorGUI()
    {
        HOCPlaceholder oP = (HOCPlaceholder)target;

        drawColorHeader(oP.m_iType);

        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, dontIncludeMe);
        serializedObject.ApplyModifiedProperties();
    }

    public void drawColorHeader(eHOCComponent _iType)
    {
        var defColor = GUI.color;
        Rect oRect = EditorGUILayout.GetControlRect();
        oRect.height = 4;

        switch (_iType)
        {
            case eHOCComponent.State:
                GUI.color = oColorState;
                break;
            case eHOCComponent.Behavior:
                GUI.color = oColorBehavior;
                break;
            case eHOCComponent.Event:
                GUI.color = oColorEvent;
                break;
            case eHOCComponent.Transition:
                GUI.color = oColorTransition;
                break;
        }

        GUI.DrawTexture(oRect, EditorGUIUtility.whiteTexture);
        GUI.color = defColor;
    }
}
