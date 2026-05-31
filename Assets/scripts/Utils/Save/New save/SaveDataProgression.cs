using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SaveDataProgression : AutoSerializableData, ISave
{
    public string m_sVersion;

    #region Declarations

    [FoldoutGroup("Base")] public bool m_bCreated = false;

    [FoldoutGroup("Diff")] public int m_iDifficulty = 0;
    [FoldoutGroup("Diff")] public bool m_bDifficultyPopupDone;

    [FoldoutGroup("Unlocks")] public List<string> m_asUnlocks = new();

    [FoldoutGroup("Tutorial")] public bool m_bTutorialActive;
    [FoldoutGroup("Tutorial")] public bool m_bTutorialDone;

    [FoldoutGroup("Stats")] public List<string> m_asAchievementsCompleted = new();
    [FoldoutGroup("Stats")] public float m_fTotalTimePlayed;

    public bool m_bWishlistLinkClicked = false;

    public byte[] m_aoUserGUIDEncrypted;

    public bool m_bShowEndDemoPopUp = true;

    public string m_sUserGUID;
    public string m_sGUID;

    #endregion

    #region Basic
    public SaveDataProgression() { }
    public SaveDataProgression(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }

    public string Version { get => m_sVersion; set => m_sVersion = value; }

    public void initialize()
    {
        m_fTotalTimePlayed = 0;

        AchievementsManager.Instance.checkAchievements();

        m_sGUID = Guid.NewGuid().ToString();
    }

    public void load()
    {
        AchievementsManager.Instance.checkAchievements();
    }

    public void prepareSave()
    {
    }
    #endregion
}
