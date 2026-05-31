#if UNITY_PS4

using System;
using System.Collections.Generic;

public class PS4Manager : PlatformManagerSingleton<PS4Manager>
{
    #region declarations
    static public UInt64 TestBlockSize = Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN + ((1024 * 1024 * 4) / Sony.PS4.SaveData.Mounting.MountRequest.BLOCK_SIZE);

    UnityEngine.PS4.PS4Input.LoggedInUser m_oLoggedInUser;
    int m_iUserId = -1;
    string m_sDirname;

    private bool m_bCheckDialog = false;
    private bool m_bShowCorrupt = false;
    private bool m_bShowNoSpace = false;
    private bool m_bDialogOpen = false;
    private int m_iDialogId = -1;
    Sony.PS4.SaveData.Dialogs.OpenDialogResponse m_oOpenDialogResponse;
    #endregion

    #region Main
    public override void Initialize()
    {
        base.Initialize();

        OnScreenLog.Add("PS4 initialize");

        initNPToolkit();
        initSaveSystem();
        Sony.PS4.Dialog.Main.Initialise();
        UnityEngine.PS4.Utility.onSystemServiceFlagEvent += onSystemServiceFlagEvent;
    }

    public override void UpdatePlatform()
    {
        if (!m_bInitialized)
        {
            //OnScreenLog.Add("Pad: " + UnityEngine.PS4.PS4Input.PadIsConnected(0));
            if (UnityEngine.PS4.PS4Input.PadIsConnected(0))
            {
                m_oLoggedInUser = UnityEngine.PS4.PS4Input.RefreshUsersDetails(0);
                OnScreenLog.Add("UpdatePlatform: User: " + m_oLoggedInUser.userId + " -> " + m_oLoggedInUser.userName);
                m_bInitialized = true;
                if (m_iUserId == -1)
                {
                    m_iUserId = m_oLoggedInUser.userId;
                }
            }
        }

        updateDialog();

        //Check system UI to pause the game
        if (UnityEngine.PS4.Utility.isInBackgroundExecution || UnityEngine.PS4.Utility.isSystemUiOverlaid)
        {
            EventManager.Instance.TriggerEvent("PauseGame");
        }
    }
    #endregion

    #region Dialog
    void updateDialog()
    {
        Sony.PS4.Dialog.Main.Update();

        if (!m_bDialogOpen)
        {
            if (m_bShowCorrupt)
            {
                OnScreenLog.Add("Show Corrupt");
                showSystemMessageDialog(Sony.PS4.SaveData.Dialogs.DialogType.Load, Sony.PS4.SaveData.Dialogs.SystemMessageType.CorruptedAndDelete);
                m_bShowCorrupt = false;
                m_bCheckDialog = true;
            }
            else if (m_bShowNoSpace)
            {
                OnScreenLog.Add("Show No space");
                showSystemMessageDialog(Sony.PS4.SaveData.Dialogs.DialogType.Save, Sony.PS4.SaveData.Dialogs.SystemMessageType.NoSpaceContinuable);
                m_bShowNoSpace = false;
            }
        }

        if (m_bCheckDialog && !m_bDialogOpen)
        {
            m_bCheckDialog = false;
            if (m_loading)
            {
                LoadDone(null);
            }
        }
    }

    public void showSystemMessageDialog(Sony.PS4.SaveData.Dialogs.DialogType _eType, Sony.PS4.SaveData.Dialogs.SystemMessageType _eMessage)
    {
        OnScreenLog.Add("SystemMessageDialog: " + _eMessage);
        try
        {
            Sony.PS4.SaveData.Dialogs.OpenDialogRequest request = new Sony.PS4.SaveData.Dialogs.OpenDialogRequest();

            request.UserId = m_iUserId;
            request.Mode = Sony.PS4.SaveData.Dialogs.DialogMode.SystemMsg;
            request.DispType = _eType;

            Sony.PS4.SaveData.Dialogs.SystemMessageParam msg = new Sony.PS4.SaveData.Dialogs.SystemMessageParam();
            msg.SysMsgType = _eMessage;

            request.SystemMessage = msg;

            if (_eMessage == Sony.PS4.SaveData.Dialogs.SystemMessageType.CorruptedAndDelete || _eMessage == Sony.PS4.SaveData.Dialogs.SystemMessageType.Corrupted)
            {
                Sony.PS4.SaveData.DirName[] dirNames = new Sony.PS4.SaveData.DirName[1];
                dirNames[0] = new Sony.PS4.SaveData.DirName();
                dirNames[0].Data = m_sDirname;
                Sony.PS4.SaveData.Dialogs.Items items = new Sony.PS4.SaveData.Dialogs.Items();
                items.DirNames = dirNames;
                request.Items = items;
            }

            m_oOpenDialogResponse = new Sony.PS4.SaveData.Dialogs.OpenDialogResponse();

            m_iDialogId = Sony.PS4.SaveData.Dialogs.OpenDialog(request, m_oOpenDialogResponse);
            m_bDialogOpen = true;

            OnScreenLog.Add("Dialog Opened: " + m_iDialogId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("SystemMessageDialog Exception: " + e.ExtendedMessage);
        }
    }
    #endregion

    #region NPToolkit
    public void initNPToolkit()
    {
        Sony.NP.Main.OnAsyncEvent += main_OnAsyncEvent;
        Sony.NP.InitToolkit init = new Sony.NP.InitToolkit();

        init.contentRestrictions.DefaultAgeRestriction = 3;

        // you can set affinity to other cores this way: Sony.NP.Affinity.Core2 | Sony.NP.Affinity.Core4;
        init.threadSettings.affinity = Sony.NP.Affinity.AllCores;
        init.SetPushNotificationsFlags(Sony.NP.PushNotificationsFlags.None);

        //For this example we use the first user
        m_oLoggedInUser = UnityEngine.PS4.PS4Input.RefreshUsersDetails(0);// RefreshUsersDetails(0);
        OnScreenLog.Add("initNPToolkit: User: " + m_oLoggedInUser.userId + ", " + m_oLoggedInUser.userName);

        try
        {
            Sony.NP.Main.Initialize(init);
            registerTrophyPack();
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.Add("Error initializing the NPToolkit2 : " + e.ExtendedMessage);
        }
    }

    private void registerTrophyPack()
    {
        try
        {
            Sony.NP.Trophies.RegisterTrophyPackRequest request = new Sony.NP.Trophies.RegisterTrophyPackRequest();

            request.UserId = m_oLoggedInUser.userId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which returns the Request Id 
            int requestId = Sony.NP.Trophies.RegisterTrophyPack(request, response);
            OnScreenLog.Add("RegisterTrophyPack Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.Add("Exception : " + e.ExtendedMessage);
        }
    }

    // NOTE : This is called on the "Sony NP" thread and is independent of the Behaviour update.
    // This thread is created by the SonyNP.dll when NpToolkit2 is initialized.
    private void main_OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.ApiCalled == Sony.NP.FunctionTypes.NotificationUserStateChange)
        {
            OnScreenLog.Add("Set user: " + callbackEvent.UserId.Id);
            if (m_iUserId == -1)
            {
                m_iUserId = callbackEvent.UserId.Id;
            }
        }
        //Print some useful info on the current event: 
        OnScreenLog.Add("Event: Service = (" + callbackEvent.Service + ") : API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.NpRequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");

        // handle event
        try
        {
            if (callbackEvent.Response != null)
            {
                //We got an error response 
                if (callbackEvent.Response.ReturnCodeValue < 0)
                {
                    OnScreenLog.Add("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                }
                else
                {
                    //The callback of the event is a trophyUnlock event
                    if (callbackEvent.ApiCalled == Sony.NP.FunctionTypes.TrophyUnlock)
                    {
                        OnScreenLog.Add("Trophy Unlock : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                    }
                }
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.Add("Main_OnAsyncEvent Exception = " + e.ExtendedMessage);
        }
    }

    public override void UnlockChallenge(Achievement achievement)
    {
        try
        {
            Sony.NP.Trophies.UnlockTrophyRequest request = new Sony.NP.Trophies.UnlockTrophyRequest();
            request.TrophyId = int.Parse(achievement.m_sID);
            request.UserId = m_oLoggedInUser.userId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the asynchronous call which returns the Request Id 
            int requestId = Sony.NP.Trophies.UnlockTrophy(request, response);
            OnScreenLog.Add("GetUnlockedTrophies Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.Add("Exception : " + e.ExtendedMessage);
        }
    }
    #endregion

    #region SAVE / LOAD
    void initSaveSystem()
    {
        Sony.PS4.SaveData.Main.OnAsyncEvent += save_OnAsyncEvent;

        try
        {
            Sony.PS4.SaveData.InitSettings settings = new Sony.PS4.SaveData.InitSettings();

            settings.Affinity = Sony.PS4.SaveData.ThreadAffinity.AllCores;
            //settings.Affinity = Sony.PS4.SaveData.ThreadAffinity.Core5;

            Sony.PS4.SaveData.InitResult initResult = Sony.PS4.SaveData.Main.Initialize(settings);

            if (initResult.Initialized == true)
            {
                OnScreenLog.Add("SaveData Initialized ");
                //OnScreenLog.Add("Plugin SDK Version : " + initResult.SceSDKVersion.ToString());
            }
            else
            {
                OnScreenLog.Add("SaveData not initialized ");
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception During Initialization : " + e.ExtendedMessage);
        }
#if UNITY_EDITOR
        catch (DllNotFoundException e)
        {
            OnScreenLog.AddError("Missing DLL Expection : " + e.Message);
            OnScreenLog.AddError("The sample APP will not run in the editor.");
        }
#endif
    }

    private void save_OnAsyncEvent(Sony.PS4.SaveData.SaveDataCallbackEvent callbackEvent)
    {
        OnScreenLog.Add("save_OnAsyncEvent (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.RequestId + ") : Calling User Id = (0x" + callbackEvent.UserId.ToString("X8") + ")");
        OnScreenLog.Add("HandleAsynEvent: " + callbackEvent.Response);

        try
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.PS4.SaveData.FunctionTypes.NotificationDialogClosed:
                    if (m_bDialogOpen && callbackEvent.RequestId == m_iDialogId)
                    {
                        m_bDialogOpen = false;
                    }
                    break;

                case Sony.PS4.SaveData.FunctionTypes.FileOps:
                    {
                        if (callbackEvent.Request is PS4WriteFileRequest)
                        {
                            OnScreenLog.Add("Write Request");
                        }
                        else if (callbackEvent.Request is PS4ReadFileRequest)
                        {
                            OnScreenLog.Add("Load Request");
                            try
                            {
                                LoadDone(Utils.loadObjectFromBuffer<object>((callbackEvent.Response as PS4ReadFileResponse).largeData));
                                OnScreenLog.Add("Load Done");
                            }
                            catch (Exception e)
                            {
                                OnScreenLog.Add("Load null: " + e.Message);
                                LoadDone(null);
                            }
                        }
                    }
                    break;
            }
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent SaveData Exception = " + e.ExtendedMessage);
            OnScreenLog.AddError(e.StackTrace);
        }
        catch (Exception e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent General Exception = " + e.Message);
            OnScreenLog.AddError(e.StackTrace);
        }
    }

    protected override void LoadFile(string _sFilename)
    {
        OnScreenLog.Add($"LoadFile({m_iUserId}): {_sFilename}");

        Sony.PS4.SaveData.DirName oDirname = new Sony.PS4.SaveData.DirName();
        oDirname.Data = _sFilename;
        m_sDirname = _sFilename;

        PS4ReadFileRequest oFileRequest = new PS4ReadFileRequest();
        oFileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        PS4ReadFileResponse oFileResponse = new PS4ReadFileResponse();

        GameCommon.Instance.StartCoroutine(PS4AutoSave.load(m_iUserId, oDirname,
            oFileRequest, oFileResponse, handleAutoSaveError));
    }

    protected override bool UseThreadedSave() { return false; }// true; }
    //protected override float getTimeBetweenSaves() { return 5f; }
    //protected override float getSaveDelay() { return 0.5f; }

    public override bool IsSaving()
    {
        return PS4AutoSave.ms_oCurrentState != PS4AutoSave.SaveState.IDLE || m_bDialogOpen || m_bShowCorrupt || m_bShowNoSpace;
    }

    protected override void Save(SaveRequest _oSaveRequest)
    {
        OnScreenLog.Add($"PS4Manager.Save({m_iUserId}): {_oSaveRequest.m_filename}");

        // Create the new item for the saves dialog list
        Sony.PS4.SaveData.Dialogs.NewItem oNewItem = new Sony.PS4.SaveData.Dialogs.NewItem();
        oNewItem.IconPath = "/app0/Media/StreamingAssets/PS4SaveIcon.png";
        oNewItem.Title = _oSaveRequest.m_filename;

        // The directory name for a new savedata
        Sony.PS4.SaveData.DirName oNewDirName = new Sony.PS4.SaveData.DirName();
        oNewDirName.Data = _oSaveRequest.m_filename;

        // Parameters to use for the savedata
        Sony.PS4.SaveData.SaveDataParams oSaveDataParams = new Sony.PS4.SaveData.SaveDataParams();
        oSaveDataParams.Title = oNewItem.Title;
        oSaveDataParams.SubTitle = "";
        oSaveDataParams.Detail = "";
        oSaveDataParams.UserParam = 0;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        PS4WriteFileRequest oFileRequest = new PS4WriteFileRequest();
        oFileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        //float fTime = UnityEngine.Time.realtimeSinceStartup;
        oFileRequest.largeData = Utils.getMemoryStreamOf(_oSaveRequest.m_object).GetBuffer();
        //HUDFPS.Instance.m_fSaveTime = UnityEngine.Time.realtimeSinceStartup - fTime;

        PS4WriteFileResponse oFileResponse = new PS4WriteFileResponse();

        PS4AutoSave.save(m_iUserId, oNewItem, oNewDirName, TestBlockSize, oSaveDataParams, oFileRequest, oFileResponse, handleAutoSaveError);
    }

    void handleAutoSaveError(uint _iErrorCode)
    {
        Sony.PS4.SaveData.ReturnCodes oCode = (Sony.PS4.SaveData.ReturnCodes)_iErrorCode;

        OnScreenLog.Add("PS4Manager.handleAutoSaveError: " + oCode);

        if (m_loading)
        {
            LoadDone(null);
        }

        if (oCode == Sony.PS4.SaveData.ReturnCodes.DATA_ERROR_NO_SPACE_FS)
        {
            OnScreenLog.AddError("There is no space available for the auto-save");
            m_bShowNoSpace = true;
        }
        else if (oCode == Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
        {
            OnScreenLog.AddError("The auto-save file is corrupt");
            delete();
            m_bShowCorrupt = true;
        }
    }
    #endregion

    #region Delete Save
    public void delete()
    {
        try
        {
            Sony.PS4.SaveData.Deleting.DeleteRequest request = new Sony.PS4.SaveData.Deleting.DeleteRequest();

            Sony.PS4.SaveData.DirName dirName = new Sony.PS4.SaveData.DirName();
            dirName.Data = m_sDirname;

            request.UserId = m_iUserId;
            request.DirName = dirName;

            Sony.PS4.SaveData.EmptyResponse response = new Sony.PS4.SaveData.EmptyResponse();

            int requestId = Sony.PS4.SaveData.Deleting.Delete(request, response);

            OnScreenLog.Add("Delete Async : Request Id = " + requestId);
        }
        catch (Sony.PS4.SaveData.SaveDataException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }
    #endregion

    #region AutoUnmount
    void onSystemServiceFlagEvent(UnityEngine.PS4.Utility.SystemServiceFlag _eFlagindex, bool _bNewValue)
    {
        OnScreenLog.Add("Flagindex = " + _eFlagindex + ", Value = " + _bNewValue);

        // Whenever the SystemUiOverlaid is true when it is possible the PS button has been used to open the "Quick Menu". 
        // If "Close Application" is used while a save data is in a read/write state then the PS4 will hang waiting for something to close those mount points.
        // Therefore it is very important for this sample app to make sure these mount points are closed. 
        // un normal circumstances this won't happen in a real app as mounting for read/write and then unmounting should be handled in a timely manor and unmounting shouldn't require any interaction
        // with the user.
        if (_eFlagindex == UnityEngine.PS4.Utility.SystemServiceFlag.SystemUiOverlaid)
        {
            List<Sony.PS4.SaveData.Mounting.MountPoint> mountPoints = Sony.PS4.SaveData.Mounting.ActiveMountPoints;

            for (int iIndex = 0; iIndex < mountPoints.Count; iIndex++)
            {
                Sony.PS4.SaveData.Mounting.MountPoint oMountPoint = mountPoints[iIndex];

                if (oMountPoint.IsMounted == true && (oMountPoint.MountMode & Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite) != 0)
                {
                    OnScreenLog.Add("Automatically Unmounting " + oMountPoint.DirName.Data);
                    Sony.PS4.SaveData.EmptyResponse oResponse = new Sony.PS4.SaveData.EmptyResponse();
                    PS4AutoSave.unmountSaveData(m_iUserId, oResponse, oMountPoint, true);
                }
            }
        }
    }
    #endregion
}
#endif
