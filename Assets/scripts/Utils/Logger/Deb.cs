using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[System.Flags]
public enum eLogFlags
{
    PITIKU = 1 << 0,
    MAKI = 1 << 1,
    ATTACK = 1 << 2,
    MAP = 1 << 3,
    AI = 1 << 5,
    SIGNALBUS = 1 << 6,
    RANDOM = 1 << 7,
    HOC = 1 << 8,
    BARKS = 1 << 9,
    FOOTSTEPS = 1 << 10,
    SYSTEM = 1 << 11,
    DIEGO = 1 << 12,
    REPLAY = 1 << 13,
}

public class Deb : SceneSingleton<Deb>
{
#if DEBUG
    // Attack
    public const string HUMAN_BUFF = "Hum+";
    public const string HUMAN_NERF = "Hum-";
    public const string AI_BUFF = "AI+";
    public const string AI_NERF = "AI-";
    public const string CRITIC_BUFF = "Critic+";
    public const string CRITIC_NERF = "Critic-";

    public const string m_sLoggers = "LOGGERS";
    public const string m_sAct = "Act Selector";
    public static float m_fProgression = 0;
    public static float m_fDifficulty = 0;

    //protected override void Awake()
    //{
    //    Debug.unityLogger.logEnabled = Debug.isDebugBuild;
    //}

    public static Color GIZMOS_COLOR = new Color(1, 0, 0, 0.25f);

    public bool m_bDebActionsEnabled = true;
    public static bool m_bShowTimeFrame = true;

    void Start()
    {
        addShortcut("UIDown", clearConsole);
    }

    public override void update()
    {
        base.update();

        if (!m_bDebActionsEnabled) { return; }

        updateShortcuts();
    }

    void OnGUI()
    {
        onGUIShortcuts();
    }

#endif

    #region SHORTCUTS

    public string[] m_asBasicCombination;

    Dictionary<string, UnityAction> m_dShortcuts = new Dictionary<string, UnityAction>();
    string m_sShortcutText = "";
    bool m_bBasicShortcutPressed = false;

    public void addShortcut(string sKey, UnityAction oAction)
    {
#if DEBUG
        if (m_dShortcuts.ContainsKey(sKey))
        {
            Debug.LogWarning("Adding a shortcut(" + oAction.Method.Name + ") with the same key as an existing one(" + m_dShortcuts[sKey].Method.Name + ")");
        }

        m_dShortcuts[sKey] = oAction;
        m_sShortcutText += sKey + " -> " + oAction.Method.Name + "\n";
#endif
    }

    void updateShortcuts()
    {
#if DEBUG
        for (int iPlayerIndex = 0; iPlayerIndex < RewiredManager.Instance.m_playingPlayers.Length; iPlayerIndex++)
        {
            if (iPlayerIndex == -1)
            {
                continue;
            }

            m_bBasicShortcutPressed = true;
            for (int iKeyIndex = 0; iKeyIndex < m_asBasicCombination.Length; ++iKeyIndex)
            {
                bool bPressed = RewiredManager.Instance.GetAction(iPlayerIndex, m_asBasicCombination[iKeyIndex]);
                if (!bPressed)
                {
                    m_bBasicShortcutPressed = false;
                    break;
                }
            }

            if (m_bBasicShortcutPressed)
            {
                foreach (string sKey in m_dShortcuts.Keys)
                {
                    if (RewiredManager.Instance.GetActionDown(iPlayerIndex, sKey))
                    {
                        m_dShortcuts[sKey].Invoke();
                    }
                }
                break;
            }
        }
#endif
    }

    void onGUIShortcuts()
    {
        if (m_bBasicShortcutPressed)
        {
            //BG Box
            GUI.skin.box.fontSize = 36;
            GUI.skin.box.alignment = TextAnchor.UpperCenter;
            GUI.color = Color.white;
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.5f), "SHORTCUTS", GUI.skin.box);

            //Shortcuts
            GUI.skin.label.fontSize = 36;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(Screen.width * 0.27f, Screen.height * 0.29f, Screen.width * 0.5f, Screen.height * 0.5f), m_sShortcutText, GUI.skin.label);
        }
    }

    #endregion

    #region LOGS

    public const string B_ONSCREENLOG = "bOnScreenLog";

    public static void log(float _fLog, eLogFlags _eFilter = 0)
    {
        log(_fLog.ToString(), _eFilter);
    }

    public static void log(bool _bLog, eLogFlags _eFilter = 0)
    {
        log(_bLog.ToString(), _eFilter);
    }

    public static void log(int _iLog, eLogFlags _eFilter = 0)
    {
        log(_iLog.ToString(), _eFilter);
    }

    public static void log(Vector2 _vLog, eLogFlags _eFilter = 0)
    {
        log(_vLog.ToString(), _eFilter);
    }

    public static void log(Vector3 _vLog, eLogFlags _eFilter = 0)
    {
        log(_vLog.ToString(), _eFilter);
    }

    public static void log(string _sLog, eLogFlags _eFilter = 0, GameObject _oContext = null)
    {
#if UNITY_EDITOR
        // Filter check
        if (_eFilter != 0)
        {
            var activeFilters = (eLogFlags)EditorPrefs.GetInt(m_sLoggers);
            if (!activeFilters.HasFlag(_eFilter))
            {
                return; // Do not log if the specific filter is not active
            }
        }
        else if (EditorPrefs.GetInt(m_sLoggers) == 0)
        {
            return;
        }

        if (m_bShowTimeFrame)
        {
            _sLog = "(" + Time.frameCount + ") " + _sLog;
        }

        OnScreenLog.Instance?.add(_sLog);

        Debug.Log(_sLog, _oContext);
#else
        Debug.Log(_sLog, _oContext);
#endif
    }

    public static void logWarning(string _sLog)
    {
#if UNITY_EDITOR
        OnScreenLog.Instance?.add(_sLog);
        Debug.LogWarning(_sLog);
#else
        Debug.LogWarning(_sLog);
#endif
    }

    public static void logError(string _sLog, GameObject _go = null)
    {
#if UNITY_EDITOR
        OnScreenLog.Instance?.add(_sLog);
        Debug.LogError(_sLog, _go);
#else
        Debug.LogError(_sLog, _go);
#endif
    }

    public void clearConsole()
    {
#if UNITY_EDITOR
        var assembly = Assembly.GetAssembly(typeof(SceneView));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
#endif
    }

    #endregion

    #region TIME SCALE

    public const string B_DEBUG_TIME_SCALE = "bTimeScale";
    public const string I_DEBUG_TIME_SCALE = "iTimeScale";

    public float[] m_afTimeScales = { 0.1f, 0.5f, 1.0f, 1.2f, 1.33f, 2.0f, 5.0f, 20.0f };
    int m_iCurrentTimeScaleIndex = 2;

    public void setTimeScale(int _iIndex)
    {
        m_iCurrentTimeScaleIndex = Mathf.Clamp(_iIndex, 0, m_afTimeScales.Length - 1);
        setTimeScale(m_afTimeScales[m_iCurrentTimeScaleIndex]);
    }

    public void setTimeScale(float _fTimeScale)
    {
        Deb.log("Time scale: " + _fTimeScale);

        Time.timeScale = _fTimeScale;
        GameUtils.setAudioTimeScale(_fTimeScale);
    }

    #endregion

    #region DEBUG DRAWS 
    public void draw2DArrow(Vector2 _v1, Vector2 _v2, Color _oColor, float arrowHeadLength = 0.25f, float _fTime = 0.0f)
    {
        Vector3 v1 = _v1.toVector3xz();
        Vector3 v2 = _v2.toVector3xz();

        Debug.DrawLine(v1, v2, _oColor, _fTime);

        Vector3 vRightArrow = (_v2 - _v1).normalized.rotateClockwise(135).toVector3xz();
        Vector3 vLeftArrow = (_v2 - _v1).normalized.rotateClockwise(-135).toVector3xz();

        Debug.DrawLine(v2, v2 + vRightArrow * arrowHeadLength, _oColor, _fTime);
        Debug.DrawLine(v2, v2 + vLeftArrow * arrowHeadLength, _oColor, _fTime);
    }

    public void drawRectangle(Vector2 _vP, Vector2 _vSize, Color _oColor, float _fTime = 0.0f)
    {
        Vector2 vTL = _vP + (_vSize.x * 0.5f * Vector2.left) + (_vSize.y * 0.5f * Vector2.up);
        Vector2 vTR = _vP + (_vSize.x * 0.5f * Vector2.right) + (_vSize.y * 0.5f * Vector2.up);
        Vector2 vBL = _vP + (_vSize.x * 0.5f * Vector2.left) + (_vSize.y * 0.5f * Vector2.down);
        Vector2 vBR = _vP + (_vSize.x * 0.5f * Vector2.right) + (_vSize.y * 0.5f * Vector2.down);
        Debug.DrawLine(vTL, vTR, _oColor, _fTime);
        Debug.DrawLine(vTR, vBR, _oColor, _fTime);
        Debug.DrawLine(vBR, vBL, _oColor, _fTime);
        Debug.DrawLine(vBL, vTL, _oColor, _fTime);
    }

    public void drawBounds(Bounds _oBounds, Color _oColor, float _fTime = 0.0f)
    {
        drawRectangle(_oBounds.center, _oBounds.size, _oColor, _fTime);
    }

    public void drawPolygon(Vector2[] _avVertexs, Color _oColor, float _fTime = 0.0f)
    {
        if (_avVertexs.Length > 0)
        {
            for (int i = 1; i < _avVertexs.Length; i++)
            {
                Debug.DrawLine(_avVertexs[i - 1], _avVertexs[i], _oColor, _fTime);
                Deb.Instance.drawCircle(_avVertexs[i], .1f, 8, _oColor);
            }
            Debug.DrawLine(_avVertexs[^1], _avVertexs[0], _oColor, _fTime);
            Deb.Instance.drawCircle(_avVertexs[0], .1f, 8, _oColor);
        }
    }

    public void drawTextMeshProBounds(TextMeshPro _oText)
    {
        drawRectangle(_oText.transform.posXY() + _oText.textBounds.center.xy(), _oText.textBounds.size, Color.red, 10.0f);
    }

    public void drawCircle(Vector2 _vCenter, float _fRadius, int _iSegments, Color _oColor, float _fTime = 0.0f)
    {
        drawEllipse(_vCenter, Vector3.forward, Vector3.up, _fRadius, _fRadius, _iSegments, _oColor, _fTime);
    }
    public void drawCircle(Vector2 _vCenter, float _fTime = 0.0f)
    {
        drawCircle(_vCenter, 0.3f, 10, Color.gray, _fTime);
    }
    public void drawCircle(Vector2 _vCenter, Color _oColor, float _fTime = 0.0f)
    {
        drawCircle(_vCenter, 0.3f, 10, _oColor, _fTime);
    }
    public void drawCircleXZ(Vector2 _vCenter, float _fRadius, int _iSegments, Color _oColor, float _fTime = 0.0f)
    {
        drawEllipse(_vCenter.toVector3xz(), Vector3.up, Vector3.forward, _fRadius, _fRadius, _iSegments, _oColor, _fTime);
    }
    public void drawEllipse(Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0)
    {
        float angle = 0f;
        Quaternion rot = Quaternion.LookRotation(forward, up);
        Vector3 lastPoint = Vector3.zero;
        Vector3 thisPoint = Vector3.zero;

        for (int i = 0; i < segments + 1; i++)
        {
            thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
            thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

            if (i > 0)
            {
                Debug.DrawLine((rot * lastPoint) + pos, (rot * thisPoint) + pos, color, duration);
            }

            lastPoint = thisPoint;
            angle += 360f / segments;
        }
    }

    public static void DrawConeXZ
(
    Vector3 apexXZ,          // world-space apex, Y ignored
    Vector3 forwardXZ,       // world-space forward, Y ignored, need not be normalized
    float fullAngleDeg,    // cone angle (e.g., 60°)
    float length,          // cone depth
    int segments = 12,   // how many radial lines (?2)
    Color color = default,
    float duration = 0f
)
    {
        if (segments < 2 || forwardXZ == Vector3.zero) return;

        // Prepare 2-D vectors in X-Z.
        Vector2 apex2 = new Vector2(apexXZ.x, apexXZ.z);
        Vector2 fwd2 = new Vector2(forwardXZ.x, forwardXZ.z).normalized;
        float halfRad = Mathf.Deg2Rad * fullAngleDeg * 0.5f;

        // Pre-compute right-hand basis vector in the plane.
        Vector2 right2 = new Vector2(fwd2.y, -fwd2.x);   // rotate 90° clockwise

        // Draw the cone’s perimeter ? arc made of line segments.
        float step = fullAngleDeg / segments;
        Vector2 lastPt = apex2 + (Quaternion.AngleAxis(-fullAngleDeg * 0.5f, Vector3.forward) *
                                  new Vector3(fwd2.x, fwd2.y, 0f)).xy() * length;

        for (int i = 1; i <= segments; i++)
        {
            float angleDeg = -fullAngleDeg * 0.5f + step * i;
            Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.forward);

            Vector3 dir3 = rot * new Vector3(fwd2.x, fwd2.y, 0f);
            Vector2 thisPt = apex2 + dir3.xy() * length;

            // Side ray from apex to perimeter
            if (i == 1 || i == segments)
                Debug.DrawLine(apex2.toVector3xz(), thisPt.toVector3xz(), color, duration);

            // Arc segment
            Debug.DrawLine(lastPt.toVector3xz(), thisPt.toVector3xz(), color, duration);

            lastPt = thisPt;
        }
    }

    #endregion

    #region GIZMO DRAW
    public static void gizmoDrawRectangleXZ(Vector2 _v2Center, Vector2 _v2Size, float _fAngle)
    {
        Vector2 v2Forward = Vector2.left.rotateClockwise(_fAngle);
        Vector2 v2Left = Vector2.up.rotateClockwise(_fAngle);
        Vector3 v3HalfWidthForward = v2Forward.toVector3xz() * _v2Size.x / 2f;
        Vector3 v3HalfHeightLeft = v2Left.toVector3xz() * _v2Size.y / 2f;
        Vector3 v3Center = _v2Center.toVector3xz();
        Vector3[] av3Vertices =
        {
            v3Center + v3HalfWidthForward + v3HalfHeightLeft,
            v3Center - v3HalfWidthForward + v3HalfHeightLeft,
            v3Center - v3HalfWidthForward - v3HalfHeightLeft,
            v3Center + v3HalfWidthForward - v3HalfHeightLeft
        };
        Gizmos.DrawLineStrip(av3Vertices, true);
    }

    public static void gizmoDrawCircleXZ(Vector2 _v2Center, float _fRadius)
    {
        const int iVerticesCount = 9;

        Vector3[] av3Vertices = new Vector3[iVerticesCount];
        for (int iVertexIndex = 0; iVertexIndex < iVerticesCount; ++iVertexIndex)
        {
            av3Vertices[iVertexIndex] = (_v2Center + Vector2.left.rotateClockwise(360f * (float)(iVertexIndex) / (float)iVerticesCount)).toVector3xz();
        }

        Gizmos.DrawLineStrip(av3Vertices, true);
    }
    #endregion

    #region HUDFPS



    #endregion

    #region CHEATS

    public void changeTimeScale()
    {
#if DEBUG
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Time.timeScale = 4;
        }
#endif
    }

    public static bool getPref(string _sCaption)
    {
#if UNITY_EDITOR
        return EditorPrefs.GetBool(_sCaption);
#else
        return false;
#endif
    }

    #endregion
}