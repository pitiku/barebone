#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ReorderableElementEditor
{
    public string title;
    public string property;
    public float width;
    public bool fixedWidth;

    public ReorderableElementEditor(string _title, string _property, bool _fixedWidth, float _width)
    {
        title = _title;
        property = _property;
        fixedWidth = _fixedWidth;
        width = _width;
    }
}

public class ReorderableListClassEditor : Editor
{
    const float CHAR_WIDTH = 7.0f;
    const float LABEL_EXTRA_WIDTH = 3.0f;
    const float PROPERTY_EXTRA_WIDTH = 3.0f;
    const float MIN_UNFIXED_WIDTH = 10.0f;

    protected ReorderableList list;

    public void initializeList(string listProperty)
    {
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty(listProperty),
                true, true, true, true);
    }

    public void drawTitle(string title)
    {
        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, title);
        };
    }

    public void drawElements(params ReorderableElementEditor[] _aoElements)
    {
        list.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                float fX = 0.0f;

                // first to do proper non fixed widths, calculate total width of non fixed width elements and labels
                float fTotalUnfixedWidth = 0.0f;
                float fUsedWidth = 70.0f;
                for (int i = 0; i < _aoElements.Length; ++i)
                {
                    string sTitle = _aoElements[i].title;
                    if (!sTitle.isNullOrEmpty())
                    {
                        fUsedWidth += (sTitle.Length * CHAR_WIDTH) + LABEL_EXTRA_WIDTH;
                    }

                    if (!_aoElements[i].fixedWidth)
                    {
                        fTotalUnfixedWidth += _aoElements[i].width;
                    }
                    else
                    {
                        fUsedWidth += _aoElements[i].width;
                    }
                }

                float fWidthLeft = EditorGUIUtility.currentViewWidth - fUsedWidth;
                float fUnfixedFactor = fWidthLeft / fTotalUnfixedWidth;

                for (int i = 0; i < _aoElements.Length; ++i)
                {
                    // check if this element has title
                    string sTitle = _aoElements[i].title;
                    if (!sTitle.isNullOrEmpty())
                    {
                        float fWidth = (sTitle.Length * CHAR_WIDTH) + LABEL_EXTRA_WIDTH;
                        EditorGUI.LabelField(
                            new Rect(rect.x + fX, rect.y, fWidth, EditorGUIUtility.singleLineHeight), sTitle);

                        fX += fWidth;
                    }

                    // draw element property
                    float fPropertyWidth = _aoElements[i].width;
                    if (!_aoElements[i].fixedWidth)
                    {
                        fPropertyWidth = fPropertyWidth * fUnfixedFactor;
                        if (fPropertyWidth < MIN_UNFIXED_WIDTH)
                        {
                            fPropertyWidth = MIN_UNFIXED_WIDTH;
                        }
                    }

                    EditorGUI.PropertyField(
                       new Rect(rect.x + fX, rect.y, fPropertyWidth, EditorGUIUtility.singleLineHeight),
                       element.FindPropertyRelative(_aoElements[i].property), GUIContent.none);

                    fX += fPropertyWidth;

                    EditorGUI.LabelField(
                            new Rect(rect.x + fX, rect.y, PROPERTY_EXTRA_WIDTH, EditorGUIUtility.singleLineHeight), "");

                    fX += PROPERTY_EXTRA_WIDTH;
                }
            };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    public virtual ReorderableList CreateList(SerializedObject obj, SerializedProperty prop)
    {
        ReorderableList list = new ReorderableList(obj, prop, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Sprites");
        };

        List<float> heights = new List<float>(prop.arraySize);

        list.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            Sprite s = element.objectReferenceValue as Sprite;

            bool foldout = active;
            float height = EditorGUIUtility.singleLineHeight * 1.25f;
            if (foldout)
            {
                height = EditorGUIUtility.singleLineHeight * 5;
            }

            try
            {
                heights[index] = height;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Deb.logWarning(e.Message);
            }
            finally
            {
                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = floats.toList();
            }

            float margin = height / 10;
            rect.y += margin;
            rect.height = height / 5 * 4;
            rect.width = (rect.width / 2) - (margin / 2);

            if (foldout)
            {
                if (s)
                {
                    EditorGUI.DrawPreviewTexture(rect, s.texture);
                }
            }
            rect.x += rect.width + margin;
            EditorGUI.ObjectField(rect, element, GUIContent.none);
        };

        list.elementHeightCallback = (index) =>
        {
            Repaint();
            float height = 0;

            try
            {
                height = heights[index];
            }
            catch (ArgumentOutOfRangeException e)
            {
                Deb.logWarning(e.Message);
            }
            finally
            {
                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = floats.toList();
            }

            return height;
        };

        list.drawElementBackgroundCallback = (rect, index, active, focused) =>
        {
            rect.height = heights[index];
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
            tex.Apply();
            if (active)
            {
                GUI.DrawTexture(rect, tex as Texture);
            }
        };

        list.onAddDropdownCallback = (rect, li) =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Element"), false, () =>
            {
                serializedObject.Update();
                li.serializedProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            });

            menu.ShowAsContext();

            float[] floats = heights.ToArray();
            Array.Resize(ref floats, prop.arraySize);
            heights = floats.toList();
        };

        return list;
    }
}
#endif
