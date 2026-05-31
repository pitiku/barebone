#if UNITY_GAMECORE

using System.Threading;
#if !UNITY_6000_0_OR_NEWER
using System.Threading.Tasks;
#endif
using Unity.XGamingRuntime;
using UnityEngine;

public class GDKGameRuntime : MonoBehaviour
{
    public static string GameConfigSandbox { get; private set; } = "JRWQQX.99";

    // Documented as: "Specifies the SCID to be used for Save Game Storage."
    public static string GameConfigScid { get; private set; } = "00000000-0000-0000-0000-0000703a4439";

    // Documented as: "...a default value of 'FFFFFFFF' for this element. This allows for early iteration of your
    //   title without having to immediately acquire the Id from Partner Center. It is strongly recommended to change
    //   this Id as soon as you get your title building to avoid failures when attempting to do API calls."
    public static string GameConfigTitleId { get; private set; } = "703a4439";

    public static bool Initialized { get; private set; }
    public static GDKGameRuntime Instance { get; private set; }

    private CancellationTokenSource m_CancellationTokenSource;

    public static bool TryInitialize()
    {
        if (Instance == null)
        {
            OnScreenLog.Add("GDK XGameRuntime Library not initialized.");
            // Instantiate the GDKGameRuntime object that will call Awake.
            new GameObject("GDKGamingRuntimeManager").AddComponent<GDKGameRuntime>();
            if (!Initialized)
            {
                return false;
            }
        }

        OnScreenLog.Add($"GDK XGameRuntime Library initialized: {Initialized}");
        OnScreenLog.Add($"GDK Xbox Live API SCID: {GameConfigScid}");
        OnScreenLog.Add($"GDK TitleId: {GameConfigTitleId}");
        OnScreenLog.Add($"GDK Sandbox: {GameConfigSandbox}");
        return Initialized;
    }

    private void Awake()
    {
        InitializeOrDestroyInstance(this);
    }

    private async void Start()
    {
        // SDK.XTaskQueueDispatch(0) needs to be call to pump GDK events.
        // It is possible to call the function on the main thread (i.e. update method)
        // but it can create stuttering, since some GDK actions can block the thread.
        // The recommended approach is to run XTaskQueueDispatch on another thread.
        // But keep in mind that most Unity API are not thread safe and can cause crashes
        // and other problems when accessing Unity functions.
        m_CancellationTokenSource = new CancellationTokenSource();
        await DispatchTaskQueue(m_CancellationTokenSource.Token);
    }

    /// <summary>
    /// OnApplicationQuit/OnDestroy needs to handle cleanup especially if we have
    /// initialized the services from within the editor player
    /// </summary>
    private void OnApplicationQuit()
    {
        m_CancellationTokenSource.Cancel();
        m_CancellationTokenSource.Dispose();

        SDK.CloseDefaultXTaskQueue();
        OnScreenLog.Add("Uninitializing Xbox Live API.");
        SDK.XBL.XblCleanup(null);
        OnScreenLog.Add("Uninitializing XGame Runtime Library.");
        SDK.XGameRuntimeUninitialize();

        Initialized = false;
        Instance = null;
    }

    private static bool InitializeOrDestroyInstance(GDKGameRuntime newInstance)
    {
        if (Instance != newInstance && Instance != null)
        {
            OnScreenLog.Add($"An instance of {newInstance.GetType().Name} already exist. Destroying {newInstance}...");
            Destroy(newInstance.gameObject);
            return false;
        }

        Instance = newInstance;
        GameConfigTitleId = GameConfigTitleId;
        GameConfigScid = GameConfigScid;
        DontDestroyOnLoad(Instance.gameObject);
        return InitializeRuntime();
    }

    private static bool InitializeRuntime(bool forceInitialization = false)
    {
        if (HR.FAILED(InitializeGamingRuntime(forceInitialization)) ||
            !InitializeXboxLive(GameConfigScid))
        {
            Initialized = false;
            return false;
        }

        // Not necessary but handy to know when debugging
        int hResult = SDK.XGameGetXboxTitleId(out var titleId);
        if (HR.FAILED(hResult))
        {
            OnScreenLog.Add($"FAILED: Could not get TitleID! hResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
        }

        if (titleId.ToString("X").ToLower().Equals(GameConfigTitleId.ToLower()) == false)
        {
            OnScreenLog.Add($"WARNING! Expected Title Id: {GameConfigTitleId} got: {titleId:X}");
        }

        //GameConfigTitleId = titleId.ToString("X");

        hResult = SDK.XSystemGetXboxLiveSandboxId(out var sandboxId);
        if (HR.FAILED(hResult))
        {
            OnScreenLog.Add($"FAILED: Could not get SandboxID! HResult: 0x{hResult:x} ({HR.NameOf(hResult)})");
        }

        if (sandboxId.Equals(GameConfigSandbox) == false)
        {
            OnScreenLog.Add($"WARNING! Expected sandbox Id: {GameConfigSandbox} got: {sandboxId}");
        }

        GameConfigSandbox = sandboxId;

        OnScreenLog.Add($"GDK Initialized, titleId: {GameConfigTitleId}, sandboxId: {GameConfigSandbox}");

        // Done!
        Initialized = true;
        return true;
    }

    /// <summary>
    /// Initializes the main Runtime Library.
    /// At the same time we will Creates the Async Dispatch thread which will handle all calls to work.
    /// </summary>
    private static int InitializeGamingRuntime(bool forceInitialization = false)
    {
        // We do not want stack traces for all log statements. (Exceptions logged
        // with OnScreenLog.AddException will still have stack traces though.):
        //Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        OnScreenLog.Add("Initializing XGame Runtime Library.");

        if (Initialized && forceInitialization == false)
        {
            OnScreenLog.Add("Gaming Runtime already initialized.");
            return 0;
        }

        int hResult = SDK.XGameRuntimeInitialize();
        if (HR.FAILED(hResult))
        {
            OnScreenLog.Add($"FAILED: Initialize XGameRuntime, HResult: 0x{hResult:X} ({HR.NameOf(hResult)})");
            return hResult;
        }

        hResult = SDK.CreateDefaultTaskQueue();
        if (HR.FAILED(hResult))
        {
            OnScreenLog.AddError($"FAILED: XTaskQueueCreate, HResult: 0x{hResult:X}");
            return hResult;
        }

        return 0;
    }

    /// <summary>
    /// Initializes the Xbox Live Basic API this is required for all Xbox Live API calls.
    /// </summary>
    /// <returns>The HResult value of initializing Xbox Live</returns>
    private static bool InitializeXboxLive(string scid)
    {
        OnScreenLog.Add($"Initializing Xbox Live API (SCID: {scid}).");

        int hResult = SDK.XBL.XblInitialize(scid);
        if (HR.FAILED(hResult) && hResult != HR.E_XBL_ALREADY_INITIALIZED)
        {
            OnScreenLog.Add($"FAILED: Initialize Xbox Live, HResult: 0x{hResult:X}, {HR.NameOf(hResult)}");
            return false;
        }

        return true;
    }

#if UNITY_6000_0_OR_NEWER
    /// <summary>
    /// Async Dispatch Task Queue using Awaitable - Only available starting Unity 6.
    /// This executes all GDK Asynchronous block work natively.
    /// </summary>
    private static async Awaitable DispatchTaskQueue(CancellationToken cancellationToken)
    {
        float fixedDeltaTime = Time.fixedDeltaTime;

        while (!cancellationToken.IsCancellationRequested)
        {
            SDK.XTaskQueueDispatch(0);

            // Awaitable API throws when cancelation is requested.
            try
            {
                await Awaitable.WaitForSecondsAsync(fixedDeltaTime, cancellationToken);
            }
            catch
            {
                OnScreenLog.Add("Closing default XTaskQueue.");
                SDK.CloseDefaultXTaskQueue();
            }
        }
    }
#else
        /// <summary>
        /// Async Dispatch Task Queue using Task.
        /// This executes all GDK Asynchronous block work natively.
        /// </summary>
        private static async Task DispatchTaskQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                SDK.XTaskQueueDispatch(0);
                await Task.Delay(32, token);
            }

            OnScreenLog.Add("Closing default XTaskQueue.");
            SDK.CloseDefaultXTaskQueue();
        }
#endif
}
#endif