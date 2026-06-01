using Sirenix.OdinInspector;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public class SaveManager : SceneSingleton<SaveManager>
{
    #region Declarations
    public const string FOLDERNAME = "Blightstone";
    const string SAVEDATA_FILENAME = "SaveGame";

    [ShowInInspector, NonSerialized] SaveData m_oSaveData;
    
    bool m_bLoadDone = true;
    bool m_bSaveDone = true;

    public static SaveData SaveData => Instance.m_oSaveData;
    #endregion

    #region Load
    public void load()
    {
        m_bLoadDone = false;

        PlatformManager.Current.Load(SAVEDATA_FILENAME);
    }

    public bool isLoadDone() => m_bLoadDone;
    public bool isSavedDone() => m_bSaveDone;

    void Update()
    {
        updateLoad();
        updateSave();
    }

    public void createEmptySaveData()
    {
        m_oSaveData = new SaveData();
        m_oSaveData.initialize();
    }

    public void updateLoad()
    {
        if (!m_bLoadDone && PlatformManager.Current.IsLoadDone(SAVEDATA_FILENAME))
        {
            completeLoad();
        }
    }

    void completeLoad()
    {
        m_oSaveData = PlatformManager.Current.GetLoadedGame<SaveData>(SAVEDATA_FILENAME);

        if (m_oSaveData == null)
        {
            m_oSaveData = new SaveData();
            m_oSaveData.initialize();
        }
        else
        {
            m_oSaveData.load();
        }

        m_bLoadDone = true;

        EventManager.Instance.TriggerEvent(EventsType.LoadDone);
        Debug.Log("[DataManager] Load done");
    }
    #endregion

    #region Save
    public void updateSave()
    {
        if (!m_bSaveDone && !PlatformManager.Current.isAnySavingLeft())
        {
            Deb.log("[Save] Game saved succesfully.", eLogFlags.SYSTEM);
            m_bSaveDone = true;
        }
    }

    public void saveGame()
    {
        PlatformManager.Current.Save(m_oSaveData, SAVEDATA_FILENAME);
        m_bSaveDone = false;
    }
    #endregion

    #region Achievements
    public bool isAchieved(string _sAchievement)
    {
        return SaveData.m_asAchievementsCompleted.Contains(_sAchievement);
    }
    #endregion
}

#region Serialization - DON'T TOUCH
// === This is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
// Do not change this
public sealed class VersionDeserializationBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;

        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            assemblyName = Assembly.GetExecutingAssembly().FullName;

            // The following line of code returns the type. 
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
        }

        return typeToDeserialize;
    }
}
#endregion