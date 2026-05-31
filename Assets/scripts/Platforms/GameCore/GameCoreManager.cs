#if UNITY_GAMECORE || UNITY_GAMECOREPC

using System;
using Unity.XGamingRuntime;

public class GameCoreManager : PlatformManagerSingleton<GameCoreManager>
{
    #region Declarations
    GameSaveManager m_oSaveGameManager;
    private bool m_bError = false;

    private bool m_bOnResumeAdded = false;
    private bool m_bInitialLoadDone = false;
    #endregion

    #region Initialize
    public override void Initialize()
    {
        base.Initialize();

        m_bError = false;
        m_bInitialized = false;

#if UNITY_GAMECORE_SCARLETT || UNITY_GAMECORE_XBOXONE 
        if (!m_bOnResumeAdded)
        {
            m_bOnResumeAdded = true;
            UnityEngine.WindowsGames.WindowsGamesPLM.OnResumingEvent += onResume;
        }
#endif

        if (GDKGameRuntime.TryInitialize())
        {
            UserManager.InitializeAndAddUser(OnUserAdded, OnUserLoggedOut, OnUserError);
        }
        else
        {
            OnScreenLog.Add("GDKGameRuntime.TryInitialize ERROR");
            m_bError = true;
            m_bInitialized = true;
        }
    }

    private void OnUserAdded()
    {
        OnScreenLog.Add("OnUserAdded");

        m_oSaveGameManager = new GameSaveManager();
        m_oSaveGameManager.Initialize(UserManager.CurrentUserHandle, GDKGameRuntime.GameConfigScid, false, OnInitializeSaveGamesComplete);

        EventManager.Instance.TriggerEvent("OnDisplayInfoChanged");
    }

    private void OnInitializeSaveGamesComplete(int hresult)
    {
        m_bInitialized = true;

        if (HR.FAILED(hresult))
        {
            // Enable offline mode
            if (hresult == HR.E_GS_USER_CANCELED)
            {
                OnScreenLog.Add($"XGameSave initialized in offline mode.");
                m_bError = true;
                return;
            }
            else
            {
                OnScreenLog.AddError($"Error when initializing XGameSave. HResult 0x{hresult:x} ({HR.NameOf(hresult)})");
                m_bError = true;
                return;
            }
        }

        OnScreenLog.Add("OnInitializeSaveGamesComplete.End");
    }
    #endregion

    #region Resume/Reload
    private void onResume(double secondsSuspended)
    {
        OnScreenLog.Add("Resume");
        Initialize();
    }

    public void reload()
    {
        OnUserLoggedOut();
    }
    #endregion

    #region User
    private void OnUserLoggedOut()
    {
        OnScreenLog.Add("OnUserLoggedOut");

        if (GDKGameRuntime.Initialized)
        {
            UserManager.InitializeAndAddUser(OnUserAdded, OnUserLoggedOut, OnUserError);
        }
    }

    private void OnUserError()
    {
        OnScreenLog.Add("OnUserError");

        m_bError = true;
        m_bInitialized = true;
    }

    public bool hasUser()
    {
        return !m_bError && UserManager.CurrentUserHandle != null;
    }

    public string getUsername()
    {
        try
        {
            return UserManager.CurrentUserGamerTag;
        }
        catch (Exception) { }
        return "";
    }
    #endregion

    #region Achievements
    public override void UnlockChallenge(Achievement _oAchievement)
    {
        base.UnlockChallenge(_oAchievement);

        ulong xuid;

        int hResult = SDK.XUserGetId(UserManager.CurrentUserHandle, out xuid);
        if (HR.FAILED(hResult))
        {
            OnScreenLog.AddError($"FAILED: Could not get user ID, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        string sId = _oAchievement.m_sID.ToString();
        OnScreenLog.Add($"UnlockChallenge: {xuid} - {sId}");

        // This API will work even when offline.  Offline updates will be posted by the system when connection is
        // re-established even if the title isn�t running. If the achievement has already been unlocked or the progress
        // value is less than or equal to what is currently recorded on the server HTTP_E_STATUS_NOT_MODIFIED (0x80190130L)
        // will be returned.
        SDK.XBL.XblAchievementsUpdateAchievementAsync(
            UserManager.CurrentUserXblContextHandle,
            xuid,
            sId,
            100,
            UnlockAchievementComplete
        );
    }

    private void UnlockAchievementComplete(int hResult)
    {
        string message = "Achievement Unlocked!";
        OnScreenLog.Add($"UnlockAchievementComplete");

        if (hResult == HR.HTTP_E_STATUS_NOT_MODIFIED)
        {
            message = "Achievement ALREADY Unlocked!";
        }
        else if (HR.FAILED(hResult))
        {
            OnScreenLog.AddError($"FAILED: Achievement Update, hResult=0x{hResult:X} ({HR.NameOf(hResult)})");
            return;
        }

        OnScreenLog.Add($"SUCCESS: {message}");
    }
    #endregion

    #region Load
    protected override void LoadFile(string _sFilename)
    {
        base.LoadFile(_sFilename);

        OnScreenLog.Add("LoadFile: " + _sFilename);

        if (m_bError)
        {
            LoadDone(null);
            return;
        }

        var containerName = _sFilename;
        var blobBufferName = _sFilename;

        OnScreenLog.Add($"Loading from Slot 0. Container: {containerName}. blob Name: {_sFilename}");

        m_oSaveGameManager.GetOrCreateContainer(containerName,
        (hresult) =>
        {
            if (HR.FAILED(hresult))
            {
                OnScreenLog.Add("LoadFile error");
                LoadDone(null);
                return;
            }

            OnScreenLog.Add("GameCore.LoadFile: " + _sFilename);
            m_oSaveGameManager.LoadGame(blobBufferName, onLoadCompleted);
        });
    }

    private void onLoadCompleted(int hresult, XGameSaveBlob[] blobs)
    {
        OnScreenLog.Add("onLoadCompleted: " + hresult);
        if (HR.FAILED(hresult))
        {
            OnScreenLog.AddError($"Error when loading GameSave. HResult 0x{hresult:x}");
            LoadDone(null);
            return;
        }

        // For the effects of this sampe, we only expect one blob
        if (blobs.Length > 0)
        {
            OnScreenLog.Add($"Loading data buffer successful. Name: {blobs[0].Info.Name} - Size: {blobs[0].Info.Size} bytes");
            try
            {
                LoadDone(Utils.loadObjectFromBuffer<object>(blobs[0].Data));
            }
            catch (Exception)
            {
                LoadDone(null);
            }
        }
    }
    #endregion

    #region Save
    protected override bool UseThreadedSave() { return true; }

    protected override void Save(SaveRequest _oSaveRequest)
    {
        base.Save(_oSaveRequest);

        if (m_bError) return;

        var containerName = _oSaveRequest.m_filename;
        var blobBufferName = _oSaveRequest.m_filename;

        var characterDataBytes = Utils.getMemoryStreamOf(_oSaveRequest.m_object).GetBuffer();
        var displayName = _oSaveRequest.m_filename;

        m_oSaveGameManager.GetOrCreateContainer(containerName,
                    (hresult) =>
                    {
                        if (HR.FAILED(hresult))
                        {
                            return;
                        }

                        m_oSaveGameManager.SaveGame(displayName,
                                                    blobBufferName,
                                                    characterDataBytes,
                                                    (hresult) => OnSaveGameCompleted(hresult, containerName, blobBufferName));
                    });
    }

    private void OnSaveGameCompleted(int hresult, string containerName, string blobBufferName)
    {
        if (HR.FAILED(hresult))
        {
            OnScreenLog.AddError($"Error when Saving Game. HResult 0x{hresult:x}");
            return;
        }

        OnScreenLog.Add($"Saved data succesfully on container {containerName} and data bufer {blobBufferName}");
    }
    #endregion

    #region MISC
    //public override bool isUseLoadingScreen()
    //{
    //    return SDK.XSystemGetDeviceType() == XSystemDeviceType.XboxOneS;
    //}
    #endregion
}
#endif