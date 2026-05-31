#if UNITY_GAMECORE
using System;
using Unity.XGamingRuntime;
using UnityEngine.Events;

/// <summary>
/// A very very simple initialization system to get a basic user silently
/// (i.e. it assume you arleady have auser loggedin)
///
/// This is not a good complete user intialization system and will not
/// take into account all XRs possiblities .
///
/// We recommend looking on the UserEngagement-SinglePlayer sample
/// for a better sample with more useful data and XR compliant.
/// </summary>

public class UserManager
{
    public static XblContextHandle CurrentUserXblContextHandle => s_CachedXblContextHandle;
    public static XUserHandle CurrentUserHandle => s_CachedUserHandle;
    public static ulong CurrentUserXuid => s_CachedXuid;
    public static string CurrentUserGamerTag => s_CachedGamerTag;

    private static XUserHandle s_CachedUserHandle;
    private static XblContextHandle s_CachedXblContextHandle;
    private static XUserChangeRegistrationToken s_CachedRegistrationToken;
    private static ulong s_CachedXuid;
    private static string s_CachedGamerTag;

    private static XUserLocalId s_CachedLocalId;

    private static UnityAction s_OnUserAdded;
    private static UnityAction s_OnUserLoggedOut;
    private static UnityAction s_OnUserError;

    static UserManager()
    {
        // Register for the user change event with the GDK
        SDK.XUserRegisterForChangeEvent(OnUserEventChanged, out s_CachedRegistrationToken);
    }

    /// <summary>
    /// A quick and dirty method to initialize any user
    /// </summary>
    public static void InitializeAndAddUser(UnityAction onUserAdded, UnityAction onUserLoggedOut, UnityAction _onUserError)
    {
        // ensure there is only one callback
        s_OnUserAdded = null;
        s_OnUserLoggedOut = null;
        s_OnUserError = null;

        s_OnUserAdded = onUserAdded;
        s_OnUserLoggedOut = onUserLoggedOut;
        s_OnUserError = _onUserError;

        SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, OnAddUser);
    }

    /// <summary>
    /// Close all handles and unregister for user change event.
    /// </summary>
    public static void Uninitialize()
    {
        if (s_CachedXblContextHandle != null)
        {
            SDK.XBL.XblContextCloseHandle(s_CachedXblContextHandle);
            s_CachedXblContextHandle = null;
        }

        if (s_CachedUserHandle != null)
        {
            SDK.XUserCloseHandle(s_CachedUserHandle);
            s_CachedUserHandle = null;
        }

        SDK.XUserUnregisterForChangeEvent(s_CachedRegistrationToken);
    }

    private static void OnUserEventChanged(IntPtr _, XUserLocalId userLocalId, XUserChangeEvent eventType)
    {
        if (eventType == XUserChangeEvent.SigningOut && userLocalId.Value == s_CachedLocalId.Value)
        {
            OnScreenLog.Add("User logging out");
            // Close all handles
            SDK.XBL.XblContextCloseHandle(CurrentUserXblContextHandle);
            SDK.XUserCloseHandle(s_CachedUserHandle);

            s_OnUserLoggedOut?.Invoke();
        }
        else if (eventType == XUserChangeEvent.SignedInAgain && userLocalId.Value == s_CachedLocalId.Value)
        {
            int hresult = SDK.XUserFindUserByLocalId(userLocalId, out s_CachedUserHandle);
            if (HR.FAILED(hresult))
            {
                OnScreenLog.Add($"Error signing back a user. HRESULT: 0x{hresult:X}");
                return;
            }
            OnAddUser(hresult, s_CachedUserHandle);
        }
    }

    private static void OnAddUser(int hresult, XUserHandle userHandle)
    {
        if (HR.FAILED(hresult))
        {
            OnScreenLog.Add($"Error adding a user. HRESULT: 0x{hresult:X}");
            s_OnUserError?.Invoke();
            return;
        }

        s_CachedUserHandle = userHandle;

        // Get local user id, XUID, GamerTag and XBL context handle
        hresult = SDK.XUserGetLocalId(s_CachedUserHandle, out s_CachedLocalId);
        if (HR.FAILED(hresult))
        {
            OnScreenLog.Add($"Error getting the XUserLocalID. HRESULT: 0x{hresult:X}");
            s_OnUserError?.Invoke();
            return;
        }

        hresult = SDK.XUserGetId(s_CachedUserHandle, out s_CachedXuid);
        if (HR.FAILED(hresult))
        {
            OnScreenLog.Add($"Error getting the Xuid. HRESULT: 0x{hresult:X}");
            s_OnUserError?.Invoke();
            return;
        }

        hresult = SDK.XUserGetGamertag(s_CachedUserHandle, XUserGamertagComponent.Classic, out s_CachedGamerTag);
        if (HR.FAILED(hresult))
        {
            OnScreenLog.Add($"Error getting the Gamertag. HRESULT: 0x{hresult:X}");
            s_OnUserError?.Invoke();
            return;
        }

        hresult = SDK.XBL.XblContextCreateHandle(s_CachedUserHandle, out s_CachedXblContextHandle);
        if (HR.FAILED(hresult))
        {
            OnScreenLog.Add($"Error getting the XblContextHandle. HRESULT: 0x{hresult:X}");
            s_OnUserError?.Invoke();
            return;
        }

        OnScreenLog.Add($"OnAddUserComplete finished. XUserLocalID: {s_CachedLocalId.Value}. Xuid: {CurrentUserXuid}");
        s_OnUserAdded?.Invoke();
    }
}
#endif