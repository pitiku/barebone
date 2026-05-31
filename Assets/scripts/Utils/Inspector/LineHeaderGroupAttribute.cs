using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using System.Reflection;

// is created as a group attribute because in case there is a horizontal group below, it will work
public class LineHeaderGroupAttribute : PropertyGroupAttribute
{
    public float m_fHeight;
    public Color m_oColor;
    public string m_sMethod = "";
    public string m_sGetComments = "";
    public string m_sSetComments = "";

    public LineHeaderGroupAttribute(string path) : base(path) { }

    public LineHeaderGroupAttribute(string path, float _height, float r, float g, float b, float a = 1f) : base(path)
    {
        m_fHeight = _height;
        m_oColor = new Color(r, g, b, a);
        m_sMethod = "";
        m_sGetComments = "";
        m_sSetComments = "";
    }

    public LineHeaderGroupAttribute(string path, float _height, string _sMethod, string _sGetComments = "", string _sSetComments = "") : base(path)
    {
        m_fHeight = _height;
        m_sMethod = _sMethod;
        m_sGetComments = _sGetComments;
        m_sSetComments = _sSetComments;
    }
}

public class LineHeaderGroupAttributeDrawer : OdinGroupDrawer<LineHeaderGroupAttribute>
{
    CommentPopUp m_oWindow = null;

    protected override void DrawPropertyLayout(GUIContent label)
    {
        Rect rLine = EditorGUILayout.GetControlRect();
        rLine.y = rLine.y + (rLine.height * 0.5f) - (Attribute.m_fHeight * 0.5f);
        rLine.height = Attribute.m_fHeight;

        if (Attribute.m_sMethod.isNullOrEmpty())
        {
            SirenixEditorGUI.DrawSolidRect(rLine, Attribute.m_oColor);
        }
        else
        {
            MethodInfo oMethod = Property.Tree.UnitySerializedObject.targetObject.GetType().GetMethod(Attribute.m_sMethod, BindingFlags.Instance | BindingFlags.Public);
            if (oMethod != null)
            {
                Object target = Property.Tree.UnitySerializedObject.targetObject;
                Color cColor = (Color)oMethod.Invoke(target, null);
                SirenixEditorGUI.DrawSolidRect(rLine, cColor);
            }
        }

        if (!Attribute.m_sGetComments.isNullOrEmpty())
        {
            MethodInfo oMethod = Property.Tree.UnitySerializedObject.targetObject.GetType().GetMethod(Attribute.m_sGetComments, BindingFlags.Instance | BindingFlags.Public);
            if (oMethod != null)
            {
                Object target = Property.Tree.UnitySerializedObject.targetObject;
                string sText = (string)oMethod.Invoke(target, null);
                if (!sText.isNullOrEmpty())
                {
                    float fWidth = GUI.skin.label.CalcSize(new GUIContent(sText)).x + 2;
                    Rect rLabel = new Rect(rLine.x + 50, rLine.y - 8, fWidth, 16);
                    EditorGUI.LabelField(rLabel, sText, EditorStyles.textField);
                }
            }
        }

        if (!Attribute.m_sSetComments.isNullOrEmpty())
        {
            Rect rButton = new Rect(rLine.x - 18, rLine.y - 8, 19, 18);
            if (GUI.Button(rButton, "C"))
            {
                if (m_oWindow != null)
                {
                    m_oWindow.Close();
                    m_oWindow = null;
                }

                Object target = Property.Tree.UnitySerializedObject.targetObject;
                MethodInfo oMethod = Property.Tree.UnitySerializedObject.targetObject.GetType().GetMethod(Attribute.m_sGetComments, BindingFlags.Instance | BindingFlags.Public);
                string sComment = (string)oMethod.Invoke(target, null);

                m_oWindow = ScriptableObject.CreateInstance<CommentPopUp>();
                m_oWindow.position = new Rect(500, 500, 250, 50);
                m_oWindow.setStuff(sComment, this);
                m_oWindow.Show();
            }
        }
    }

    public void setComment(string _sComment)
    {
        MethodInfo oMethod = Property.Tree.UnitySerializedObject.targetObject.GetType().GetMethod(Attribute.m_sSetComments, BindingFlags.Instance | BindingFlags.Public);
        if (oMethod != null)
        {
            Object target = Property.Tree.UnitySerializedObject.targetObject;
            oMethod.Invoke(target, new object[] { _sComment });
            EditorUtility.SetDirty(target);
        }
    }
}

public class CommentPopUp : EditorWindow
{
    string m_sText;
    LineHeaderGroupAttributeDrawer m_oCaller;
    bool m_bFirst = true;

    public CommentPopUp()
    {
        titleContent = new GUIContent("Comment");
    }

    public void setStuff(string _sText, LineHeaderGroupAttributeDrawer _oCaller)
    {
        m_sText = _sText;
        m_oCaller = _oCaller;
    }

    void OnGUI()
    {
        GUI.SetNextControlName("Comment");
        m_sText = EditorGUILayout.TextField("Comment: ", m_sText);
        if (m_bFirst)
        {
            m_bFirst = false;
            GUI.FocusControl("Comment");
        }

        Event e = Event.current;

        if (e.keyCode == KeyCode.Escape)
        {
            Close();
        }
        else if (GUILayout.Button("Done") || e.keyCode == KeyCode.KeypadEnter || e.keyCode == KeyCode.Return)
        {
            m_oCaller.setComment(m_sText);
            Close();
        }
    }
}
#else
public class LineHeaderGroupAttribute : System.Attribute
{
    public LineHeaderGroupAttribute(string path, float _height, float r, float g, float b, float a = 1f){}
    public LineHeaderGroupAttribute(string path, float _height, string _sMethod){}
}
#endif
