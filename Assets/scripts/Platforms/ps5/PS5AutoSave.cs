#if UNITY_PS5

#region using
using System;
using System.Collections;
using System.Threading;
using Unity.SaveData.PS5.Core;
using Unity.SaveData.PS5.Dialog;
using Unity.SaveData.PS5.Info;
using Unity.SaveData.PS5.Mount;
#endregion

public class PS5AutoSave
{
    static public UInt64 TestBlockSize = Mounting.MountRequest.BLOCKS_MIN + ((1024 * 1024 * 45) / Mounting.MountRequest.BLOCK_SIZE);

    #region definitions
    public delegate void ErrorHandler(uint _iErrorCode);

    public enum SaveState
    {
        Begin,
        SaveFiles,
        WriteIcon,
        WriteParams,
        Unmount,
        HandleError,

        LoadFiles,

        IDLE
    }
    #endregion

    #region Save
    public static SaveState ms_eCurrentState = SaveState.Begin;

    public static void save(int userId, string _sFilename, object _oObject, ErrorHandler errHandler)
    {
        setCurrentState(SaveState.Begin);

        // Create the new item for the saves dialog list
        Dialogs.NewItem oNewItem = new Dialogs.NewItem();
        oNewItem.IconPath = "/app0/Media/StreamingAssets/PS5SaveIcon.png";
        oNewItem.Title = _sFilename;

        // The directory name for a new savedata
        DirName oNewDirName = new DirName();
        oNewDirName.Data = _sFilename;

        // Parameters to use for the savedata
        SaveDataParams oSaveDataParams = new SaveDataParams();
        oSaveDataParams.Title = oNewItem.Title;
        oSaveDataParams.SubTitle = "";
        oSaveDataParams.Detail = "";
        oSaveDataParams.UserParam = (uint)0;

        // Actual custom file operation to perform on the savedata, once it is mounted.
        PS5WriteFileRequest oFileRequest = new PS5WriteFileRequest();
        oFileRequest.IgnoreCallback = false; // In this example get a async callback once the file operations are complete
        oFileRequest.m_aiLargeData = Utils.getMemoryStreamOf(_oObject).GetBuffer();

        PS5WriteFileResponse oFileResponse = new PS5WriteFileResponse();

        Mounting.MountResponse oMountResponse = new Mounting.MountResponse();
        Mounting.MountPoint oMountPoint = null;

        int iErrorCode = 0;

        while (ms_eCurrentState != SaveState.IDLE)
        {
            switch (ms_eCurrentState)
            {
                case SaveState.Begin:
                    {
                        Mounting.MountModeFlags flags = Mounting.MountModeFlags.Create2 | Mounting.MountModeFlags.ReadWrite;

                        iErrorCode = MountSaveData(userId, TestBlockSize, oMountResponse, oNewDirName, flags);

                        if (iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            // Wait for save data to be mounted.
                            while (oMountResponse.Locked)
                            {
                                //_oWaitHandle.WaitOne();
                                Thread.Sleep(20);
                            }

                            if (oMountResponse.IsErrorCode)
                            {
                                iErrorCode = oMountResponse.ReturnCodeValue;

                                // Must handle no space and broken save games
                                //    ReturnCodes.DATA_ERROR_NO_SPACE_FS
                                //    ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                setCurrentState(SaveState.HandleError);
                            }
                            else
                            {
                                // Save data is now mounted, so files can be saved.
                                oMountPoint = oMountResponse.MountPoint;
                                setCurrentState(SaveState.SaveFiles);
                            }
                        }
                    }
                    break;
                case SaveState.SaveFiles:
                    {
                        // Do actual saving
                        oFileRequest.MountPointName = oMountPoint.PathName;
                        oFileRequest.Async = true;
                        oFileRequest.UserId = userId;

                        iErrorCode = FileOps.CustomFileOp(oFileRequest, oFileResponse);

                        if (iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            while (oFileResponse.Locked == true)
                            {
                                Thread.Sleep(20);
                                //_oWaitHandle.WaitOne();
                            }

                            // Write the icon and any detail parmas set here.
                            EmptyResponse iconResponse = new EmptyResponse();

                            iErrorCode = WriteIcon(userId, iconResponse, oMountPoint, oNewItem);

                            if (iErrorCode < 0)
                            {
                                setCurrentState(SaveState.HandleError);
                            }
                            else
                            {
                                EmptyResponse paramsResponse = new EmptyResponse();

                                iErrorCode = WriteParams(userId, paramsResponse, oMountPoint, oSaveDataParams);

                                if (iErrorCode < 0)
                                {
                                    setCurrentState(SaveState.HandleError);
                                }
                                else
                                {
                                    // Wait for save icon to be mounted.
                                    while (iconResponse.Locked == true || paramsResponse.Locked == true)
                                    {
                                        Thread.Sleep(20);
                                    }

                                    setCurrentState(SaveState.WriteIcon);
                                }
                            }
                        }
                    }
                    break;
                case SaveState.WriteIcon:
                    {
                        // Write the icon and any detail parmas set here.
                        EmptyResponse iconResponse = new EmptyResponse();

                        iErrorCode = WriteIcon(userId, iconResponse, oMountPoint, oNewItem);

                        if (iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            while (iconResponse.Locked == true)
                            {
                                Thread.Sleep(20);
                                //_oWaitHandle.WaitOne();
                            }

                            setCurrentState(SaveState.WriteParams);
                        }
                    }
                    break;
                case SaveState.WriteParams:
                    {
                        EmptyResponse paramsResponse = new EmptyResponse();

                        iErrorCode = WriteParams(userId, paramsResponse, oMountPoint, oSaveDataParams);

                        if (iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            // Wait for save icon to be mounted.
                            while (paramsResponse.Locked == true)
                            {
                                Thread.Sleep(20);
                                //_oWaitHandle.WaitOne();
                            }
                            setCurrentState(SaveState.Unmount);
                        }
                    }
                    break;
                case SaveState.Unmount:
                    {
                        EmptyResponse unmountResponse = new EmptyResponse();

                        iErrorCode = UnmountSaveData(userId, unmountResponse, oMountPoint);

                        if (iErrorCode < 0)
                        {
                            setCurrentState(SaveState.HandleError);
                        }
                        else
                        {
                            while (unmountResponse.Locked == true)
                            {
                                Thread.Sleep(20);
                                //_oWaitHandle.WaitOne();
                            }

                            setCurrentState(SaveState.IDLE);
                        }
                    }
                    break;
                case SaveState.HandleError:
                    {
                        if (oMountPoint != null)
                        {
                            EmptyResponse unmountResponse = new EmptyResponse();

                            UnmountSaveData(userId, unmountResponse, oMountPoint);
                        }

                        if (errHandler != null)
                        {
                            errHandler((uint)iErrorCode);
                        }
                    }
                    setCurrentState(SaveState.IDLE);
                    break;
            }
        }
    }

    static void setCurrentState(SaveState _eState)
    {
        ms_eCurrentState = _eState;
    }
    #endregion

    #region Load
    public static IEnumerator load(int userId, DirName dirName, FileOps.FileOperationRequest fileRequest, FileOps.FileOperationResponse fileResponse, ErrorHandler errHandler)
    {
        SaveState currentState = SaveState.Begin;

        Mounting.MountResponse mountResponse = new Mounting.MountResponse();
        Mounting.MountPoint mp = null;

        int errorCode = 0;

        while (currentState != SaveState.IDLE)
        {
            switch (currentState)
            {
                case SaveState.Begin:
                    {
                        Mounting.MountModeFlags flags = Mounting.MountModeFlags.ReadOnly;

                        errorCode = MountSaveData(userId, 0, mountResponse, dirName, flags);

                        if (errorCode < 0)
                        {
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
                                // Must handle broken save games
                                //    ReturnCodes.SAVE_DATA_ERROR_BROKEN)
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

                        errorCode = FileOps.CustomFileOp(fileRequest, fileResponse);

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
                        EmptyResponse unmountResponse = new EmptyResponse();

                        errorCode = UnmountSaveData(userId, unmountResponse, mp);

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
                            EmptyResponse unmountResponse = new EmptyResponse();

                            UnmountSaveData(userId, unmountResponse, mp);
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
    internal static int MountSaveData(int userId, UInt64 blocks, Mounting.MountResponse mountResponse, DirName dirName, Mounting.MountModeFlags flags)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Mounting.MountRequest request = new Mounting.MountRequest();

            request.UserId = userId;
            request.IgnoreCallback = true;
            request.DirName = dirName;

            request.MountMode = flags;

            if (blocks < Mounting.MountRequest.BLOCKS_MIN)
            {
                blocks = Mounting.MountRequest.BLOCKS_MIN;
            }

            request.Blocks = blocks;

            //              request.SystemBlocks = 0;     // setting to zero specifies savedata that does no support rollback. https://game.develop.playstation.net/resources/documents/sdk/latest/SaveData-Reference/0011.html

            Mounting.Mount(request, mountResponse);
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

    internal static int UnmountSaveData(int userId, EmptyResponse unmountResponse, Mounting.MountPoint mp)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Mounting.UnmountRequest request = new Mounting.UnmountRequest();

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.IgnoreCallback = true;

            Mounting.Unmount(request, unmountResponse);

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

    internal static int WriteIcon(int userId, EmptyResponse iconResponse, Mounting.MountPoint mp, Dialogs.NewItem newItem)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Mounting.SaveIconRequest request = new Mounting.SaveIconRequest();

            if (mp == null) return errorCode;

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.RawPNG = newItem.RawPNG;
            request.IconPath = newItem.IconPath;
            request.IgnoreCallback = true;

            Mounting.SaveIcon(request, iconResponse);

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

    internal static int WriteParams(int userId, EmptyResponse paramsResponse, Mounting.MountPoint mp, SaveDataParams saveDataParams)
    {
        int errorCode = unchecked((int)0x80B8000E);

        try
        {
            Mounting.SetMountParamsRequest request = new Mounting.SetMountParamsRequest();

            if (mp == null) return errorCode;

            request.UserId = userId;
            request.MountPointName = mp.PathName;
            request.IgnoreCallback = true;

            request.Params = saveDataParams;

            Mounting.SetMountParams(request, paramsResponse);

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