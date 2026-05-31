using UnityEditor;
using UnityEngine;

public class HUDFPS : SceneSingleton<HUDFPS>
{
#if DEBUG
    public static string B_SHOWFPS = "SHOWFPS";

    bool m_bVisible = false;

    float m_fDeltaTime = 0.0f;

    Rect m_oRectFront;
    Rect m_oRectBack;
    GUIStyle m_oStyleWhite;
    GUIStyle m_oStyleBlack;

    void Start()
    {
        if (Deb.Instance != null)
        {
            Deb.Instance.addShortcut("RT", switchVisibleFPS);
        }

        int w = Screen.width, h = Screen.height;
        m_oRectFront = new Rect(w * 0.8f, 0.1f, 100, 100);
        m_oRectBack = new Rect(m_oRectFront);
        m_oRectBack.x++;
        m_oRectBack.y++;

        m_oStyleWhite = new GUIStyle();
        m_oStyleWhite.alignment = TextAnchor.UpperLeft;
        m_oStyleWhite.fontSize = h * 2 / 100;
        m_oStyleWhite.normal.textColor = Color.white;

        m_oStyleBlack = new GUIStyle();
        m_oStyleBlack.alignment = TextAnchor.UpperLeft;
        m_oStyleBlack.fontSize = h * 2 / 100;
        m_oStyleBlack.normal.textColor = Color.black;
    }

    public void Update()
    {
        base.update();

        m_fDeltaTime += (Time.deltaTime - m_fDeltaTime) * 0.1f;

#if UNITY_EDITOR
        m_bVisible = EditorPrefs.GetBool(HUDFPS.B_SHOWFPS);
#endif
    }

    public void OnGUI()
    {
        if (m_bVisible)
        {
            float msec = m_fDeltaTime * 1000.0f;
            float fps = 1.0f / m_fDeltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

            GUI.Label(m_oRectBack, text, m_oStyleBlack);
            GUI.Label(m_oRectFront, text, m_oStyleWhite);
        }
    }

    public void switchVisibleFPS()
    {
        m_bVisible = !m_bVisible;
    }
#endif
}

