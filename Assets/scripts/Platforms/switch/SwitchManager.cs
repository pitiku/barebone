#if UNITY_SWITCH
using Rewired.Platforms.Switch;
using UnityEngine;

public class SwitchManager : PlatformManagerSingleton<SwitchManager>
{
    private nn.account.Uid m_iUserId;
    [HideInInspector] public string m_sUserNick;
    private nn.fs.FileHandle m_oFileHandle = new nn.fs.FileHandle();

    private bool m_bSaving = false;
    private bool m_bFastLoad = false;

    #region Main
    public override void Initialize()
    {
        base.Initialize();
        m_bInitialized = true;

#if !UNITY_EDITOR
            nn.account.Account.Initialize();
            nn.account.UserHandle userHandle = new nn.account.UserHandle();
    
    		if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
    		{
    			return;
    		}
            
            nn.account.Account.GetUserId(ref m_iUserId, userHandle);
    
            nn.account.Nickname oNickname = new nn.account.Nickname();
            nn.account.Account.GetNickname(ref oNickname, m_iUserId);
            m_sUserNick = oNickname.name;
    
            nn.Result result = nn.fs.SaveData.Mount(DataManager.FOLDERNAME, m_iUserId);
    
            result.abortUnlessSuccess();
    
            m_bInitialized = true;
#endif
    }
    #endregion

    #region SAVE
    private string getFilePath(string _sFileName)
    {
        return string.Format("{0}:/{1}", DataManager.FOLDERNAME, _sFileName);
    }

    protected override bool UseThreadedSave() { return true; }

    private string getMountPath(string _sFolder)
    {
        return string.Format("{0}:/", DataManager.FOLDERNAME);
    }

    protected override void preSave()
    {
        base.preSave();

        // Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
    }

    protected override void postSave()
    {
        base.postSave();

        // Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
    }

    public override bool IsSaving() => m_bSaving;

    protected override void Save(SaveRequest saveRequest)
    {
        m_bSaving = true;

        byte[] data = Utils.getMemoryStreamOf(saveRequest.m_object).GetBuffer();

        int iDataSize = data.Length;

        string sFilePath = getFilePath(saveRequest.m_filename);
        nn.Result result = nn.fs.File.Delete(sFilePath);
        if (!nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            result.abortUnlessSuccess();
        }

        result = nn.fs.File.Create(sFilePath, iDataSize);
        result.abortUnlessSuccess();

        result = nn.fs.File.Open(ref m_oFileHandle, sFilePath, nn.fs.OpenFileMode.Write);
        result.abortUnlessSuccess();

        result = nn.fs.File.Write(m_oFileHandle, 0, data, data.LongLength, nn.fs.WriteOption.Flush);
        result.abortUnlessSuccess();

        nn.fs.File.Close(m_oFileHandle);
        result = nn.fs.FileSystem.Commit(DataManager.FOLDERNAME);
        result.abortUnlessSuccess();

        m_bSaving = false;
    }

    protected override void LoadFile(string filename)
    {
        Debug.Log("load " + filename);

        string sFilePath = getFilePath(filename);
        nn.fs.EntryType entryType = 0;

        nn.Result result = nn.fs.FileSystem.GetEntryType(ref entryType, sFilePath);
        if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
        {
            LoadDone(null);
            return;
        }

        result.abortUnlessSuccess();

        result = nn.fs.File.Open(ref m_oFileHandle, sFilePath, nn.fs.OpenFileMode.Read);
        result.abortUnlessSuccess();

        long fileSize = 0;
        result = nn.fs.File.GetSize(ref fileSize, m_oFileHandle);
        result.abortUnlessSuccess();

        byte[] data = new byte[fileSize];
        result = nn.fs.File.Read(m_oFileHandle, 0, data, fileSize);
        result.abortUnlessSuccess();

        LoadDone(Utils.loadObjectFromBuffer<object>(data));

        nn.fs.File.Close(m_oFileHandle);
    }
    #endregion

    #region Misc
    public override float getVibrationMultiplier() => 0.1f;

    public void showControllerApplet()
    {
        // Set the options to pass to the Controller Applet
        ControllerAppletOptions options = new ControllerAppletOptions();
        options.playerCountMax = 4;
        options.showColors = true;
        options.showLabels = true;
        options.players[0].color = Color.red;
        options.players[0].label = "Red Player";
        options.players[1].color = Color.green;
        options.players[1].label = "Green Player";
        options.players[2].color = Color.blue;
        options.players[2].label = "Blue Player";
        options.players[3].color = Color.yellow;
        options.players[3].label = "Yellow Player";

        // Show the controller applet
        UnityEngine.Switch.Applet.Begin(); // See Unity documentation for explanation of this function
        SwitchInput.ControllerApplet.Show(options);
        UnityEngine.Switch.Applet.End();
    }

    public void setCurrentFastLoad()
    {
#if !UNITY_EDITOR
            UnityEngine.Switch.Performance.SetCpuBoostMode(m_bFastLoad ? UnityEngine.Switch.Performance.CpuBoostMode.FastLoad : UnityEngine.Switch.Performance.CpuBoostMode.Normal);
#endif
    }

    public void setFastLoad(bool _bValue)
    {
        m_bFastLoad = _bValue;
        setCurrentFastLoad();
    }

    //public override bool isUseLoadingScreen()
    //{
    //    return true;
    //}
    #endregion
}
#endif