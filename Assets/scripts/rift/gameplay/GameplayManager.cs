using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum eTimeOfDay
{
    Morning = 1 << 0,
    Noon = 1 << 1,
    Evening = 1 << 2,
    Night = 1 << 3,
}

public enum eGameLocation
{
    MainMenu,
    XanderTower,
    CrystalGems=3,
    Map,
    TeamScreeInventory,
    TeamScreenSkills,
    Campfire,
    Node,
    Compendium,
    Settings,
    PauseMenu,
    EndRunScreen,
    RemoveHeroScreen,
    Intro,
    LoadingScreen,
    Store,
    UNUSED,
    ClosingGame,
    None,
    Logo,
    InitialCrystalGem,
    MapInfo,
    MapTreasure,
    MapArc,
    Loading = 26
}

public class GameplayManager : SceneSingleton<GameplayManager>
{
    public const string FORCE_LEVEL = "ForceLevel";

    public const string RUN_SEED = "RUN";
    public const string MAP_SEED = "MAP";
    public const string NODE_SEED = "NODE";
    public const string COLOR_SEED = "COLOR";
    public const string SCENE_SEED = "SCENE";

    [NonSerialized] public bool m_bInNodeScene = false;
    [NonSerialized] public bool m_bAbandonRun = false; // cuando le das a abandonRun
    [NonSerialized] public bool m_bExitRun = false; // cuando le das a save&exit

    [Header("cameras")]
    public Camera m_oManagementCamera;
    public Camera m_oCombatCamera;
    [SerializeField] Camera m_oRenderTextureCamera;
    public Transform m_oCombatCameraTransform;
    public Transform m_oCombatCameraShake;
    Vector3 m_oCombatCameraDefaultPosition;
    float m_oCombatCameraDefaultOrtographicSize;

    Light m_oSunLight;

    [ReadOnly, ShowInInspector] List<Tag> m_asTags = new List<Tag>();
    List<GameObject> m_aoGOToDisable = new List<GameObject>();

    [SerializeField, ReadOnly] private eGameLocation m_eGameLocation = eGameLocation.None;
    [SerializeField, ReadOnly] private eGameLocation m_eLastGameLocation = eGameLocation.None;

    #region BASIC

    protected override void Awake()
    {
        base.Awake();

        m_oCombatCameraDefaultPosition = m_oCombatCamera.transform.position;
        m_oCombatCameraDefaultOrtographicSize = m_oCombatCamera.orthographicSize;
    }

    private void OnEnable()
    {
        EventManager.Instance.StartListening(EventsType.DisableGameobjects, disableGameobjects);
        EventManager.Instance.StartListening(EventsType.ResetCombatCameraPosition, resetCombatCameraPosition);
    }

    private void OnDisable()
    {
        if (EventManager.isNull()) { return; }

        EventManager.Instance.StopListening(EventsType.DisableGameobjects, disableGameobjects);
        EventManager.Instance.StopListening(EventsType.ResetCombatCameraPosition, resetCombatCameraPosition);
    }

    void disableGameobjects()
    {
        for (int i = 0; i < m_aoGOToDisable.Count; i++)
        {
            m_aoGOToDisable[i].gameObject.SetActive(false);
        }
    }

    void resetCombatCameraPosition()
    {
        m_oCombatCameraTransform.setPositionX(0);
        m_oCombatCameraTransform.setPositionY(0);
    }

    public void initialize()
    {
        m_oCombatCamera = GameObject.Find("CombatCamera").GetComponent<Camera>();
        m_oManagementCamera = GameObject.Find("ManagementCamera").GetComponent<Camera>();
        m_oCombatCamera.depth = 0;
        m_oManagementCamera.depth = 1;

        ShakeManager.Instance.m_vInitialManagementCameraPos = m_oManagementCamera.transform.localPosition;
        ShakeManager.Instance.m_vInitialCombatCameraPos = m_oCombatCameraShake.localPosition;
    }

    public override void update()
    {
        base.update();
    }

    public void adjustCameraToTargetAspectRatio(in Resolution _oMaximumResolution, bool _bFullResolution)
    {
        Rect oCameraRect = _bFullResolution ?
            new Rect(0f, 0f, 1f, 1f) :
            AspectRatioUtils.getCameraRectToTargetAspectRatio(_oMaximumResolution);
        m_oCombatCamera.rect = oCameraRect;
        m_oManagementCamera.rect = oCameraRect;
        //NOTE: We do not adjust m_oRenderTextureCamera. Instead adjust the target render to texture resolution.
    }

    #endregion

    #region
    public Vector3 getSunDirection()
    {
        return m_oSunLight.transform.forward;
    }

    public void setSun(Light _oLight)
    {
        m_oSunLight = _oLight;
    }

    public Light getSun()
    {
        return m_oSunLight;
    }

    #endregion

    #region CAMPFIRE

    private eTimeOfDay getNextNodeTimeOfDay(eTimeOfDay _eTime)
    {
        if (_eTime == eTimeOfDay.Morning) { return eTimeOfDay.Noon; }
        else if (_eTime == eTimeOfDay.Noon) { return eTimeOfDay.Evening; }
        else { return eTimeOfDay.Morning; }
    }

    private eTimeOfDay getTimeOfDayInXNodes(eTimeOfDay _eTime, int _iNodesFoward)
    {
        eTimeOfDay eTimeOfDay = _eTime;
        for (int i = 0; i < _iNodesFoward; i++)
        {
            eTimeOfDay = getNextNodeTimeOfDay(eTimeOfDay);
        }

        return eTimeOfDay;
    }

    #endregion

    #region Extras

    public void addTag(Tag _oTag) { m_asTags.addIfNotContains(_oTag); }
    public void removeTag(Tag _oTag) { m_asTags.remove(_oTag); }
    public GameObject getGameobject(string _sTag)
    {
        checkNullReferences();

        for (int i = 0; i < m_asTags.Count; i++)
        {
            Tag oTag = m_asTags[i];
            if (_sTag == oTag.getValue()) { return oTag.gameObject; }
        }

        return null;
    }

    private void checkNullReferences()
    {
        for (int i = 0; i < m_asTags.Count;)
        {
            if (m_asTags[i] == null) { m_asTags.RemoveAt(i); }
            else { i++; }
        }
    }

    public void addGameobjectToDisable(GameObject _oGO)
    {
        m_aoGOToDisable.addIfNotContains(_oGO);
    }
    public void removeGameobjectToDisable(GameObject _oGO)
    {
        m_aoGOToDisable.Remove(_oGO);
    }

    public Camera RenderTextureCamera { get { return m_oRenderTextureCamera; } }
    public void returnCombatCameraToDefaultPosition()
    {
        m_oCombatCamera.transform.position = m_oCombatCameraDefaultPosition;
        m_oCombatCamera.orthographicSize = m_oCombatCameraDefaultOrtographicSize;
    }

#endregion EXTRAS

    public void setGameLocation(eGameLocation _eGameLocation)
    {
        if (m_eGameLocation != _eGameLocation)
        {
            m_eLastGameLocation = m_eGameLocation;
            m_eGameLocation = _eGameLocation;
        }
    }

    public void setToLastGameLocation()
    {
        if (m_eGameLocation != m_eLastGameLocation)
        {
            eGameLocation eLastGameLocation = m_eGameLocation;
            m_eGameLocation = m_eLastGameLocation;
            m_eLastGameLocation = eLastGameLocation;
        }
    }

    public eGameLocation getGameLocation()
    {
        return m_eGameLocation;
    }

    public GameObject m_oLastInstantiatedObject; // should no be here
}
