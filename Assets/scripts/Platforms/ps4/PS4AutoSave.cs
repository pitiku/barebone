#if UNITY_PS4

using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class PS4AutoSave
{
    #region Definitions
    public delegate void ErrorHandler(uint errorCode);

    public enum SaveState
    {
        Begin = 0,
        SaveFiles = 1,
        WriteIcon = 2,
        WriteParams = 3,
        Unmount = 4,
        Unmounting = 5,
        HandleError = 10,

        LoadFiles = 11,

        IDLE = 20
    }
    #endregion

    #region Save
    public static SaveState ms_oCurrentState = SaveState.IDLE;
    static Sony.PS4.SaveData.Mounting.MountResponse ms_oMountResponse;
    static Sony.PS4.SaveData.Mounting.MountPoint ms_oMountPoint;
    static int ms_iErrorCode;

    static int ms_iUserId;
    static Sony.PS4.SaveData.Dialogs.NewItem ms_oNewItem;
    static Sony.PS4.SaveData.SaveDataParams ms_oSaveDataParams;
    static PS4WriteFileRequest ms_oFileRequest;
    static Sony.PS4.SaveData.FileOps.FileOperationResponse ms_oFileResponse;
    static ErrorHandler ms_dErrHandler;
    static Sony.PS4.SaveData.EmptyResponse ms_oIconResponse;
    static Sony.PS4.SaveData.EmptyResponse ms_oParamsResponse;
    static Sony.PS4.SaveData.EmptyResponse m_oUnmountResponse;

    public static void save(int _iUserId, Sony.PS4.SaveData.Dialogs.NewItem _oNewItem, Sony.PS4.SaveData.DirName _oAutoSaveDirName,
        UInt64 _iNewSaveDataBlocks, Sony.PS4.SaveData.SaveDataParams _oSaveDataParams,
        PS4WriteFileRequest _oFileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse _oFileResponse, ErrorHandler _dErrHandler)
    {
        ms_iUserId = _iUserId;
        ms_oNewItem = _oNewItem;
        ms_oSaveDataParams = _oSaveDataParams;
        ms_oFileRequest = _oFileRequest;
        ms_oFileResponse = _oFileResponse;
        ms_dErrHandler = _dErrHandler;

        ms_oMountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();
        ms_oMountPoint = null;

        // Begin
        Sony.PS4.SaveData.Mounting.MountModeFlags eFlags = Sony.PS4.SaveData.Mounting.MountModeFlags.Create2 | Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite;
        Sony.PS4.SaveData.DirName oDirName = _oAutoSaveDirName;
        ms_iErrorCode = mountSaveData(ms_iUserId, _iNewSaveDataBlocks, ms_oMountResponse, oDirName, eFlags);
        if (ms_iErrorCode < 0)
        {
            setCurrentState(SaveState.HandleError);
        }
        else
        {
            setCurrentState(SaveState.Begin);
        }

        while (ms_oCurrentState != SaveState.IDLE)
        {
            updateSave();
            Thread.Sleep(20);
        }
    }

    static void setCurrentState(SaveState _oState)
    {
        ms_oCurrentState = _oState;
        OnScreenLog.Add("updateSave: " + ms_oCurrentState);
    }

    public static void updateSave()
    {
        switch (ms_oCurrentState)
        {
            case SaveState.Begin:
                {
                    // Wait for save data to be mounted.
                    if (!ms_oMountResponse.Locked)
                    {
                        if (ms_oMountResponse.IsErrorCode == true)
                        {
                            ms_iErrorCode = ms_oMountResponse.ReturnCodeValue;

                            // Must handle DATA_ERROR_NO_SPACE_FS & SAVE_DATA_ERROR_BROKEN
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            // Save data is now mounted, so files can be saved.
                            ms_oMountPoint = ms_oMountResponse.MountPoint;

                            // SaveFiles -> Do actual saving
                            ms_oFileRequest.MountPointName = ms_oMountPoint.PathName;
                            ms_oFileRequest.Async = true;
                            ms_oFileRequest.UserId = ms_iUserId;

                            ms_iErrorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(ms_oFileRequest, ms_oFileResponse);

                            if (ms_iErrorCode < 0)
                            {
                                setCurrentState(SaveState.HandleError);
                            }
                            else
                            {
                                setCurrentState(SaveState.SaveFiles);
                            }
                        }
                    }
                }
                break;

            case SaveState.SaveFiles:
                {
                    if (!ms_oFileResponse.Locked)
                    {
                        // Write the icon and any detail params set here.
                        ms_oIconResponse = new Sony.PS4.SaveData.EmptyResponse();
                        ms_iErrorCode = writeIcon(ms_iUserId, ms_oIconResponse, ms_oMountPoint, ms_oNewItem);

                        if (ms_iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            ms_oParamsResponse = new Sony.PS4.SaveData.EmptyResponse();
                            ms_iErrorCode = writeParams(ms_iUserId, ms_oParamsResponse, ms_oMountPoint, ms_oSaveDataParams);

                            setCurrentState(SaveState.WriteIcon);
                        }
                    }
                }
                break;

            case SaveState.WriteIcon:
                {
                    if (!ms_oIconResponse.Locked && !ms_oParamsResponse.Locked)
                    {
                        ms_oIconResponse = new Sony.PS4.SaveData.EmptyResponse();
                        ms_iErrorCode = writeIcon(ms_iUserId, ms_oIconResponse, ms_oMountPoint, ms_oNewItem);

                        if (ms_iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            setCurrentState(SaveState.WriteParams);
                        }
                    }
                }
                break;

            case SaveState.WriteParams:
                {
                    if (!ms_oIconResponse.Locked)
                    {
                        ms_oParamsResponse = new Sony.PS4.SaveData.EmptyResponse();
                        ms_iErrorCode = writeParams(ms_iUserId, ms_oParamsResponse, ms_oMountPoint, ms_oSaveDataParams);

                        if (ms_iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            setCurrentState(SaveState.Unmount);
                        }
                    }
                }
                break;

            case SaveState.Unmount:
                if (!ms_oParamsResponse.Locked)
                {
                    unMount();
                }
                break;

            case SaveState.Unmounting:
                if (!m_oUnmountResponse.Locked)
                {
                    setCurrentState(SaveState.IDLE);
                }
                break;

            case SaveState.HandleError:
                handleError();
                break;
        }
    }

    static void unMount()
    {
        m_oUnmountResponse = new Sony.PS4.SaveData.EmptyResponse();
        ms_iErrorCode = unmountSaveData(ms_iUserId, m_oUnmountResponse, ms_oMountPoint, true);

        if (ms_iErrorCode < 0)
        {
            setCurrentState(SaveState.HandleError);
        }
        else
        {
            setCurrentState(SaveState.Unmounting);
        }
    }

    static void handleError()
    {
        if (ms_oMountPoint != null)
        {
            Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();
            unmountSaveData(ms_iUserId, unmountResponse, ms_oMountPoint, true);
        }

        if (ms_dErrHandler != null)
        {
            ms_dErrHandler((uint)ms_iErrorCode);
        }

        setCurrentState(SaveState.IDLE);
    }
    #endregion

    #region Load
    public static IEnumerator load(int userId, Sony.PS4.SaveData.DirName dirName, Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse, ErrorHandler errHandler)
    {
        SaveState currentState = SaveState.Begin;

        Sony.PS4.SaveData.Mounting.MountResponse mountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();
        Sony.PS4.SaveData.Mounting.MountPoint mp = null;

        int errorCode = 0;

        while (currentState != SaveState.IDLE)
        {
            Debug.Log("LoadProcess: " + currentState);
            switch (currentState)
            {
                case SaveState.Begin:
                    {
                        Sony.PS4.SaveData.Mounting.MountModeFlags flags = Sony.PS4.SaveData.Mounting.MountModeFlags.ReadOnly;

                        errorCode = mountSaveData(userId, 0, mountResponse, dirName, flags);

                        if (errorCode < 0)
                        {
                            Debug.Log("error1: " + errorCode);
                            currentState = SaveState.HandleError;
                        }
                        else
                        {
                            // Wait for save data to be mounted.
                            while (mountResponse.Locked == true)
                            {
                                yield return null;
                            }

                            if (mountResponse.IsErrorCode == true)
                            {
                                errorCode = mountResponse.ReturnCodeValue;
                                Debug.Log("error2: " + errorCode);

                                // Must handle broken save games
                                //    Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Save data is now mounted, so files can be saved.
                                mp = mountResponse.MountPoint;
                                currentState = SaveState.LoadFiles;
                            }
                        }
                    }
                    break;
                case SaveState.LoadFiles:
                    {
                        // Do actual loading
                        fileRequest.MountPointName = mp.PathName;
                        fileRequest.Async = true;
                        fileRequest.UserId = userId;

                        errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(fileRequest, fileResponse);

                        if (errorCode < 0)
                        {
                            currentState = SaveState.HandleError;
                        }
                        else
                        {
                            while (fileResponse.Locked == true)
                            {
                                yield return null;
                            }

                            currentState = SaveState.Unmount;
                        }
                    }
                    break;
                case SaveState.Unmount:
                    {
                        Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                        errorCode = unmountSaveData(userId, unmountResponse, mp, false);

                        if (errorCode < 0)
                        {
                            currentState = SaveState.HandleError;
                        }
                        else
                        {
                            while (unmountResponse.Locked == true)
                            {
                                yield return null;
                            }

                            currentState = SaveState.IDLE;
                        }
                    }
                    break;
                case SaveState.HandleError:
                    {
                        if (mp != null)
                        {
                            Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                            unmountSaveData(userId, unmountResponse, mp, false);
                        }

                        if (errHandler != null)
                        {
                            errHandler((uint)errorCode);
                        }
                    }
                    currentState = SaveState.IDLE;
                    break;
            }

            yield return null;
        }
    }
    #endregion

    #region Platform functions
    internal static int mountSaveData(int userId, UInt64 blocks, Sony.PS4.SaveData.Mounting.MountResponse mountResponse, Sony.PS4.SaveData.DirName dirName, Sony.PS4.SaveData.Mounting.MountModeFlags flags)
    {
        int errorCode = unchecked((int)0x80B8000E);
        try
        {
            Sony.PS4.SaveData.Mounting.MountRequest request = new Sony.PS4.SaveData.Mounting.MountRequest();

            request.UserId = userId;
            request.IgnoreCallback = true;
            request.DirName = dirName;

            request.MountMode = flags;

            if (blocks < Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN)
            {
                blocks = Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN;
            }

            request.Blocks = blocks;

            Sony.PS4.SaveData.Mounting.Mount(request, mountResponse);
            errorCode = 0;
        }
        catch
        {
            if (mountResponse.ReturnCodeValue < 0)
            {
                errorCode = mountResponse.ReturnCodeValue;
            }
        }

        return errorCode;
    }

    internal static int unmountSaveData(int userId, Sony.PS4.SaveData.EmptyResponse unmountResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, bool backup)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Sony.PS4.SaveData.Mounting.UnmountRequest request = new Sony.PS4.SaveData.Mounting.UnmountRequest();

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.Backup = backup;
            request.IgnoreCallback = true;

            Sony.PS4.SaveData.Mounting.Unmount(request, unmountResponse);

            errorCode = 0;
        }
        catch
        {
            if (unmountResponse.ReturnCodeValue < 0)
            {
                errorCode = unmountResponse.ReturnCodeValue;
            }
        }

        return errorCode;
    }

    internal static int writeIcon(int userId, Sony.PS4.SaveData.EmptyResponse iconResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.Dialogs.NewItem newItem)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Sony.PS4.SaveData.Mounting.SaveIconRequest request = new Sony.PS4.SaveData.Mounting.SaveIconRequest();

            if (mp == null) return errorCode;

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.RawPNG = newItem.RawPNG;
            request.IconPath = newItem.IconPath;
            request.IgnoreCallback = true;

            Sony.PS4.SaveData.Mounting.SaveIcon(request, iconResponse);

            errorCode = 0;
        }
        catch
        {
            if (iconResponse.ReturnCodeValue < 0)
            {
                errorCode = iconResponse.ReturnCodeValue;
            }
        }

        return errorCode;
    }

    internal static int writeParams(int userId, Sony.PS4.SaveData.EmptyResponse paramsResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.SaveDataParams saveDataParams)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Sony.PS4.SaveData.Mounting.SetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.SetMountParamsRequest();

            if (mp == null) return errorCode;

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.IgnoreCallback = true;

            request.Params = saveDataParams;

            Sony.PS4.SaveData.Mounting.SetMountParams(request, paramsResponse);

            errorCode = 0;
        }
        catch
        {
            if (paramsResponse.ReturnCodeValue < 0)
            {
                errorCode = paramsResponse.ReturnCodeValue;
            }
        }

        return errorCode;
    }
    #endregion
}
#endif