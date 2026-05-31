#if UNITY_PS5

#region using
using System;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.UDS;
using Unity.PSN.PS5.Users;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
using Unity.SaveData.PS5.Initialization;
using Unity.SaveData.PS5.Mount;
#endregion

public class PS5Manager : PlatformManagerSingleton<PS5Manager>
{
    string m_sDirname = "";

    UnityEngine.PS5.PS5Input.LoggedInUser m_oLoggedInUser;

    private bool m_bShowCorrupt = false;
    private bool m_bShowNoSpace = false;
    private bool m_bDisableNoSpaceMessage = false;

    bool m_bInitializedUDS = false;

    #region Main
    public override void Initialize()
    {
        base.Initialize();

        OnScreenLog.Add("PS5 initialize");

        Unity.PSN.PS5.Initialization.InitResult initResult = Unity.PSN.PS5.Main.Initialize();

        if (initResult.Initialized == true)
        {
            OnScreenLog.Add("PSN Initialized ");
            OnScreenLog.Add("Plugin SDK Version : " + initResult.SceSDKVersion.ToString());
        }
        else
        {
            OnScreenLog.Add("PSN not initialized ");
        }

        UnityEngine.PS5.Utility.onSystemServiceFlagEvent += OnSystemServiceFlagEvent;

        Sony.PS5.Dialog.Main.Initialise();
    }

    void OnSystemServiceFlagEvent(UnityEngine.PS5.Utility.SystemServiceFlag flagindex, bool newValue)
    {
        //OnScreenLog.AddError("Flagindex = " + flagindex + ", Value = " + newValue);

        // Whenever the SystemUiOverlaid is true when it is possible the PS button has been used to open the "Quick Menu".
        // If "Close Application" is used while a save data is in a read/write state then the PS5 will hang waiting for something to close those mount points.
        // Therefore it is very important for this sample app to make sure these mount points are closed.
        // un normal circumstances this won't happen in a real app as mounting for read/write and then unmounting should be handled in a timely manor and unmounting shouldn't require any interaction
        // with the user.
        if (flagindex == UnityEngine.PS5.Utility.SystemServiceFlag.SystemUiOverlaid)
        {
            List<Mounting.MountPoint> mountPoints = Mounting.ActiveMountPoints;

            for (int i = 0; i < mountPoints.Count; i++)
            {
                Mounting.MountPoint mp = mountPoints[i];

                if (mp.IsMounted == true && (mp.MountMode & Mounting.MountModeFlags.ReadWrite) != 0)
                {
                    OnScreenLog.Add("Automatically Unmounting " + mp.DirName.Data);
                    EmptyResponse oResponse = new EmptyResponse();
                    PS5AutoSave.UnmountSaveData(m_oLoggedInUser.userId, oResponse, mp);
                }
            }
        }
    }

    public override void UpdatePlatform()
    {
        base.UpdatePlatform();

        Unity.PSN.PS5.Main.Update();

        if (!m_bInitialized)
        {
            //OnScreenLog.Add("Pad: " + UnityEngine.PS5.PS5Input.PadIsConnected(0));
            if (UnityEngine.PS5.PS5Input.PadIsConnected(0))
            {
                m_oLoggedInUser = UnityEngine.PS5.PS5Input.RefreshUsersDetails(0);
                OnScreenLog.Add("User: " + m_oLoggedInUser.userId + " -> " + m_oLoggedInUser.userName);
                m_bInitialized = true;

                UserSystem.AddUserRequest request = new UserSystem.AddUserRequest() { UserId = m_oLoggedInUser.userId };

                var requestOp = new AsyncRequest<UserSystem.AddUserRequest>(request).ContinueWith((antecedent) =>
                {
                    if (antecedent != null && antecedent.Request != null)
                    {
                        OnScreenLog.Add("User request");
                    }
                });

                UserSystem.Schedule(requestOp);

                OnScreenLog.Add("User being added...");

                initSaveSystem();
            }
        }

        if (!m_bInitialized) return;

        if (!m_bInitializedUDS) initUDS();

        UpdateDialog();

        //Check system UI to pause the game
        if (UnityEngine.PS5.Utility.isInBackgroundExecution || UnityEngine.PS5.Utility.isSystemUiOverlaid)
        {
            EventManager.Instance.TriggerEvent("PauseGame");
        }
    }
    #endregion

    #region Dialog
    void UpdateDialog()
    {
        Sony.PS5.Dialog.Main.Update();
        Sony.PS5.Dialog.Common.Update();

        if (!Sony.PS5.Dialog.Common.IsDialogOpen)
        {
            if (m_bShowCorrupt)
            {
                m_bShowCorrupt = false;
                showSystemMessageDialog(Dialogs.DialogType.Load, Dialogs.SystemMessageType.Corrupted);
                OnScreenLog.Add("Corrupt shown");
            }
            else if (m_bShowNoSpace)
            {
                m_bShowNoSpace = false;
                //m_bDisableNoSpaceMessage = true;
                showSystemMessageDialog(Dialogs.DialogType.Save, Dialogs.SystemMessageType.NoSpaceContinuable);
                OnScreenLog.Add("No Space shown");
            }
        }
    }

    public void showSystemMessageDialog(Dialogs.DialogType _eType, Dialogs.SystemMessageType _eMessage)
    {
        OnScreenLog.Add("SystemMessageDialog: " + _eMessage);
        try
        {
            Dialogs.OpenDialogRequest request = new Dialogs.OpenDialogRequest();

            request.UserId = m_oLoggedInUser.userId;
            request.Mode = Dialogs.DialogMode.SystemMsg;
            request.DispType = _eType;

            Dialogs.SystemMessageParam msg = new Dialogs.SystemMessageParam();
            msg.SysMsgType = _eMessage;

            request.SystemMessage = msg;

            if (_eMessage == Dialogs.SystemMessageType.Corrupted)
            {
                DirName[] dirNames = new DirName[1];
                dirNames[0] = new DirName();
                dirNames[0].Data = m_sDirname;
                Dialogs.Items items = new Dialogs.Items();
                items.DirNames = dirNames;
                request.Items = items;
            }

            Dialogs.OpenDialogResponse response = new Dialogs.OpenDialogResponse();

            int requestId = Dialogs.OpenDialog(request, response);

            OnScreenLog.Add("SystemMessageDialog: done");

        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("SystemMessageDialog Exception: " + e.ExtendedMessage);
        }
    }
    #endregion

    #region UDS (trophies)
    public void initUDS()
    {
        OnScreenLog.Add("InitNPToolkit");
        try
        {   // Start UDS
            UniversalDataSystem.StartSystemRequest request = new UniversalDataSystem.StartSystemRequest();

            request.PoolSize = 256 * 1024;

            var requestOp = new AsyncRequest<UniversalDataSystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
            {
                if (checkAysncRequestOK(antecedent))
                {
                    OnScreenLog.Add("System Started");
                }
            });

            UniversalDataSystem.Schedule(requestOp);

            m_bInitializedUDS = true;
        }
        catch (Exception e)
        {
            OnScreenLog.Add(e.ToString());
        }
    }

    public static bool checkRequestOK<R>(R request) where R : Request
    {
        if (request == null)
        {
            UnityEngine.Debug.LogError("Request is null");
            return false;
        }

        if (request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Success)
        {
            return true;
        }

        outputApiResult(request.Result);

        return false;
    }

    public static bool checkAysncRequestOK<R>(AsyncRequest<R> asyncRequest) where R : Request
    {
        if (asyncRequest == null)
        {
            UnityEngine.Debug.LogError("AsyncRequest is null");
            return false;
        }

        return checkRequestOK<R>(asyncRequest.Request);
    }

    public static void outputApiResult(Unity.PSN.PS5.APIResult result)
    {
        if (result.apiResult == Unity.PSN.PS5.APIResultTypes.Success)
        {
            return;
        }

        string output = result.ErrorMessage();

        OnScreenLog.AddError($"\n{result.sceErrorCode}\n");

        if (result.apiResult == Unity.PSN.PS5.APIResultTypes.Error)
        {
            OnScreenLog.AddError(output);
        }
        else
        {
            OnScreenLog.Add(output);
        }
    }

    public override void UnlockChallenge(Achievement _oAchievement)
    {
        UniversalDataSystem.UnlockTrophyRequest request = new UniversalDataSystem.UnlockTrophyRequest();

        request.TrophyId = int.Parse(_oAchievement.m_sID);
        request.UserId = m_oLoggedInUser.userId;

        var getTrophyOp = new AsyncRequest<UniversalDataSystem.UnlockTrophyRequest>(request).ContinueWith((antecedent) =>
        {
            if (checkAysncRequestOK(antecedent))
            {
                OnScreenLog.Add("Trophy Unlock Request finished = " + antecedent.Request.TrophyId);
            }
        });

        UniversalDataSystem.Schedule(getTrophyOp);
        OnScreenLog.Add("Trophy Unlocking");
    }
    #endregion

    #region SAVE / LOAD
    void initSaveSystem()
    {
        Unity.SaveData.PS5.Main.OnAsyncEvent += save_OnAsyncEvent;

        initializeSaveData();
    }

    void initializeSaveData()
    {
        try
        {
            InitSettings oSettings = new InitSettings();
            oSettings.Affinity = ThreadAffinity.Core5;

            InitResult oInitResult = Unity.SaveData.PS5.Main.Initialize(oSettings);
            if (oInitResult.Initialized == true)
            {
                OnScreenLog.Add("SaveData Initialized ");
            }
            else
            {
                OnScreenLog.Add("SaveData not initialized ");
            }
        }
        catch (SaveDataException e)
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

    protected override void LoadFile(string _sFilename)
    {
        OnScreenLog.Add($"LoadFile({m_oLoggedInUser.userId}): {_sFilename}");
        DirName dirName = new DirName();
        dirName.Data = _sFilename;

        PS5ReadFileRequest fileRequest = new PS5ReadFileRequest();
        fileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        PS5ReadFileResponse fileResponse = new PS5ReadFileResponse();

        GameCommon.Instance.StartCoroutine(PS5AutoSave.load(m_oLoggedInUser.userId, dirName, fileRequest, fileResponse, save_HandleAutoSaveError));
    }

    protected override bool UseThreadedSave() { return true; }

    protected override void Save(SaveRequest _osaveRequest)
    {
        OnScreenLog.Add("Saving: " + _osaveRequest.m_filename);

        m_sDirname = _osaveRequest.m_filename; // Keep for deletion if corrupted

        PS5AutoSave.save(m_oLoggedInUser.userId, _osaveRequest.m_filename, _osaveRequest.m_object, save_HandleAutoSaveError);
    }

    private void save_OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        OnScreenLog.Add("save_OnAsyncEvent (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.RequestId + ") : Calling User Id = (0x" + callbackEvent.UserId.ToString("X8") + ")");

        try
        {
            if (callbackEvent.Response != null)
            {
                if (callbackEvent.Response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                }
                else
                {
                    OnScreenLog.Add("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                }

                if (callbackEvent.Response.Exception != null)
                {
                    if (callbackEvent.Response.Exception is SaveDataException)
                    {
                        OnScreenLog.AddError("Response Exception: " + ((SaveDataException)callbackEvent.Response.Exception).ExtendedMessage);
                    }
                    else
                    {
                        OnScreenLog.AddError("Response Exception: " + callbackEvent.Response.Exception.Message);
                    }
                }
            }

            switch (callbackEvent.ApiCalled)
            {
                case FunctionTypes.FileOps:
                    {
                        if (callbackEvent.Request is PS5WriteFileRequest)
                        {
                            OnScreenLog.Add("Write Request");
                        }
                        else if (callbackEvent.Request is PS5ReadFileRequest)
                        {
                            OnScreenLog.Add("Load Response");
                            try
                            {
                                LoadDone(Utils.loadObjectFromBuffer<object>((callbackEvent.Response as PS5ReadFileResponse).m_aiLargeData));
                                OnScreenLog.Add("Load Done");
                            }
                            catch (Exception)
                            {
                                OnScreenLog.Add("Load null");
                                LoadDone(null);
                            }
                        }
                    }
                    break;
            }
        }
        catch (SaveDataException e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent SaveData Exception = " + e.ExtendedMessage);
            Console.Error.WriteLine(e.ExtendedMessage); // Output to the PS5 Stderr TTY
            Console.Error.WriteLine(e.StackTrace); // Output to the PS5 Stderr TTY
        }
        catch (Exception e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent General Exception = " + e.Message);
            OnScreenLog.AddError(e.StackTrace);
            Console.Error.WriteLine(e.StackTrace); // Output to the PS5 Stderr TTY
        }
    }

    void save_HandleAutoSaveError(uint errorCode)
    {
        OnScreenLog.Add("HandleAutoSaveError: " + errorCode);

        LoadDone(null);

        if (errorCode == (uint)ReturnCodes.DATA_ERROR_NO_SPACE_FS)
        {
            if (!m_bDisableNoSpaceMessage)
            {
                m_bShowNoSpace = true;
            }
            OnScreenLog.AddError("There is no space available for the auto-save");
        }
        else if (errorCode == (uint)ReturnCodes.SAVE_DATA_ERROR_BROKEN)
        {
            m_bShowCorrupt = true;
            delete();
            OnScreenLog.AddError("The auto-save file is corrupt");
            // At this point the title should inform the user their auto-save is corrupt and then decide how best to handle that situation.
            // If the save has a backup that could be restored or the title could choose the next oldest save data to load instead.
            // How this is handled will be up to the title.
        }
    }
    #endregion

    #region Delete
    private void delete()
    {

    }
    #endregion

    #region Activity
    public void startActivity()
    {
        UniversalDataSystem.UDSEvent oEvent = new UniversalDataSystem.UDSEvent();
        oEvent.Create("activityStart");
        oEvent.Properties.Set("activityId", "season");

        UniversalDataSystem.PostEventRequest oPostEvent = new UniversalDataSystem.PostEventRequest
        {
            UserId = m_oLoggedInUser.userId,
            EventData = oEvent
        };
        AsyncRequest<UniversalDataSystem.PostEventRequest> oAsyncOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(oPostEvent);
        UniversalDataSystem.Schedule(oAsyncOp);
    }

    public void endActivity()
    {
        UniversalDataSystem.UDSEvent oEvent = new UniversalDataSystem.UDSEvent();
        oEvent.Create("activityEnd");
        oEvent.Properties.Set("activityId", "season");

        UniversalDataSystem.PostEventRequest oPostEvent = new UniversalDataSystem.PostEventRequest
        {
            UserId = m_oLoggedInUser.userId,
            EventData = oEvent
        };
        AsyncRequest<UniversalDataSystem.PostEventRequest> oAsyncOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(oPostEvent);
        UniversalDataSystem.Schedule(oAsyncOp);
    }
    #endregion
}
#endif
