using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITY_EDITOR
#endif

public class PlatformManager
{
    public virtual int MaxAvatars => 1000;
    public virtual int MaxTeams => 150;
    public virtual int MaxTournaments => 25;
    public virtual int MaxPlayers => 4;

    #region Main
    public virtual void Initialize()
    {
        m_aoLoadOperations.Clear();
        m_iCurrentIndex = -1;
        m_loading = false;
        m_fLastSaveTime = 0;
    }

    protected bool m_bInitialized = false;
    public bool IsInitialized() { return m_bInitialized; }

    public void update()
    {
        UpdatePlatform();

        UpdateLoad();

        updateSave();
    }

    public virtual void UpdatePlatform() { }
    #endregion

    #region Achievements/Trophies

    public virtual void UnlockChallenge(Achievement _oAchievement) { }

    #endregion

    #region Load

    struct LoadOperation
    {
        public object m_oLoadedObject;
        public string m_sLoadFilename;
        public bool m_bDone;
    }

    List<LoadOperation> m_aoLoadOperations = new List<LoadOperation>();
    int m_iCurrentIndex = -1;

    protected bool m_loading = false;

    public void Load(string filename)
    {
        LoadOperation oLoad;
        oLoad.m_oLoadedObject = null;
        oLoad.m_sLoadFilename = filename;
        oLoad.m_bDone = false;
        m_aoLoadOperations.Add(oLoad);

        CheckLoadNext();
    }

    public void CheckLoadNext()
    {
        if (m_bInitialized && !m_loading && !IsSaving() && m_iCurrentIndex < m_aoLoadOperations.Count - 1)
        {
            m_loading = true;
            m_iCurrentIndex++;
            LoadFile(GetCurrentFilename());
        }
    }

    void UpdateLoad()
    {
        CheckLoadNext();
    }

    protected string GetCurrentFilename()
    {
        return m_aoLoadOperations[m_iCurrentIndex].m_sLoadFilename;
    }

    protected void LoadDone(object loadedObject)
    {
        m_loading = false;
        LoadOperation oLoad = m_aoLoadOperations[m_iCurrentIndex];
        oLoad.m_oLoadedObject = loadedObject;
        oLoad.m_bDone = true;
        m_aoLoadOperations[m_iCurrentIndex] = oLoad;
    }

    protected virtual void LoadFile(string filename) { }

    LoadOperation GetLoadOperation(string filename)
    {
        for (int index = m_aoLoadOperations.Count - 1; index >= 0; --index)
        {
            if (m_aoLoadOperations[index].m_sLoadFilename.Equals(filename))
            {
                return m_aoLoadOperations[index];
            }
        }
        LoadOperation oLoad = new LoadOperation();
        return oLoad;
    }

    public bool IsLoadDone(string filename)
    {
        return GetLoadOperation(filename).m_bDone;
    }

    public TS GetLoadedGame<TS>(string filename)
    {
        //OnScreenLog.Add("GetLoadedGame: " + filename);
        LoadOperation oLoad = GetLoadOperation(filename);
        if (oLoad.m_oLoadedObject == null)
        {
            //OnScreenLog.Add("   No data");
        }

        TS loadedObject = default(TS);
        try
        {
            loadedObject = (TS)oLoad.m_oLoadedObject;
        }
        catch (System.Exception e)
        {
            Deb.logWarning("Error loading file: " + filename + e.ToString());
        }

        return loadedObject;
    }

    #endregion

    #region Save
    protected struct SaveRequest
    {
        public object m_object;
        public string m_filename;
        public float m_fTimeRequested;
    }

    protected List<SaveRequest> m_aoSaveRequests = new List<SaveRequest>();
    protected float m_fLastSaveTime = 0.0f;
    public const float TIME_BETWEEN_SAVES = 0.0f;
    public const float SAVE_DELAY = 0;

    protected Thread m_oThread = null;

    protected virtual bool UseThreadedSave() { return false; }
    protected virtual void preSave() { }
    protected virtual void postSave() { }
    protected virtual float getTimeBetweenSaves() { return TIME_BETWEEN_SAVES; }
    protected virtual float getSaveDelay() { return SAVE_DELAY; }

    private void StartSaveThread(SaveRequest _oSaveRequest)
    {
        preSave();
        m_oThread = new Thread(() => Save(_oSaveRequest));
        m_oThread.Start();
    }

    protected virtual void Save(SaveRequest _oSaveRequest) { }

    public void Save(object _oObjectToSave, string _sFileName)
    {
        if (_oObjectToSave == null)
        {
            Debug.LogError("empty save: " + _sFileName);
            return;
        }

        SaveRequest oSaveRequest = new SaveRequest();
        oSaveRequest.m_object = _oObjectToSave;
        oSaveRequest.m_filename = _sFileName;
        oSaveRequest.m_fTimeRequested = Time.realtimeSinceStartup;

        // Check for save request of the same file and remove it
        for (int iIndex = 0; iIndex < m_aoSaveRequests.Count; ++iIndex)
        {
            if (m_aoSaveRequests[iIndex].m_filename.Equals(_sFileName))
            {
                m_aoSaveRequests.RemoveAt(iIndex);
                break;
            }
        }
        m_aoSaveRequests.Add(oSaveRequest);
    }

    void updateSave()
    {
        if (m_bInitialized)
        {
            if (m_oThread != null && !m_oThread.IsAlive)
            {
                postSave();
                m_oThread = null;
            }

            if(m_oThread == null)
            {
                if (m_aoSaveRequests.Count > 0 && !IsSaving() && (Time.realtimeSinceStartup - m_fLastSaveTime) > getTimeBetweenSaves() && (Time.realtimeSinceStartup - m_aoSaveRequests[0].m_fTimeRequested) > getSaveDelay())
                {
                    SaveRequest oSaveRequest = m_aoSaveRequests[0];
                    m_aoSaveRequests.RemoveAt(0);
                    if (UseThreadedSave())
                    {
                        StartSaveThread(oSaveRequest);
                    }
                    else
                    {
                        Save(oSaveRequest);
                    }
                    m_fLastSaveTime = Time.time;
                }
            }
        }
    }

    public virtual bool IsSaving()
    {
        if (m_oThread != null)
        {
            return m_oThread.IsAlive;
        }
        return false;
    }

    public virtual bool isAnySavingLeft()
    {
        return m_aoSaveRequests.Count > 0 || IsSaving();
    }
    #endregion

    #region MISC
    public virtual float getVibrationMultiplier()
    {
        return 1.0f;
    }
    #endregion
    
    public static PlatformManager Current
#if UNITY_STANDALONE || UNITY_EDITOR
        => PCManager.Instance;
#elif UNITY_GAMECORE
        => GameCoreManager.Instance;
#elif UNITY_PS4
        => PS4Manager.Instance;
#elif UNITY_PS5
        => PS5Manager.Instance;
#elif UNITY_SWITCH
        => SwitchManager.Instance;
#endif
}
