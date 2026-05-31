using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
[ExecuteInEditMode]
public class HierarchyManager : MonoBehaviour
{
    [SerializeField] Color m_cDefaultBackgroundColor = new Color(0.22f, 0.22f, 0.22f, 1.0f);
    [HorizontalGroup("header", Width = 60, MarginRight = 3), LabelWidth(40), LabelText("active")]
    public bool m_bActive;
    [HorizontalGroup("header", Width = 110), LabelWidth(75), LabelText("update freq.")]
    public float m_fUpdateComponentsFrequency = 1.0f;


    public readonly static float ICON_WIDTH = 18.0f;
    public readonly static float RIGHT_MARGIN = 18.0f;

    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference] public List<HierarchyStyleConfig> m_aoStyles = new List<HierarchyStyleConfig>();

    [ListDrawerSettings(ShowFoldout = true)]
    [SerializeReference] public List<HierarchyIconConfig> m_aoIcons = new List<HierarchyIconConfig>();

    #region FLOW STUFF
    HierarchyManager() { initialize(); }

    void OnValidate() { initialize(); }

    void LateUpdate()
    {
        if (m_bActive)
        {
            // if last frame update was activated, disable it
            if (m_bUpdateNeeded)
            {
                m_bUpdateNeeded = false;
            }

            EditorApplication.RepaintHierarchyWindow();
            m_bUpdateNeeded = m_oUpdateComponentsTimer.hasPassedTime(m_fUpdateComponentsFrequency);
            if (m_bUpdateNeeded)
            {
                m_oUpdateComponentsTimer.start();
            }
        }
    }

    void initialize()
    {
        if (!m_bActive) return;

        m_oUpdateComponentsTimer.start(-m_fUpdateComponentsFrequency);
        m_aoObjectStyles.Clear();

        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyHighlight_OnGUI;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyHighlight_OnGUI;
    }
    #endregion

    #region PAINT STUFF
    Dictionary<GameObject, (HierarchyStyleConfig, bool)> m_aoObjectStyles = new Dictionary<GameObject, (HierarchyStyleConfig, bool)>();
    Timer m_oUpdateComponentsTimer = new Timer(eType.DontTrack);
    bool m_bUpdateNeeded = false;
    void HierarchyHighlight_OnGUI(int inSelectionID, Rect _oInSelectionRect)
    {
        GameObject oGO = EditorUtility.EntityIdToObject(inSelectionID) as GameObject;

        if (oGO != null)
        {
            bool bEnabled = oGO.activeInHierarchy;
            string sName = oGO.name;

            HierarchyStyleConfig oSelectedStyle = null;
            // check style
            if (m_bUpdateNeeded)
            {
                bool bStyleFound = false;
                for (int i = 0; i < m_aoStyles.Count; ++i)
                {
                    Component oComponent = oGO.GetComponent(m_aoStyles[i].m_sType);
                    if (oComponent != null)
                    {
                        // check if its enabled in case is a monobehavior
                        if (bEnabled && oComponent is MonoBehaviour)
                        {
                            bEnabled &= (oComponent as MonoBehaviour).enabled;
                        }

                        oSelectedStyle = m_aoStyles[i];
                        m_aoObjectStyles[oGO] = (m_aoStyles[i], bEnabled);
                        bStyleFound = true;
                        break;
                    }
                }

                if (!bStyleFound && m_aoObjectStyles.ContainsKey(oGO))
                {
                    m_aoObjectStyles.Remove(oGO);
                }
            }
            else
            {
                if (m_aoObjectStyles.ContainsKey(oGO))
                {
                    oSelectedStyle = m_aoObjectStyles[oGO].Item1;
                    bEnabled = m_aoObjectStyles[oGO].Item2;
                }
            }

            //Do something only for the defined styles
            if (oSelectedStyle == null)
            {
                return;
            }

            bool bPrefab = PrefabUtility.GetNearestPrefabInstanceRoot(oGO) == oGO;
            bool bIsSelected = Selection.entityIds.Contains(inSelectionID);
            Rect Offset = new Rect(_oInSelectionRect.position + new Vector2(ICON_WIDTH, 0f), _oInSelectionRect.size);
            Rect BackgroundOffset = new Rect(_oInSelectionRect.position + new Vector2(ICON_WIDTH, 0f), _oInSelectionRect.size);
            Rect LeftIconRect = Offset;
            Rect RightIconRect = Offset;
            RightIconRect.x = _oInSelectionRect.x + _oInSelectionRect.width;
            LeftIconRect.width = ICON_WIDTH;
            RightIconRect.width = ICON_WIDTH;

            if (bPrefab)
            {
                RightIconRect.x -= RIGHT_MARGIN;
            }

            // icons
            Rect oIconRect;
            int iLeftIcons = 0;
            int iRightIcons = 0;
            for (int i = 0; i < m_aoIcons.Count; ++i)
            {
                Component oComponent = oGO.GetComponent(m_aoIcons[i].m_sType);
                if (oComponent != null)
                {
                    // calculate position of the icon
                    if (m_aoIcons[i].m_bRightOrLeft)
                    {
                        oIconRect = RightIconRect;
                        oIconRect.x -= ICON_WIDTH * iRightIcons;
                        drawIcon(bIsSelected, oSelectedStyle, m_aoIcons[i].m_oIcon, oIconRect);
                        ++iRightIcons;
                    }
                    else
                    {
                        oIconRect = LeftIconRect;
                        oIconRect.x += ICON_WIDTH * iLeftIcons;
                        drawIcon(bIsSelected, oSelectedStyle, m_aoIcons[i].m_oIcon, oIconRect);
                        ++iLeftIcons;
                        Offset.x += ICON_WIDTH;
                        BackgroundOffset.x += ICON_WIDTH;
                    }
                }
            }

            // now that we know the exact starting position, calculate also the right limit
            float fExtraOffset = (iRightIcons + iLeftIcons) * ICON_WIDTH;

            if (iLeftIcons > 0)
            {
                //fExtraOffset += 2.0f;
                Offset.x += 2.0f;
                Offset.x += 2.0f;
            }
            BackgroundOffset.width -= fExtraOffset;

            // and draw the text with style
            drawStyle(bIsSelected, oSelectedStyle, sName, Offset, BackgroundOffset, bPrefab, bEnabled);
        }
    }

    void drawStyle(bool _bSelected, HierarchyStyleConfig _oConfig, string _sName, Rect _oOffset, Rect _oBackgroundOffset, bool _bPrefab, bool _bActive)
    {
        if (_bPrefab)
        {
            _oBackgroundOffset.width -= RIGHT_MARGIN;
        }

        drawRectangle(_bSelected, _oConfig, _oBackgroundOffset);

        // draw characters
        EditorGUI.LabelField(_oOffset, _sName, new GUIStyle()
        {
            normal = new GUIStyleState() { textColor = _bActive ? _oConfig.m_oEnabled : _oConfig.m_oDisabled },
            fontStyle = _oConfig.getFontStyle()
        });
    }

    void drawIcon(bool _bSelected, HierarchyStyleConfig _oConfig, Texture2D _oIcon, Rect _oRect)
    {
        // first, clean the background
        drawRectangle(_bSelected, _oConfig, _oRect);

        // draw the icon
        GUI.Label(_oRect, _oIcon);
    }

    void drawRectangle(bool _bSelected, HierarchyStyleConfig _oConfig, Rect _oRect)
    {
        // draw background first
        Color oBackgroundColor = m_cDefaultBackgroundColor;
        if (_oConfig.m_oBackground.a > 0.0f)
        {
            oBackgroundColor = _oConfig.m_oBackground;
        }

        if (_bSelected)
        {
            Color oColor = Color.Lerp(GUI.skin.settings.selectionColor, oBackgroundColor, 0.3f);
            oColor.a = 1.0f;
            EditorGUI.DrawRect(_oRect, oColor);
        }
        else
        {
            EditorGUI.DrawRect(_oRect, oBackgroundColor);
        }
    }

    #endregion

    [MenuItem("UPTools/Enable HierarchyManager", false, 100)]
    [Button]
    public static void enableHierarchyManager()
    {
        HierarchyManager oInstance = FindAnyObjectByType<HierarchyManager>();
        oInstance.m_bActive = true;
        oInstance.initialize();
    }

    [MenuItem("UPTools/Disable HierarchyManager", false, 101)]
    [Button]
    public static void disableHierarchyManager()
    {
        HierarchyManager oInstance = FindAnyObjectByType<HierarchyManager>();
        oInstance.m_bActive = false;
        EditorApplication.hierarchyWindowItemOnGUI -= oInstance.HierarchyHighlight_OnGUI;
    }
}
#else

public class HierarchyManager : SceneSingleton<HierarchyManager> {}

#endif