#if UNITY_STANDALONE || UNITY_EDITOR

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class PCManager : PlatformManagerSingleton<PCManager>
{
    private string m_sSavedGameFolder = "";

    public override void Initialize()
    {
        base.Initialize();

        checkOldSavedGameFolder();

        m_sSavedGameFolder = GetSavedGameFolder(true);
        m_bInitialized = true;
    }

    #region Save File Migration
    void checkOldSavedGameFolder()
    {
        string sOldSavedGameFolder = getOldSavedGameFolder();
        string sNewSavedGameFolder = GetSavedGameFolder(false);

        bool bOldFiles = checkSaveFiles(sOldSavedGameFolder);
        bool bNewFiles = checkSaveFiles(sNewSavedGameFolder);

        if (bOldFiles)
        {
            try
            {
                if (bNewFiles)
                {
                    Deb.log("[Migrate] Old and new save files!", eLogFlags.SYSTEM);

                    // Check file date
                    DirectoryInfo oDirectoryInfo_OLD = new DirectoryInfo(sOldSavedGameFolder);
                    DirectoryInfo oDirectoryInfo_NEW = new DirectoryInfo(sNewSavedGameFolder);

                    // Check newer OLD files and copy them
                    bool bIsNewSaveOlder = false;
                    foreach (FileInfo oFileInfo_OLD in oDirectoryInfo_OLD.GetFiles("*n.sav"))
                    {
                        foreach (FileInfo oFileInfoNEW in oDirectoryInfo_NEW.GetFiles("*n.sav"))
                        {
                            if (oFileInfoNEW.Name == oFileInfo_OLD.Name)
                            {
                                if (oFileInfoNEW.LastWriteTime.CompareTo(oFileInfo_OLD.LastWriteTime) < 0)
                                {
                                    Deb.log("[Migrate] New file are older: " + oFileInfoNEW.Name + " (NEW " + oFileInfoNEW.LastWriteTime + ") VS. (OLD " + oFileInfo_OLD.LastWriteTime + ")", eLogFlags.SYSTEM);
                                    File.Copy(oFileInfo_OLD.FullName, oFileInfoNEW.FullName, true);
                                    bIsNewSaveOlder = true;
                                }
                                File.Delete(oFileInfo_OLD.FullName);
                                break;
                            }
                        }
                    }

                    if (bIsNewSaveOlder)
                    {
                        copyOldFiles(oDirectoryInfo_OLD, sNewSavedGameFolder);
                    }

                    deleteOldFiles(oDirectoryInfo_OLD);

                    Deb.log("[Migrate] Migration completed. Used OLD: " + bIsNewSaveOlder, eLogFlags.SYSTEM);
                }
                else // Migrate old files to new folder
                {
                    Directory.CreateDirectory(sNewSavedGameFolder);

                    DirectoryInfo oDirectoryInfo = new DirectoryInfo(sOldSavedGameFolder);

                    copyOldFiles(oDirectoryInfo, sNewSavedGameFolder);
                    deleteOldFiles(oDirectoryInfo);

                    Deb.log("[Migrate] Success.", eLogFlags.SYSTEM);
                }
            }
            catch (IOException ex)
            {
                Deb.logError($"[Migrate] IO error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Deb.logError($"[Migrate] Unexpected error: {ex.Message}");
            }
        }
    }

    void copyOldFiles(DirectoryInfo _oDirectoryInfo, string _sFolderNew)
    {
        foreach (FileInfo oFileInfo in _oDirectoryInfo.GetFiles())
        {
            if (oFileInfo.Name.EndsWith(".sav") || oFileInfo.Name.EndsWith(".data"))
            {
                File.Copy(oFileInfo.FullName, _sFolderNew + oFileInfo.Name, true);
            }
        }
    }

    void deleteOldFiles(DirectoryInfo _oDirectoryInfo)
    {
        // Delete files (separated from copy just in case)
        foreach (FileInfo oFileInfo in _oDirectoryInfo.GetFiles())
        {
            if (oFileInfo.Name.EndsWith(".sav") || oFileInfo.Name.EndsWith(".data"))
            {
                File.Delete(oFileInfo.FullName);
            }
        }
    }

    bool checkSaveFiles(string _sFolder)
    {
        if (Directory.Exists(_sFolder))
        {
            DirectoryInfo oDirectoryInfo = new DirectoryInfo(_sFolder);
            foreach (FileInfo oFileInfo in oDirectoryInfo.GetFiles())
            {
                if (oFileInfo.Name.EndsWith(".sav") || oFileInfo.Name.EndsWith(".data"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    string getOldSavedGameFolder()
    {
        string sFolderMyGames = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/") + "/My Games/";
        string sSavedGameFolder = sFolderMyGames + SaveManager.FOLDERNAME + "/";

#if UNITY_STANDALONE && !DISABLESTEAMWORKS
        sSavedGameFolder += Steamworks.SteamUser.GetSteamID().ToString() + "/";
#elif UNITY_EDITOR
        sSavedGameFolder += "Editor" + "/";
#else
        sSavedGameFolder += "Build" + "/";
#endif

        return sSavedGameFolder;
    }
    #endregion

    #region paths
    public static string GetSavedGameFolder(bool _bCreateFolders)
    {
        string sSavedGameFolder = Application.persistentDataPath + "/";

#if UNITY_STANDALONE && !DISABLESTEAMWORKS
        sSavedGameFolder += Steamworks.SteamUser.GetSteamID().ToString() + "/";
#elif UNITY_EDITOR
        sSavedGameFolder += "Editor" + "/";
#else
        sSavedGameFolder += "Build" + "/";
#endif

        if (_bCreateFolders && !File.Exists(sSavedGameFolder))
        {
            Directory.CreateDirectory(sSavedGameFolder);
        }

        return sSavedGameFolder;
    }

    public string GetFullPath(string filename)
    {
        string sPath = m_sSavedGameFolder + filename;

        if (!sPath.Contains(".sav"))
        {
            sPath += ".sav";
        }

        return sPath;
    }
    #endregion

    #region Achievements/Trophies

    public override void UnlockChallenge(Achievement achievement)
    {
#if UNITY_STANDALONE && !DISABLESTEAMWORKS
        SteamManager.Instance.unlockAchievement(achievement);
#endif
    }

    #endregion

    #region Load/Save
    protected override void LoadFile(string _sFilename)
    {
        string fullPath = GetFullPath(_sFilename);
        object oASD = loadFile(fullPath);
        LoadDone(oASD);
    }

    object loadFile(string _sFullPath)
    {
        object oLoadedFile = null;

        if (File.Exists(_sFullPath))
        {
            var bformatter = new BinaryFormatter();
            bformatter.Binder = new VersionDeserializationBinder();
            bformatter.Context = new StreamingContext(StreamingContextStates.All);

            try
            {
                using (Stream stream = File.Open(_sFullPath, FileMode.Open))
                {
                    oLoadedFile = bformatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                Deb.logError("Error loading file: " + _sFullPath + e.ToString());
            }
        }
        return oLoadedFile;
    }

    protected override bool UseThreadedSave() => true;

    protected override void Save(SaveRequest saveRequest)
    {
        if (saveRequest.m_object == null)
        {
            Debug.LogException(new Exception("[SAVE] Trying to save empty object: :" + saveRequest.m_filename));
            return;
        }

        string sFileName = GetFullPath(saveRequest.m_filename);
        string sFileNameBackup = sFileName + "_BACKUP";
        if (File.Exists(sFileName)) File.Copy(sFileName, sFileNameBackup, true);

        Stream stream = null;
        BinaryFormatter bformatter = new BinaryFormatter();
        bool bRetry = false;
        bformatter.Context = new StreamingContext(StreamingContextStates.All);

        try
        {
            stream = File.Open(GetFullPath(saveRequest.m_filename), FileMode.Create);
            bformatter.Binder = new VersionDeserializationBinder();
            AutoSerializableData.syncAllBeforeSave(saveRequest.m_object);
            bformatter.Serialize(stream, saveRequest.m_object);
            stream.Close();
        }
        catch (Exception e)
        {
            if (stream != null) stream.Close();

            File.Copy(sFileNameBackup, sFileName, true);
            Debug.LogError("Error saving. Recovering BACKUP.");
            Debug.LogException(e);

            bRetry = true;
        }
        finally
        {
            File.Delete(sFileNameBackup);
        }

        if (bRetry)
        {
            try
            {
                stream = null;
                AutoSerializableData.setLogs(true);

                stream = File.Open(GetFullPath(saveRequest.m_filename + "_BUGGED"), FileMode.Create);
                bformatter.Binder = new VersionDeserializationBinder();
                bformatter.Context = new StreamingContext(StreamingContextStates.All);
                bformatter.Serialize(stream, saveRequest.m_object);
            }
            catch (Exception)
            {
            }
            finally
            {
                Debug.LogError(AutoSerializableData.getLogs());
                if (stream != null) stream.Close();
                File.Delete(GetFullPath(saveRequest.m_filename + "_BUGGED"));
                AutoSerializableData.setLogs(false);
            }
        }
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    [UnityEditor.MenuItem("UPTools/Delete Saved Game", false, 120)]
#endif
    public static void DeleteSavedGame()
    {
        string savedGameFolder = GetSavedGameFolder(false);

        if (!Directory.Exists(savedGameFolder)) return;

        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(savedGameFolder);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (file.Extension.EndsWith("sav"))
                {
                    file.Delete();
                }
            }

            Deb.log("[Save] Successfully cleared save game directory.", eLogFlags.SYSTEM);
        }
        catch (IOException ex)
        {
            Deb.logError($"[Save] An IO error occurred while deleting save data: {ex.Message}");
        }
        catch (Exception ex)
        {
            Deb.logError($"[Save] An unexpected error occurred: {ex.Message}");
        }
    }
    #endregion
}

#endif
