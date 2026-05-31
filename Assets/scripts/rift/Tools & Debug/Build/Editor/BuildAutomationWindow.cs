using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class DefineSymbols
{
    public const string MARKETING_TOOL = "MARKETING_TOOL";
    public const string DISABLE_ANALYTICS2 = "DISABLE_ANALYTICS2";
    public const string SNAPSHOTS = "SNAPSHOTS";
    public const string EXHIBITION = "VERSION_EXHIBITION";
    public const string DISABLE_STEAM = "DISABLESTEAMWORKS";
    public const string TUTORIAL = "TUTORIAL";
    public const string DRUID_UNLOCKED = "DRUID_UNLOCKED";
    public const string ROGUE_UNLOCKED = "ROGUE_UNLOCKED";
    public const string BARBARIAN_UNLOCKED = "BARBARIAN_UNLOCKED";
    public const string CONTROLLER = "CONTROLLER";
    public const string COMPENDIUM = "COMPENDIUM";
    public const string STEAM_ANONIMITY = "STEAM_ANONIMITY";

    public static readonly string[] ALL_MANAGED_DEFINES = new string[]
    {
        MARKETING_TOOL, EXHIBITION, DISABLE_STEAM, TUTORIAL, CONTROLLER, DRUID_UNLOCKED, ROGUE_UNLOCKED, BARBARIAN_UNLOCKED, COMPENDIUM, STEAM_ANONIMITY, DISABLE_ANALYTICS2, SNAPSHOTS
    };
}

[InitializeOnLoad]
public class BuildProfileInitializer
{
    static BuildProfileInitializer()
    {
        if (BuildProfile.GetActiveBuildProfile() == null)
        {
            InitializeBuildProfile();
        }
    }

    static void InitializeBuildProfile()
    {
        string[] profileGuids = AssetDatabase.FindAssets("t:BuildProfile");
        if (profileGuids.Length == 0)
        {
            Debug.LogWarning("No Build Profiles found anywhere in the project. Please create one.");
            return;
        }

        var allProfiles = profileGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<BuildProfile>(path))
            .Where(profile => profile != null)
            .ToList();

        if (!allProfiles.Any())
        {
            Debug.LogWarning("Failed to load any Build Profile assets. Please check for corruption.");
            return;
        }

        Utils.setActiveBuildProfile(allProfiles.First());
    }
}

public class BuildAutomationWindow : OdinEditorWindow
{
    // A state machine to manage the multi-step build process without async/await.
    private enum BuildState
    {
        Idle,
        WaitingForProfileRecompile,
        WaitingForDefinesRecompile,
        FetchingGitVersion,
        ReadyToBuild,
        Building,
        FetchingGitVersion_ForPrefabOnly
    }

    #region Configuration Fields

    [Title("Build Configuration", Bold = true, HorizontalLine = true)]
    [BoxGroup("Profile Selection"), Required]
    [LabelText("Build Profile")]
    [AssetList(Path = "/Settings/Build Profiles", AutoPopulate = true)]
    [OnValueChanged("LoadDefineSymbolsState")]
    [Tooltip("Select the build profile containing platform-specific settings")]
    public BuildProfile m_oSelectedProfile;
    void setSelectedProfile(BuildProfile _oProfile)
    {
        //Debug.Log("setSelectedProfile: " + _oProfile);
        m_oSelectedProfile = _oProfile;
    }

    [BoxGroup("Scenes Overwrite")]
    [LabelText("Custom Scenes")]
    public bool customScenes = false;

    [BoxGroup("Scenes Overwrite")]
    [ShowIf("@customScenes")]
    [LabelText("Dont Include MainMenu")]
    public bool dontIncludeMainMenu = false;

    [BoxGroup("Scenes Overwrite")]
    [ShowIf("@customScenes")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [Tooltip("Sets the current scene as unique in build settings")]
    private void SetCurrentSceneIntoBuildScenes()
    {
        m_aoInitialEditorBuildSettingsScenes = EditorBuildSettings.scenes.ToList();

        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        if (!dontIncludeMainMenu)
        {
            foreach (var oBuildedScenes in EditorBuildSettings.scenes)
            {
                if (oBuildedScenes.path.Contains("MAP") || oBuildedScenes.path.Contains("intro_scene") || oBuildedScenes.path.Contains("MainMenu") || oBuildedScenes.path.Contains("loading"))
                {
                    editorBuildSettingsScenes.Add(oBuildedScenes);
                }
            }
        }
        List<SceneAsset> aoSceneAssets = Utils.findAssetsByType<SceneAsset>(new string[] { "Assets/Scenes/nodes/mountain", "Assets/Scenes/nodes/forest", "Assets/Scenes/nodes/moor", "Assets/Scenes/nodes/desert", "Assets/Scenes/nodes/blighted" }).Where(oTarget => oTarget.name.Contains("scn")).ToList();

        foreach (var sceneAsset in aoSceneAssets)
        {
            if (EditorSceneManager.GetActiveScene().name == sceneAsset.name)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true));
                break;
            }
        }

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    [BoxGroup("Naming Scheme")]
    [LabelText("Build Prefix")]
    [ValidateInput("ValidateString", "Prefix cannot be empty")]
    [Tooltip("Prefix for build name (e.g., project abbreviation)")]
    private string BuildPrefix
    {
        get
        {
            if (milestoneNumber.isNullOrEmpty() || milestoneNumber.Length < 3)
            {
                return "error";
            }
            string sVersionNumeric = $"V{milestoneNumber[0]}.{milestoneNumber[1]}.{milestoneNumber[2]}";
            return sVersionNumeric;
        }
    }

    private const string SharedSettingsAssetPath = "Assets/Settings/BuildAutomationSharedSettings.asset";
    private const string ApplyDefinesOnStartupSessionKey = "BuildAutomation.ApplyDefinesStartupDone";
    private BuildAutomationSharedSettings m_oSharedSettings;

    [BoxGroup("Naming Scheme")]
    [LabelText("Major")]
    [ValidateInput("ValidateString", "Major.Minor cannot be empty")]
    [Tooltip("Major version number")]
    public string buildMajor = "0";

    [BoxGroup("Naming Scheme")]
    [LabelText("Minor")]
    [ValidateInput("ValidateString", "Major.Minor cannot be empty")]
    [Tooltip("Minor version number in major.minor")]
    public string buildMinor = "1";

    // NEW: milestone (XX) for the new scheme 0.7.XX.commit
    [BoxGroup("Naming Scheme")]
    [LabelText("Milestone")]
    [Tooltip("Milestone number for new scheme: major.minor.XX.commit")]
    public string milestoneNumber = "010";

    [BoxGroup("Naming Scheme")]
    [LabelText("Internal")]
    [ValidateInput("ValidateString", "Internal build cannot be empty")]
    [Tooltip("Internal build version number in (....)")]
    public string buildMinorInternal = "8";

    [BoxGroup("Naming Scheme")]
    [LabelText("Use Custom Version")]
    [Tooltip("If true, uses the custom commit below instead of the Git commit count.")]
    public bool useCustomVersionString = false;

    // Optional: keep a human label but make sure it doesn't break parsing (it will be placed in parentheses)
    [BoxGroup("Naming Scheme")]
    [LabelText("Custom Tag (Optional)")]
    [ShowIf("useCustomVersionString")]
    [Tooltip("Optional tag to append in parentheses (e.g., 'rc1'). Does not affect version parsing.")]
    public string customVersionSuffix = "custom";

    [BoxGroup("Naming Scheme", CenterLabel = true)]
    [InfoBox("Build Name Format: [Prefix]major.minor.milestone.commit(optionalTag)", InfoMessageType.Info)]
    [ShowInInspector, ReadOnly]
    [Tooltip("Automatically generated build name preview (PARSEABLE by GameVersion.tryParse)")]
    private string PreviewBuildName
    {
        get
        {
            string sPrefix = getBuildPrefix();

            // commit number:
            string sCommit = m_sGitCommitCount;

            string sSuffix = "[";
            // If we have a git hash, add it in parentheses (ignored by parser)
            if (!string.IsNullOrEmpty(m_sGitShortHash))
            {
                sSuffix += $"{m_sGitShortHash}";
            }
            // Optional tag in parentheses (ignored by GameVersion.tryParse)
            if (useCustomVersionString)
            {
                sSuffix += $"_{customVersionSuffix}";
            }
            sSuffix += "]";

            // Build is always NEW scheme for naming: prefix + major.minor.milestone.commit
            // Legacy compatibility is handled by GameVersion.tryParse (3-part). Here we always output 4-part.
            string sVersionNumeric = $"{BuildPrefix} ({getBuildPrefix()}{buildMajor}.{buildMinorInternal}.{milestoneNumber}.{sCommit}{sSuffix})";

            return sVersionNumeric;
        }
    }

    #endregion

    #region Scripting Define Symbols

    [Title("Scripting Define Symbols", Bold = true, HorizontalLine = true)]
    [BoxGroup("Define Symbols")]
    [LabelText("(M)arketing Tool")]
    [Tooltip("Enable marketing tool features")]
    public bool marketingToolDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("(E)xhibition")]
    [Tooltip("Enable stand-specific features")]
    public bool exhibitionDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("(S)team")]
    [Tooltip("Enable Steam integration")]
    public bool steamDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("(A) Disable Analytics2")]
    [Tooltip("Disable Analytics2")]
    public bool analytics2Define = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Snapshots")]
    [Tooltip("Enable SaveData Snapshots")]
    public bool snapshotsDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("(C)ontroller")]
    [Tooltip("Enable controller support")]
    public bool controllerDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Tutorial")]
    [Tooltip("Enable tutorial system")]
    public bool tutorialDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Compendium")]
    [Tooltip("Enable compendium system")]
    public bool compendiumDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Druid Unlocked")]
    [Tooltip("Enable druid unlocked content")]
    public bool druidUnlockedDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Rogue Unlocked")]
    [Tooltip("Enable rogue unlocked content")]
    public bool rogueUnlockedDefine = false;

    [BoxGroup("Define Symbols")]
    [LabelText("Barbarian Unlocked")]
    [Tooltip("Enable barbarian unlocked content")]
    public bool barbarianUnlockedDefine = false;

    [BoxGroup("Define Symbols")]
    [Button(ButtonSizes.Medium), GUIColor(0.7f, 0.7f, 1f)]
    [LabelText("Apply")]
    [Tooltip("Apply selected define symbols to current build target")]
    private void ApplyDefineSymbols() => SetScriptingDefineSymbols();

    [BoxGroup("Define Symbols")]
    [ShowInInspector, ReadOnly, TextArea(1, 20)]
    [LabelText("Current Defines")]
    [Tooltip("Currently active define symbols from Build Profile and Player Settings")]
    private string CurrentDefineSymbols => GetCurrentDefineSymbols();

    #endregion

    #region Path Configuration

    [BoxGroup("Path Configuration")]
    [LabelText("Set Custom Path")]
    [Tooltip("Enable to specify a custom build output directory")]
    public bool bCustomPath;

    [BoxGroup("Path Configuration")]
    [LabelText("Exe Folder Path")]
    [ShowIf("@bCustomPath")]
    [FolderPath(ParentFolder = "Assets", RequireExistingPath = true)]
    [Tooltip("Custom directory for build output (overrides default Desktop/builds)")]
    public string customExeFolderPath;

    #endregion

    #region Batch mode Configuration

    [BoxGroup("Batch mode settings")]
    [LabelText("Builds history filename")]
    [Tooltip("Name of the file where build results will be appended to. The path is StandardBuildsPath")]
    public string m_sBuildsHistoryFilename;

    [BoxGroup("Batch mode settings")]
    [LabelText("Remove DoNotShip folder")]
    [Tooltip("Removes DoNotShip folder after finishing the build")]
    public bool m_bRemoveDoNotShipFolder;

    [BoxGroup("Batch mode settings")]
    [LabelText("Max build count")]
    [Tooltip("Maximum builds to keep. If any more are stored, remove the surplus before creating a new one")]
    public int m_iMaxBuildsCount;

    #endregion

    #region Version Tools (Prefab Update Only)

    [BoxGroup("Build")]
    [Button(ButtonSizes.Large), GUIColor(0.6f, 1.0f, 0.6f)]
    [EnableIf("@!IsBuilding")]
    [Tooltip("Fetches Git version (or uses custom) and updates DataManager prefab version fields.")]
    private void UpdateVersionInDataManagerPrefab()
    {
        if (m_oSelectedProfile == null)
        {
            Debug.LogError("No build profile selected. Cannot update version.");
            return;
        }

        if (EditorApplication.isCompiling)
        {
            Debug.LogWarning("Editor is compiling. Try again when compilation finishes.");
            return;
        }

        Debug.Log("Fetching Git commit count/hash, then updating DataManager prefab...");
        StartGitProcess();
        currentState = BuildState.FetchingGitVersion_ForPrefabOnly;
    }

    #endregion

    #region Build Execution

    [BoxGroup("Build"), HorizontalGroup("Build/options", width: 240)]
    [LabelText("Rename Existing Build Folder If Exists"), LabelWidth(215)]
    [Tooltip("If a build folder with the same name already exists, it will be renamed before creating a new build folder.")]
    public bool renameOldBuildIfExists = false;

    [BoxGroup("Build"), HorizontalGroup("Build/options", width: 50)]
    [LabelText("Zip"), LabelWidth(25)]
    public bool m_bCreateZip = false;

    [BoxGroup("Build"), HorizontalGroup("Build/options", width: 75)]
    [LabelText("Upload"), LabelWidth(50)]
    public bool m_bUploadToSteam = false;

    [BoxGroup("Build"), HorizontalGroup("Build/options2", width: 120), ShowIf("@m_bUploadToSteam")]
    [LabelText("MainOrPlaytest"), LabelWidth(95)]
    public bool m_bMainOrPlaytest = false;

    [BoxGroup("Build"), HorizontalGroup("Build/options2", width: 130), ShowIf("@m_bUploadToSteam")]
    [LabelText("User"), LabelWidth(30)]
    public string m_sSteamUser = "";
    private string m_sSteamworksPath = "";

    [BoxGroup("Build"), HorizontalGroup("Build/options2", width: 140), ShowIf("@m_bUploadToSteam")]
    [LabelText("Branch"), LabelWidth(40), ValueDropdown("@getBranches()")]
    public string m_sBranch = "";

    [BoxGroup("Build"), HorizontalGroup("Build/options2", width: 50), ShowIf("@m_bUploadToSteam")]
    [Button("Setup")]
    void setupPath()
    {
        m_sSteamworksPath = EditorUtility.OpenFolderPanel("Steamworks ContentBuilder Path", "", "");
        PlayerPrefs.SetString(PLAYER_PREFS_STEAMWORKS_PATH, m_sSteamworksPath);
    }

    [BoxGroup("Build"), HorizontalGroup("Build/options3"), ShowIf("@m_bUploadToSteam")]
    [LabelText("Comment"), LabelWidth(55)]
    public string m_sExtraComment = "";

    string[] getBranches()
    {
        if (m_bMainOrPlaytest) return new string[] { "quicktest", "pre-test", "reviewbuild", "testing", "testing2" };
        else return new string[] { "analytics", "pre-test", "marketing" };
    }

    [BoxGroup("Build")]
    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    [EnableIf("@!IsBuilding")]
    [Tooltip("Switches profile, applies defines, recompiles, and then starts the automated build process")]
    private void StartAutomatedBuild()
    {
        if (IsBuilding) return;
        if (m_oSelectedProfile == null) { Debug.LogError("Build cannot start: No build profile selected!"); return; }

        isBuilding = true; // Lock the UI

        if (BuildProfile.GetActiveBuildProfile() != m_oSelectedProfile)
        {
            Debug.Log($"Switching active build profile to '{m_oSelectedProfile.name}'...");
            SessionState.SetString(BuildStateKey, BuildState.WaitingForProfileRecompile.ToString());
            Utils.setActiveBuildProfile(m_oSelectedProfile);
            currentState = BuildState.WaitingForProfileRecompile;
        }
        else
        {
            Debug.Log("Active profile is already correct. Applying define symbols...");
            executeScriptingDefineSymbolsFlow();
        }
    }

    [BoxGroup("Discord")]
    [Header("Activate discord automation")]
    [SerializeField] private bool m_bActivateDiscordMessage = false;

    [BoxGroup("Discord")]
    [Header("Extra message")]
    [TextArea(3, 8)]
    [ShowIf("@m_bActivateDiscordMessage")]
    [SerializeField] private string m_sExtraMessage = "";

    [BoxGroup("Discord")]
    [TextArea(3, 8)]
    [ShowIf("@m_bActivateDiscordMessage")]
    [Header("Custom message")]
    [SerializeField] private string m_sCustomMessage = "";

    [BoxGroup("Discord")]
    [Header("Optional")]
    [ShowIf("@m_bActivateDiscordMessage")]
    [SerializeField] private string m_sDiscordBotName = "Build Automation";

    [Serializable]
    private sealed class DiscordWebhookPayload
    {
        public string content;
        public string username;
    }

    [Button]
    private void createZip()
    {
        string sZipFile = $"{pathToBuild}/../{PreviewBuildName}.zip";
        foreach (string sFolder in Directory.GetDirectories(pathToBuild, "*DoNot*"))
        {
            Directory.Delete(sFolder, true);
        }
        if (File.Exists(sZipFile)) File.Delete(sZipFile);
        ZipFile.CreateFromDirectory(pathToBuild, sZipFile);
    }

    [Button]
    private void uploadToSteam()
    {
        if (m_sBranch.isNullOrEmpty())
        {
            Debug.Log("Upload to steam cancelled. Branch empty. ");
            return;
        }

        if (m_sSteamUser.isNullOrEmpty())
        {
            Debug.Log("Upload to steam cancelled. User empty. ");
            return;
        }

        if (m_sSteamworksPath.isNullOrEmpty())
        {
            Debug.Log("Upload to steam cancelled. Steamworks path not set. ");
            return;
        }

        if (pathToBuild.isNullOrEmpty())
        {
            pathToBuild = EditorUtility.OpenFolderPanel("Choose build path", "", "");
        }

        string sContentBuilderPath = PlayerPrefs.GetString(PLAYER_PREFS_STEAMWORKS_PATH).Replace("\\", "/");// "C:/steamworks_sdk_163/tools/ContentBuilder/";
        string sToolsPath = $"{Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/")}/Tools/steamworks/";
        string sSteamContentFolder = $"{sToolsPath}build";

        // Prepare build content
        if (Directory.Exists(sSteamContentFolder)) Directory.Delete(sSteamContentFolder, true);
        copyDirectory(pathToBuild, sSteamContentFolder);
        foreach (string sFolder in Directory.GetDirectories(sSteamContentFolder, "*DoNot*"))
        {
            Directory.Delete(sFolder, true);
        }

        string sSteamCmd = $"{sContentBuilderPath}/builder/steamcmd.exe";
        string sVDF = $"{sToolsPath}rift_{(m_bMainOrPlaytest ? "main" : "playtest")}.vdf";
        string sVDFContent = File.ReadAllText(sVDF);
        sVDFContent = sVDFContent.Replace("##BRANCH##", m_sBranch);
        sVDFContent = sVDFContent.Replace("##COMMENT##", pathToBuild.Split('\\')[^1] + m_sExtraComment);
        string sVDFModified = sVDF + "_";
        File.Delete(sVDFModified);
        File.WriteAllText(sVDFModified, sVDFContent);
        string uUser = m_sSteamUser;
        string sArguments = $"+login {uUser} +run_app_build \"{sVDFModified}\" +quit";

        var oStartInfo = new ProcessStartInfo
        {
            FileName = sSteamCmd,
            Arguments = sArguments,
            UseShellExecute = true,
            CreateNoWindow = false
        };

        using Process oProcess = Process.Start(oStartInfo);

        if (oProcess == null)
        {
            Debug.LogError("Failed to start cmd.exe.");
            return;
        }

        oProcess.WaitForExit();

        if (oProcess.ExitCode == 0)
        {
            Debug.Log($"Steam build successfully uploaded via CMD.");
            if(m_bActivateDiscordMessage) sendDiscordMessage();
        }
        else
        {
            Debug.LogError(
                "Steam build failed.\n" +
                $"Exit Code: {oProcess.ExitCode}\n");
        }

        File.Delete(sVDFModified);
    }

    public void copyDirectory(string _sOrigin, string _sDestination)
    {
        DirectoryInfo dir = new DirectoryInfo(_sOrigin);

        if (!Directory.Exists(_sDestination)) Directory.CreateDirectory(_sDestination);

        foreach (FileInfo archivo in dir.GetFiles())
        {
            string rutaArchivoDestino = Path.Combine(_sDestination, archivo.Name);
            archivo.CopyTo(rutaArchivoDestino, true);
        }

        foreach (DirectoryInfo subDir in dir.GetDirectories())
        {
            string nuevaRutaDestino = Path.Combine(_sDestination, subDir.Name);
            copyDirectory(subDir.FullName, nuevaRutaDestino);
        }
    }

    //Method to be called in batched mode to generate builds. Uses whatever setting are saved.
    public /*async*/ static void StartAutomatedBuildBatched() // pitiku - remove async to remove the warning
    {
        DateTime oStartTime = DateTime.Now;
        BuildAutomationWindow oBuildAutomationWindow = GetWindow<BuildAutomationWindow>();
        oBuildAutomationWindow.m_bIsBatchMode = true;
        oBuildAutomationWindow.Show(true);
        string sBuildHistoryPath = $"{StandardBuildsPath}\\{oBuildAutomationWindow.m_sBuildsHistoryFilename}";
        try
        {
            RemoveOldBuilds(StandardBuildsPath, oBuildAutomationWindow.m_iMaxBuildsCount);
            oBuildAutomationWindow.StartAutomatedBuild();
            //Wait until the build finishes
            while (oBuildAutomationWindow.currentState != BuildState.Idle)
            {
                oBuildAutomationWindow.EditorUpdate();
            }
            oBuildAutomationWindow.Close();
            if (oBuildAutomationWindow.m_bRemoveDoNotShipFolder)
            {
                RemoveDoNotShipFolder(oBuildAutomationWindow.pathToBuild);
            }
            DateTime oEndTime = DateTime.Now;
            BuildResult eBuildResult = oBuildAutomationWindow.m_oLastBuildRepot?.summary.result ?? BuildResult.Failed;
            WriteToBatchResultsHistory(sBuildHistoryPath,
                $"[{oEndTime}] Build {oBuildAutomationWindow.PreviewBuildName} finished in {oEndTime - oStartTime} with result {eBuildResult}\n");
        }
        catch (Exception e)
        {
            DateTime oEndTime = DateTime.Now;
            WriteToBatchResultsHistory(sBuildHistoryPath,
                $"[{oEndTime}] Build {oBuildAutomationWindow.PreviewBuildName} failed in {oEndTime - oStartTime} with exception {e}\n");
        }
    }

    private static void RemoveOldBuilds(string _sBuildsPath, int _iMaxBuildsCount)
    {
        try
        {
            string[] aoBuildFolders = Directory.GetDirectories(_sBuildsPath);
            int iBuildFoldersCount = aoBuildFolders.Length;
            if (iBuildFoldersCount >= _iMaxBuildsCount)
            {
                DateTime[] aoCreationTimes = new DateTime[iBuildFoldersCount];
                for (int iBuildFolderIndex = 0; iBuildFolderIndex < iBuildFoldersCount; ++iBuildFolderIndex)
                {
                    aoCreationTimes[iBuildFolderIndex] = Directory.GetCreationTime(aoBuildFolders[iBuildFolderIndex]);
                }
                Array.Sort(aoCreationTimes, aoBuildFolders);
                //We're about to add a new build, so remove an additional one
                int iBuildsToRemoveCount = iBuildFoldersCount - _iMaxBuildsCount + 1;
                foreach (string sBuildPath in aoBuildFolders.AsSpan(0, iBuildsToRemoveCount))
                {
                    Debug.Log($"Remove build '{sBuildPath}'");
                    Directory.Delete(sBuildPath, true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to remove old builds from '{_sBuildsPath}'");
            Debug.LogException(e);
        }
    }

    private static void RemoveDoNotShipFolder(string _sBuildPath)
    {
        try
        {
            string[] aoDoNotShipFolders = Directory.GetDirectories(_sBuildPath, "*DoNotShip");
            foreach (string sDoNotShipFolder in aoDoNotShipFolders)
            {
                Directory.Delete(sDoNotShipFolder, true);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to remove DoNotShip folders from '{_sBuildPath}'");
            Debug.LogException(e);
        }
    }

    private static void WriteToBatchResultsHistory(string _sBuildHistoryPath, string _sResult)
    {
        try
        {
            using (FileStream oBuildHistoryFileStream = File.Open(_sBuildHistoryPath, FileMode.Append, FileAccess.Write))
            {
                Byte[] sResult = new UTF8Encoding(true).GetBytes(_sResult);
                oBuildHistoryFileStream.Write(sResult, 0, sResult.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Could write results to '{_sBuildHistoryPath}'");
            Debug.LogException(e);
        }
    }

    #endregion

    #region Private Fields

    private bool IsBuilding => isBuilding || currentState == BuildState.Building;
    private bool isBuilding = false;

    // NEW: keep commit count + hash separately (PreviewBuildName uses commit count as numeric)
    private string m_sGitCommitCount = "0";
    private string m_sGitShortHash = "";

    private string pathToBuild;
    private const string BuildStateKey = "BuildAutomation.BuildState";
    private List<EditorBuildSettingsScene> m_aoInitialEditorBuildSettingsScenes = new();

    // Fields for our manual state machine
    private BuildState currentState = BuildState.Idle;
    private Process gitProcess;

    // Fields for batch build
    private bool m_bIsBatchMode;
    private BuildReport m_oLastBuildRepot;

    #endregion

    #region Initialization & State Management

    [MenuItem("UPTools/Build Automation", false, 50)]
    private static void OpenWindow() => GetWindow<BuildAutomationWindow>().Show();

    const string PLAYER_PREFS_STEAM_USER = "STEAM_USER";
    const string PLAYER_PREFS_STEAMWORKS_PATH = "STEAMWORKS_PATH";

    protected override void OnEnable()
    {
        base.OnEnable();
        EditorApplication.update += EditorUpdate; // Subscribe to the editor's "Update" loop

        LoadEditorPreferences();
        LoadDefineSymbolsState();

        // Restore state after a recompile
        string buildStateStr = SessionState.GetString(BuildStateKey, "");
        if (!string.IsNullOrEmpty(buildStateStr) && Enum.TryParse(buildStateStr, out BuildState restoredState))
        {
            SessionState.SetString(BuildStateKey, "");
            currentState = restoredState;
            isBuilding = true;
            Debug.Log($"Restored build state to: {currentState}");
        }
        else if (IsBuilding)
        {
            Debug.LogWarning("Build was interrupted. Resetting UI state.");
            ResetBuildState();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EditorApplication.update -= EditorUpdate; // ALWAYS unsubscribe to prevent leaks

        SaveEditorPreferences();

        // Clean up the process if the window is closed mid-operation
        if (gitProcess != null && !gitProcess.HasExited)
        {
            gitProcess.Kill();
            gitProcess = null;
        }
        resetScenes();
    }

    #endregion

    #region Build Flow (Manual State Machine)

    private void EditorUpdate()
    {
        if (currentState == BuildState.Idle || currentState == BuildState.Building) return;

        switch (currentState)
        {
            case BuildState.WaitingForProfileRecompile:
                if (!EditorApplication.isCompiling)
                {
                    executeScriptingDefineSymbolsFlow();
                }
                break;

            case BuildState.WaitingForDefinesRecompile:
                if (!EditorApplication.isCompiling)
                {
                    StartGitProcess();
                }
                break;

            case BuildState.FetchingGitVersion:
            case BuildState.FetchingGitVersion_ForPrefabOnly:
                if (gitProcess != null && gitProcess.HasExited)
                {
                    HandleGitProcessResult();
                }
                break;

            case BuildState.ReadyToBuild:
                Debug.Log("All pre-build steps complete. Starting final build.");
                ContinueBuildProcess();
                break;
        }
    }

    private void ContinueBuildProcess()
    {
        try
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            SetVersionInPrefab();
            pathToBuild = CreateBuildDirectories();
            if (string.IsNullOrEmpty(pathToBuild))
            {
                throw new Exception("Failed to generate build directories.");
            }
            ExecuteBuildPipeline();
        }
        catch (Exception e)
        {
            Debug.LogError($"Automated build process failed: {e.Message}\n{e.StackTrace}");
            ResetBuildState();
        }
        finally
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            resetScenes();
            ResetBuildState();
        }
    }

    private void ExecuteBuildPipeline()
    {
        if (!ValidateBuildParameters()) { ResetBuildState(); return; }
        DateTime buildStartTime = DateTime.Now;
        try
        {
            currentState = BuildState.Building;
            OnLoadSceneFunctions.onBuildStuff();
            setScenes();
            var buildOptions = new BuildPlayerWithProfileOptions
            {
                buildProfile = m_oSelectedProfile,
                locationPathName = GetBuildPath(),
                options = BuildOptions.None,
            };
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            m_oLastBuildRepot = report;
            HandleBuildResult(report);

            if (report != null && report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                if (m_bCreateZip) createZip();
                if (m_bUploadToSteam) uploadToSteam();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Build failed: {e.Message}");
        }
        finally
        {
            TimeSpan buildDuration = DateTime.Now - buildStartTime;
            Debug.Log($"Build completed in {buildDuration.Minutes} min {buildDuration.Seconds} sec.");
        }
    }

    private void ResetBuildState()
    {
        SessionState.SetString(BuildStateKey, "");
        currentState = BuildState.Idle;
        isBuilding = false;
        Debug.Log("Build process finished. UI unlocked.");
    }

    #endregion

    #region Git Integration (Non-Blocking)

    private void StartGitProcess()
    {
        Debug.Log("Starting Git process to fetch commit count...");
        var processInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-list --count HEAD",
            WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        gitProcess = new Process { StartInfo = processInfo };
        gitProcess.Start();
        currentState = BuildState.FetchingGitVersion;
    }

    private void HandleGitProcessResult()
    {
        try
        {
            if (gitProcess.ExitCode != 0)
                throw new Exception(gitProcess.StandardError.ReadToEnd());

            string sCommitCount = gitProcess.StandardOutput.ReadToEnd().Trim();
            if (string.IsNullOrEmpty(sCommitCount))
                throw new Exception("Git commit count was empty.");

            // Dispose the first process, then run a second one for short hash.
            gitProcess?.Dispose();
            gitProcess = null;

            var processInfoHash = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var oHashProcess = new Process { StartInfo = processInfoHash })
            {
                oHashProcess.Start();
                oHashProcess.WaitForExit();

                if (oHashProcess.ExitCode != 0)
                    throw new Exception(oHashProcess.StandardError.ReadToEnd());

                string sShortHash = oHashProcess.StandardOutput.ReadToEnd().Trim();

                m_sGitCommitCount = sCommitCount;
                m_sGitShortHash = sShortHash;

                Debug.Log($"Successfully fetched Git version numbers => commitCount={m_sGitCommitCount}, shortHash={m_sGitShortHash}");

                // if this was a prefab-only update, do it now and stop.
                if (currentState == BuildState.FetchingGitVersion_ForPrefabOnly)
                {
                    SetVersionInPrefab();
                    currentState = BuildState.Idle;
                    return;
                }

                currentState = BuildState.ReadyToBuild;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Git process failed: {e.Message}");
            ResetBuildState();
        }
        finally
        {
            gitProcess?.Dispose();
            gitProcess = null;
        }
    }

    #endregion

    #region Prefab Adds

    void SetVersionInPrefab()
    {
        // IMPORTANT: PreviewBuildName is now parseable by GameVersion.tryParse:
        // prefix + major.minor.milestone.commit(optionalTag)
        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(DataManager.Instance.gameObject);
        DataManager oDataManagerPrefab = PrefabUtility.LoadPrefabContents(prefabPath).GetComponent<DataManager>();
        Undo.RecordObject(oDataManagerPrefab, "Update Build Version");
        //oDataManagerPrefab.CurrentVersion = PreviewBuildName;

        // This will now succeed with the new GameVersion parser (legacy 3-part OR new 4-part).
        EditorUtility.SetDirty(oDataManagerPrefab);
        PrefabUtility.SaveAsPrefabAsset(oDataManagerPrefab.gameObject, prefabPath);
        PrefabUtility.UnloadPrefabContents(oDataManagerPrefab.gameObject);
        Debug.Log($"Version set to '{PreviewBuildName}' in prefab.");
    }

    #endregion

    #region Scripting Define Symbols Management

    private void executeScriptingDefineSymbolsFlow()
    {
        SessionState.SetString(BuildStateKey, BuildState.WaitingForDefinesRecompile.ToString());
        SetScriptingDefineSymbols();
        currentState = BuildState.WaitingForDefinesRecompile;
    }

    private void RecompileScripts()
    {
        AssetDatabase.Refresh();
        CompilationPipeline.RequestScriptCompilation();
    }

    private void SetScriptingDefineSymbols()
    {
        if (m_oSelectedProfile != null)
        {
            SetBuildProfileDefines();
            return;
        }
        SetPlayerSettingsDefines();
    }

    private void SetBuildProfileDefines()
    {
        var newDefinesList = new List<string>();

        if (marketingToolDefine) newDefinesList.Add(DefineSymbols.MARKETING_TOOL);
        if (analytics2Define) newDefinesList.Add(DefineSymbols.DISABLE_ANALYTICS2);
        if (snapshotsDefine) newDefinesList.Add(DefineSymbols.SNAPSHOTS);
        if (exhibitionDefine) newDefinesList.Add(DefineSymbols.EXHIBITION);
        if (!steamDefine) newDefinesList.Add(DefineSymbols.DISABLE_STEAM);
        if (tutorialDefine) newDefinesList.Add(DefineSymbols.TUTORIAL);
        if (druidUnlockedDefine) newDefinesList.Add(DefineSymbols.DRUID_UNLOCKED);
        if (rogueUnlockedDefine) newDefinesList.Add(DefineSymbols.ROGUE_UNLOCKED);
        if (barbarianUnlockedDefine) newDefinesList.Add(DefineSymbols.BARBARIAN_UNLOCKED);
        if (compendiumDefine) newDefinesList.Add(DefineSymbols.COMPENDIUM);
        if (controllerDefine) newDefinesList.Add(DefineSymbols.CONTROLLER);

        if (m_oSelectedProfile.name.toLower().Contains("demo"))
        {
            newDefinesList.Add("VERSION_DEMO");
        }
        else
        {
            newDefinesList.Add("VERSION_MAIN");
        }

        var currentDefines = m_oSelectedProfile.scriptingDefines ?? new string[0];

        var currentDefinesSet = new HashSet<string>(currentDefines);
        var newDefinesSet = new HashSet<string>(newDefinesList);

        if (currentDefinesSet.SetEquals(newDefinesSet))
        {
            Debug.Log("Scripting defines are already up-to-date. No recompile needed.");
            if (BuildProfile.GetActiveBuildProfile() != m_oSelectedProfile)
            {
                Utils.setActiveBuildProfile(m_oSelectedProfile);
            }
            return;
        }

        Debug.Log($"Applying updated scripting defines to Build Profile '{m_oSelectedProfile.name}': {string.Join(";", newDefinesList)}");
        m_oSelectedProfile.scriptingDefines = newDefinesList.ToArray();

        EditorUtility.SetDirty(m_oSelectedProfile);
        AssetDatabase.SaveAssets();

        if (BuildProfile.GetActiveBuildProfile() != m_oSelectedProfile)
        {
            Utils.setActiveBuildProfile(m_oSelectedProfile);
        }
        else
        {
            RecompileScripts();
        }
    }

    private void SetPlayerSettingsDefines()
    {
        var definesList = new List<string>();

        if (marketingToolDefine) definesList.Add(DefineSymbols.MARKETING_TOOL);
        if (analytics2Define) definesList.Add(DefineSymbols.DISABLE_ANALYTICS2);
        if (snapshotsDefine) definesList.Add(DefineSymbols.SNAPSHOTS);
        if (exhibitionDefine) definesList.Add(DefineSymbols.EXHIBITION);
        if (!steamDefine) definesList.Add(DefineSymbols.DISABLE_STEAM);
        if (tutorialDefine) definesList.Add(DefineSymbols.TUTORIAL);
        if (druidUnlockedDefine) definesList.Add(DefineSymbols.DRUID_UNLOCKED);
        if (rogueUnlockedDefine) definesList.Add(DefineSymbols.ROGUE_UNLOCKED);
        if (barbarianUnlockedDefine) definesList.Add(DefineSymbols.BARBARIAN_UNLOCKED);
        if (compendiumDefine) definesList.Add(DefineSymbols.COMPENDIUM);
        if (controllerDefine) definesList.Add(DefineSymbols.CONTROLLER);

        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        var currentDefinesList = string.IsNullOrEmpty(currentDefines) ?
            new List<string>() :
            currentDefines.Split(';').Where(d => !string.IsNullOrEmpty(d)).ToList();

        var preservedDefines = currentDefinesList.Where(d => !DefineSymbols.ALL_MANAGED_DEFINES.Contains(d)).ToList();

        var allDefines = preservedDefines.Concat(definesList).Distinct().ToList();
        var defineString = string.Join(";", allDefines);

        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defineString);

        Debug.Log($"Applied scripting defines to Player Settings: {defineString}");
    }

    private void LoadDefineSymbolsState()
    {
        List<string> activeDefines;

        if (m_oSelectedProfile != null && m_oSelectedProfile.scriptingDefines != null)
        {
            activeDefines = m_oSelectedProfile.scriptingDefines.ToList();
        }
        else
        {
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            activeDefines = string.IsNullOrEmpty(currentDefines) ?
                new List<string>() :
                currentDefines.Split(';').Where(d => !string.IsNullOrEmpty(d)).ToList();
        }

        marketingToolDefine = activeDefines.Contains(DefineSymbols.MARKETING_TOOL);
        analytics2Define = activeDefines.Contains(DefineSymbols.DISABLE_ANALYTICS2);
        snapshotsDefine = activeDefines.Contains(DefineSymbols.SNAPSHOTS);
        exhibitionDefine = activeDefines.Contains(DefineSymbols.EXHIBITION);
        steamDefine = !activeDefines.Contains(DefineSymbols.DISABLE_STEAM);
        tutorialDefine = activeDefines.Contains(DefineSymbols.TUTORIAL);
        druidUnlockedDefine = activeDefines.Contains(DefineSymbols.DRUID_UNLOCKED);
        rogueUnlockedDefine = activeDefines.Contains(DefineSymbols.ROGUE_UNLOCKED);
        barbarianUnlockedDefine = activeDefines.Contains(DefineSymbols.BARBARIAN_UNLOCKED);
        compendiumDefine = activeDefines.Contains(DefineSymbols.COMPENDIUM);
        controllerDefine = activeDefines.Contains(DefineSymbols.CONTROLLER);
    }

    private string GetCurrentDefineSymbols()
    {
        if (m_oSelectedProfile != null && m_oSelectedProfile.scriptingDefines != null && m_oSelectedProfile.scriptingDefines.Length > 0)
        {
            return string.Join("\n", m_oSelectedProfile.scriptingDefines);
        }

        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var playerDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        return string.IsNullOrEmpty(playerDefines) ? "No defines set." : playerDefines.Replace(';', '\n');
    }

    #endregion

    #region Scene Management

    private void setScenes()
    {
        if (customScenes) return;
        m_aoInitialEditorBuildSettingsScenes = EditorBuildSettings.scenes.ToList();

        List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        foreach (var oBuildedScenes in EditorBuildSettings.scenes)
        {
            if (oBuildedScenes.path.Contains("MAP") || oBuildedScenes.path.Contains("intro_scene") || oBuildedScenes.path.Contains("MainMenu") || oBuildedScenes.path.Contains("loading"))
            {
                editorBuildSettingsScenes.Add(oBuildedScenes);
            }
        }
        List<SceneAsset> aoSceneAssets = Utils.findAssetsByType<SceneAsset>(new string[] { "Assets/Scenes/nodes/mountain", "Assets/Scenes/nodes/forest", "Assets/Scenes/nodes/moor", "Assets/Scenes/nodes/desert", "Assets/Scenes/nodes/blighted" }).Where(oTarget => oTarget.name.Contains("scn")).ToList();

        EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
    }

    private void resetScenes()
    {
        if (m_aoInitialEditorBuildSettingsScenes.Any())
        {
            EditorBuildSettings.scenes = m_aoInitialEditorBuildSettingsScenes.ToArray();
        }
        m_aoInitialEditorBuildSettingsScenes.Clear();
    }

    private bool ValidateBuildParameters()
    {
        if (m_oSelectedProfile == null)
        {
            Debug.LogError("Build aborted: No profile selected!");
            return false;
        }

        if (bCustomPath && string.IsNullOrEmpty(customExeFolderPath))
        {
            Debug.LogError("Build aborted: Invalid custom path!");
            return false;
        }

        return true;
    }

    private void HandleBuildResult(UnityEditor.Build.Reporting.BuildReport report)
    {
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            if (!m_bIsBatchMode)
            {
                EditorUtility.RevealInFinder(pathToBuild);
            }
            Debug.Log($"Build succeeded! Output at: {pathToBuild}");
        }
        else
        {
            Debug.LogError("Build failed. Check console for details.");
        }
    }

    #endregion

    #region Utility Methods

    private bool ValidateString(string value) => !string.IsNullOrWhiteSpace(value);

#if UNITY_SWITCH
    private string GetBuildPath() => Path.Combine(pathToBuild, $"{PlayerSettings.productName}.NSP");
#else
    private string GetBuildPath() => Path.Combine(pathToBuild, $"{PlayerSettings.productName}.exe");
#endif

    public string getBuildPrefix()
    {
        if (m_oSelectedProfile == null) return "error";

        string sPrefix = "";
        if (m_oSelectedProfile.name.Contains("DEMO", StringComparison.OrdinalIgnoreCase))
        {
            sPrefix += "d";
        }

        // NOTE: your original code uses ".contains" which is not C#.
        // I kept your logic intent but fixed the call to .Contains to compile.
        if (m_oSelectedProfile.scriptingDefines == null || !m_oSelectedProfile.scriptingDefines.Contains(DefineSymbols.DISABLE_STEAM))
        {
            sPrefix += "s";
        }

        if (m_oSelectedProfile.scriptingDefines != null && m_oSelectedProfile.scriptingDefines.Contains(DefineSymbols.CONTROLLER))
        {
            sPrefix += "c";
        }

        if (m_oSelectedProfile.scriptingDefines != null && 
            !m_oSelectedProfile.scriptingDefines.Contains(DefineSymbols.DISABLE_ANALYTICS2))
        {
            sPrefix += "a";
        }

        if (m_oSelectedProfile.scriptingDefines != null && m_oSelectedProfile.scriptingDefines.Contains(DefineSymbols.EXHIBITION))
        {
            sPrefix += "e";
        }
        if (m_oSelectedProfile.scriptingDefines != null && m_oSelectedProfile.scriptingDefines.Contains(DefineSymbols.MARKETING_TOOL))
        {
            sPrefix += "m";
        }
        return sPrefix;
    }

#endregion

    #region Persistent Storage
    const string BUILD_PROFILE_KEY = "Build_BuildProfileKey";
    string getFolderName() => Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
    private string getLocalPrefKey(string _sKey) => _sKey + "_" + getFolderName();

    private void LoadEditorPreferences()
    {
        LoadSharedSettings();

        customExeFolderPath = EditorPrefs.GetString("Build_InputPath", "");

        var savedProfilePath = EditorPrefs.GetString(getLocalPrefKey(BUILD_PROFILE_KEY));
        if (!string.IsNullOrEmpty(savedProfilePath))
        {
            setSelectedProfile(AssetDatabase.LoadAssetAtPath<BuildProfile>(savedProfilePath));
        }

        if (m_oSelectedProfile == null)
        {
            Debug.Log("No saved profile found or profile was invalid. Searching for a default profile...");
            SelectDefaultBuildProfile();
        }

        m_sBuildsHistoryFilename = EditorPrefs.GetString("Build_BuildsHistoryFilename", "History.txt");
        m_bRemoveDoNotShipFolder = EditorPrefs.GetBool("Build_RemoveDoNotShipFolder", true);
        m_iMaxBuildsCount = EditorPrefs.GetInt("Build_MaxBuildsCount", 10);

        m_sSteamUser = PlayerPrefs.GetString(PLAYER_PREFS_STEAM_USER);
        m_sSteamworksPath = PlayerPrefs.GetString(PLAYER_PREFS_STEAMWORKS_PATH);

        m_bActivateDiscordMessage = EditorPrefs.GetBool("Discord_Message_Activated");
        m_sDiscordBotName = EditorPrefs.GetString("Discord_User_Bot");
    }

    private void SelectDefaultBuildProfile()
    {
        string[] profileGuids = AssetDatabase.FindAssets("t:BuildProfile");
        if (profileGuids.Length == 0)
        {
            Debug.LogWarning("Build Automation: No Build Profiles found anywhere in the project. Please create one.");
            return;
        }

        var allProfiles = profileGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<BuildProfile>(path))
            .Where(profile => profile != null)
            .ToList();

        if (!allProfiles.Any())
        {
            Debug.LogWarning("Build Automation: Failed to load any Build Profile assets. Please check for corruption.");
            return;
        }

        string[] defaultNames = { "Base", "Default", "Main" };
        BuildProfile defaultProfile = allProfiles.FirstOrDefault(p =>
            defaultNames.Any(name => p.name.Equals(name, StringComparison.OrdinalIgnoreCase))
        );

        if (defaultProfile == null)
        {
            setSelectedProfile(allProfiles.First());
            Debug.Log($"Build Automation: No profile named 'Base', 'Default', or 'Main' found. Selected the first available profile: '{m_oSelectedProfile.name}'");
        }
        else
        {
            setSelectedProfile(defaultProfile);
            Debug.Log($"Build Automation: Default profile found and selected: '{m_oSelectedProfile.name}'");
        }
    }

    private void SaveEditorPreferences()
    {
        SaveSharedSettings();

        EditorPrefs.SetString("Build_InputPath", customExeFolderPath);

        if (m_oSelectedProfile != null)
        {
            EditorPrefs.SetString(getLocalPrefKey(BUILD_PROFILE_KEY), AssetDatabase.GetAssetPath(m_oSelectedProfile));
        }

        EditorPrefs.SetString("Build_BuildsHistoryFilename", m_sBuildsHistoryFilename);
        EditorPrefs.SetBool("Build_RemoveDoNotShipFolder", m_bRemoveDoNotShipFolder);
        EditorPrefs.SetInt("Build_MaxBuildsCount", m_iMaxBuildsCount);

        PlayerPrefs.SetString(PLAYER_PREFS_STEAM_USER, m_sSteamUser);

        EditorPrefs.SetBool("Discord_Message_Activated", m_bActivateDiscordMessage);
        EditorPrefs.SetString("Discord_User_Bot", m_sDiscordBotName);
    }

    #endregion

    #region Directory Management

    //Builds will be written to the "builds" folder in the desktop
    private static string StandardBuildsPath => Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "builds"), BuildProfile.GetActiveBuildProfile().name);

    private string CreateBuildDirectories()
    {
        try
        {
            if (bCustomPath)
            {
                if (!Directory.Exists(customExeFolderPath))
                {
                    throw new Exception($"Invalid custom path: {customExeFolderPath}");
                }
                return customExeFolderPath;
            }
            else
            {
                var buildsFolder = StandardBuildsPath;
                var versionFolder = Path.Combine(buildsFolder, PreviewBuildName);

                Directory.CreateDirectory(buildsFolder);

                // If renaming is enabled and target folder doesn't exist, show folder picker
                if (renameOldBuildIfExists && !Directory.Exists(versionFolder))
                {
                    var folders = Directory.GetDirectories(buildsFolder);

                    if (folders.Length > 0)
                    {
                        string selectedFolder = ShowFolderPickerDialog(folders);

                        if (!string.IsNullOrEmpty(selectedFolder))
                        {
                            Directory.Move(selectedFolder, versionFolder);
                            Debug.Log($"Renamed old build folder: '{Path.GetFileName(selectedFolder)}' → '{PreviewBuildName}'");
                        }
                        else
                        {
                            Debug.LogWarning("No folder selected. Creating new folder.");
                            Directory.CreateDirectory(versionFolder);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(versionFolder);
                }

                return versionFolder;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Directory creation failed: {ex.Message}");
            return null;
        }
    }

    private string ShowFolderPickerDialog(string[] folders)
    {
        List<string> folderNames = folders.Select(f => Path.GetFileName(f)).ToList();

        int selectedIndex = EditorUtility.DisplayDialog("Reuse Build Folder",
            $"Select a folder to reuse:\n\n{string.Join("\n", folderNames.Select((f, i) => $"{i + 1}. {f}"))}\n\nCancel to create new folder.",
            folderNames.Count > 0 ? folderNames[0] : "Cancel",
            folderNames.Count > 1 ? folderNames[1] : "New Folder") ? 0 : -1;

        if (selectedIndex >= 0 && selectedIndex < folders.Length)
        {
            return folders[selectedIndex];
        }

        return null;
    }

    #endregion

    #region Discord

    const string DISCORD_WEB_HOOK = "https://discord.com/api/webhooks/1481634083262955600/PJ-E7n4PrFV8KgCO9KygPvMDLwBvlzhQ36x18xj5I6gHge96Nka6YGlCcKcEIQVqLd13";

    [Button, ShowIf("@m_bActivateDiscordMessage")]
    public void sendDiscordMessage()
    {
#if !(UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    Debug.LogError("This CMD version only works on Windows Editor / Windows Standalone.");
    return;
#else
        try
        {
            DiscordWebhookPayload oPayload = new DiscordWebhookPayload
            {
                content = createMessageDiscord(),
                username = string.IsNullOrWhiteSpace(m_sDiscordBotName) ? null : m_sDiscordBotName
            };

            string sJson = JsonUtility.ToJson(oPayload);

            string sTempDirectory = Path.Combine(Path.GetTempPath(), "UnityDiscordWebhook");
            Directory.CreateDirectory(sTempDirectory);

            string sPayloadPath = Path.Combine(sTempDirectory, $"discord_payload_{Guid.NewGuid():N}.json");
            string sBatchPath = Path.Combine(sTempDirectory, $"discord_send_{Guid.NewGuid():N}.cmd");

            File.WriteAllText(sPayloadPath, sJson);
            File.WriteAllText(sBatchPath, buildBatchFileContents(sPayloadPath, DISCORD_WEB_HOOK));

            ProcessStartInfo oStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{sBatchPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = sTempDirectory
            };

            using Process oProcess = Process.Start(oStartInfo);

            if (oProcess == null)
            {
                Debug.LogError("Failed to start cmd.exe.");
                cleanupTempFiles(sPayloadPath, sBatchPath);
                return;
            }

            string sStdOut = oProcess.StandardOutput.ReadToEnd();
            string sStdErr = oProcess.StandardError.ReadToEnd();

            oProcess.WaitForExit();

            if (oProcess.ExitCode == 0)
            {
                Debug.Log($"Discord message sent successfully via CMD.\n{sStdOut}");
            }
            else
            {
                Debug.LogError(
                    "Failed to send Discord message via CMD.\n" +
                    $"Exit Code: {oProcess.ExitCode}\n" +
                    $"STDOUT: {sStdOut}\n" +
                    $"STDERR: {sStdErr}");
            }

            cleanupTempFiles(sPayloadPath, sBatchPath);
        }
        catch (Exception _exception)
        {
            Debug.LogException(_exception);
        }
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private string buildBatchFileContents(string _sPayloadPath, string _sWebhookUrl)
    {
        string sEscapedPayloadPath = _sPayloadPath.Replace("\"", "\"\"");
        string sEscapedWebhookUrl = _sWebhookUrl.Replace("\"", "\"\"");

        return
            "@echo off\r\n" +
            "setlocal\r\n" +
            "curl --fail-with-body " +
            "-H \"Content-Type: application/json\" " +
            $"--data \"@{sEscapedPayloadPath}\" " +
            $"\"{sEscapedWebhookUrl}\"\r\n" +
            "exit /b %errorlevel%\r\n";
    }

    private void cleanupTempFiles(string _sPayloadPath, string _sBatchPath)
    {
        tryDeleteFile(_sPayloadPath);
        tryDeleteFile(_sBatchPath);
    }

    private void tryDeleteFile(string _sPath)
    {
        try
        {
            if (File.Exists(_sPath))
            {
                File.Delete(_sPath);
            }
        }
        catch (Exception _exception)
        {
            Debug.LogWarning($"Could not delete temp file '{_sPath}'. Exception: {_exception.Message}");
        }
    }
    private string createMessageDiscord()
    {
        if (!m_sCustomMessage.isNullOrEmpty())
        {
            return m_sCustomMessage;
        }
        return $"@everyone 📦 New Build Deployed!\r\n\r\n🔢 Version: {pathToBuild.Split('\\')[^1]}\r\n\r\n\U0001f9f1 Build: {(m_bMainOrPlaytest ? "main" : "playtest")}\r\n\U0001f9ea Branch: {m_sBranch}\r\n⚠️ Comprobad la versión siempre xfiss \r\n{m_sExtraMessage}";
    }
#endif

    #endregion

    [InitializeOnLoadMethod]
    private static void ApplyDefineSymbolsOnEditorStartup()
    {
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool(ApplyDefinesOnStartupSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(ApplyDefinesOnStartupSessionKey, true);

            var sharedSettings = AssetDatabase.LoadAssetAtPath<BuildAutomationSharedSettings>(SharedSettingsAssetPath);
            if (sharedSettings != null && !sharedSettings.applyDefineSymbolsOnEditorStartup)
            {
                return;
            }

            var window = CreateInstance<BuildAutomationWindow>();
            try
            {
                window.LoadEditorPreferences();
                window.LoadDefineSymbolsState();
                window.ApplyDefineSymbols();
                Debug.Log("BuildAutomation: ApplyDefineSymbols executed on editor startup.");
            }
            catch (Exception e)
            {
                Debug.LogError($"BuildAutomation startup define apply failed: {e.Message}");
            }
            finally
            {
                DestroyImmediate(window);
            }
        };
    }

    private void LoadSharedSettings()
    {
        m_oSharedSettings = AssetDatabase.LoadAssetAtPath<BuildAutomationSharedSettings>(SharedSettingsAssetPath);
        if (m_oSharedSettings == null)
        {
            string directoryPath = Path.GetDirectoryName(SharedSettingsAssetPath);
            if (!string.IsNullOrEmpty(directoryPath) && !AssetDatabase.IsValidFolder(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            m_oSharedSettings = CreateInstance<BuildAutomationSharedSettings>();

            // one-time migration from local EditorPrefs defaults
            m_oSharedSettings.buildMajor = EditorPrefs.GetString("Build_Major", m_oSharedSettings.buildMajor);
            m_oSharedSettings.buildMinor = EditorPrefs.GetString("Build_Minor", m_oSharedSettings.buildMinor);
            m_oSharedSettings.buildMinorInternal = EditorPrefs.GetString("Build_Minor_Internal", m_oSharedSettings.buildMinorInternal);
            m_oSharedSettings.milestoneNumber = EditorPrefs.GetString("Build_MilestoneNumber", m_oSharedSettings.milestoneNumber);

            AssetDatabase.CreateAsset(m_oSharedSettings, SharedSettingsAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        buildMajor = m_oSharedSettings.buildMajor;
        buildMinor = m_oSharedSettings.buildMinor;
        buildMinorInternal = m_oSharedSettings.buildMinorInternal;
        milestoneNumber = m_oSharedSettings.milestoneNumber;
    }

    private void SaveSharedSettings()
    {
        if (m_oSharedSettings == null)
        {
            m_oSharedSettings = AssetDatabase.LoadAssetAtPath<BuildAutomationSharedSettings>(SharedSettingsAssetPath);
            if (m_oSharedSettings == null)
            {
                return;
            }
        }

        m_oSharedSettings.buildMajor = buildMajor;
        m_oSharedSettings.buildMinor = buildMinor;
        m_oSharedSettings.buildMinorInternal = buildMinorInternal;
        m_oSharedSettings.milestoneNumber = milestoneNumber;

        EditorUtility.SetDirty(m_oSharedSettings);
        AssetDatabase.SaveAssets();
    }
}