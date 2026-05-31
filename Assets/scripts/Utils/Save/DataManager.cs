//#define SNAPSHOTS

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public class DataManager : SceneSingleton<DataManager>
{
    #region Declarations
    public const int SLOT_COUNT = 3;

    public const string FOLDERNAME = "Blightstone";
    const string SAVEDATA_FILENAME = "SavedGame_";
    const string SAVEDATA_DEMO_FILENAME = "SavedGame_1_demo";

    const string SAVEDATA_OPTIONS_SUFFIX = "_options";
    const string SAVEDATA_PROGRESSION_SUFFIX = "_progression";

    [ShowInInspector, NonSerialized] SaveDataOptions m_oSaveDataOptions;
    [ShowInInspector, NonSerialized] List<SaveDataProgression> m_aoSaveDataProgression = new List<SaveDataProgression>(SLOT_COUNT) { null, null, null };

    bool m_bLoadDone = true;
    bool m_bSaveDone = true;

    public static SaveDataOptions SaveDataOptions => Instance.m_oSaveDataOptions;
    public static SaveDataProgression SaveDataProgression => Instance.m_aoSaveDataProgression[Instance.SlotIndex];
    public static SaveDataProgression SaveDataProgressionSlot(int _iSlot) => Instance.m_aoSaveDataProgression[_iSlot];

    public static float CurrentSlotTime() => SlotTime(Instance.SlotIndex);
    public static float SlotTime(int _iSlot) => SaveDataProgressionSlot(_iSlot).m_fTotalTimePlayed;

    [NonSerialized, ShowInInspector] public bool m_bSaveForbidden = true;

    private string m_sCurrentVersion = "1.0.0";
    public static string CurrentVersion
    {
        get => Instance.m_sCurrentVersion;
        set { Instance.m_sCurrentVersion = value; }
    }

    #endregion

    #region Load
    public void load()
    {
        m_bLoadDone = false;

        PlatformManager.Current.Load(getSaveDataGameOptions());
        for (int iIndex = 0; iIndex < SLOT_COUNT; ++iIndex)
        {
            PlatformManager.Current.Load(getSaveDataGameProgression(iIndex));
        }
    }

    string getSaveDataGame(int _iSlotIndex)
    {
#if VERSION_MAIN
        return SAVEDATA_FILENAME + (_iSlotIndex + 1);
#else
        return SAVEDATA_DEMO_FILENAME;
#endif
    }

    string getSaveDataGameOptions() => getSaveDataGame(0) + SAVEDATA_OPTIONS_SUFFIX;
    public string getSaveDataGameProgression(int _iSlotIndex) => getSaveDataGame(_iSlotIndex) + SAVEDATA_PROGRESSION_SUFFIX;

    public bool IsLoadDone() => m_bLoadDone;
    public bool IsSavedDone() => m_bSaveDone;

    void Update()
    {
        updateLoad();
        updateSave();
    }

    public void createEmptySaveData(bool _bDeleteOptionsAlso = true)
    {
        if (_bDeleteOptionsAlso)
        {
            m_oSaveDataOptions = new SaveDataOptions();
            m_oSaveDataOptions.initialize();
        }

        m_aoSaveDataProgression[SlotIndex] = new SaveDataProgression();
        m_aoSaveDataProgression[SlotIndex].initialize();
    }

    public void deleteCurrentSlot()
    {
        createEmptySaveData(false);
        saveGame(false, true);
    }

    public void updateLoad()
    {
#if UNITY_EDITOR
        if (m_bSaveForbidden) return;
#endif

        if (!m_bLoadDone)
        {
            if (PlatformManager.Current.IsLoadDone(getSaveDataGameOptions()))
            {
                for (int iIndex = 0; iIndex < SLOT_COUNT; ++iIndex)
                {
                    if (!PlatformManager.Current.IsLoadDone(getSaveDataGameProgression(iIndex)))
                    {
                        return;
                    }
                }

                completeLoad();
            }
        }
    }

    void completeLoad()
    {
        m_oSaveDataOptions = PlatformManager.Current.GetLoadedGame<SaveDataOptions>(getSaveDataGameOptions());

        if (m_oSaveDataOptions == null)
        {
            m_oSaveDataOptions = new SaveDataOptions();
            m_oSaveDataOptions.initialize();
        }
        else
        {
            m_oSaveDataOptions.load();
        }

        int iSlotBackup = SaveDataOptions.m_iCurrentSlot;

        for (int iSlotIndex = 0; iSlotIndex < SLOT_COUNT; ++iSlotIndex)
        {
            // set slot index while loading so it's used from everywhere
            selectSlot(iSlotIndex, false);

            m_aoSaveDataProgression[iSlotIndex] = PlatformManager.Current.GetLoadedGame<SaveDataProgression>(getSaveDataGameProgression(iSlotIndex));

            if (m_aoSaveDataProgression[iSlotIndex] == null)
            {
                m_aoSaveDataProgression[iSlotIndex] = new SaveDataProgression();
                m_aoSaveDataProgression[iSlotIndex].initialize();
            }
        }

        selectSlot(iSlotBackup);
        m_bLoadDone = true;

        EventManager.Instance.TriggerEvent(EventsType.LoadDone);
        Debug.Log("[DataManager] Load done");
    }

    public int SlotIndex { get { return SaveDataOptions?.m_iCurrentSlot ?? 0; } }

    public void selectSlot(int _iIndex, bool _bLoad = true)
    {
        SaveDataOptions.m_iCurrentSlot = _iIndex;
        if (_bLoad)
        {
            SaveDataProgression.m_bCreated = true;
            saveGame(true, true);
            SaveDataProgression.load();
        }

        EventManager.Instance.TriggerEvent(EventsType.ChangeSaveSlot);
    }

    #endregion

    #region Save
    public void updateSave()
    {
        if (!m_bSaveDone && !PlatformManager.Current.isAnySavingLeft())
        {
            Deb.log("[Save] Game saved succesfully.", eLogFlags.SYSTEM);

            m_bSaveDone = true;
            //ManagementUI.Instance.showSavingScreen(false);
        }
    }

    public void allowSave() => m_bSaveForbidden = false;

    public void saveGame(bool _bOptions, bool _bProgression)
    {
#if UNITY_EDITOR
        if (m_bSaveForbidden)
        {
            Deb.log("[Save] Cannot save the game. Save is set to forbidden.", eLogFlags.SYSTEM);
            return;
        }
#endif

        if (_bOptions) m_oSaveDataOptions.prepareSave();
        if (_bProgression) { SaveDataProgression.prepareSave(); }

        if (_bOptions) PlatformManager.Current.Save(m_oSaveDataOptions, getSaveDataGameOptions());
        if (_bProgression) PlatformManager.Current.Save(SaveDataProgression, getSaveDataGameProgression(SlotIndex));

        m_bSaveDone = false;
        //ManagementUI.Instance.showSavingScreen(true);
    }
    #endregion

    #region Achievements
    public bool isAchieved(string _sAchievement)
    {
        return SaveDataProgression.m_asAchievementsCompleted.Contains(_sAchievement);
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