using UnityEngine;

#if UNITY_GAMECORE
using Unity.XGamingRuntime;
#endif

public class XboxOneUtils : Singleton<XboxOneUtils>
{
    public void DisplayUsersInfo()
    {
#if UNITY_GAMECORE
        //        UserManager.RefreshUsersList();
        //#if DEBUG
        //                DebugManager.log("Users:");
        //                int index = 0;
        //                foreach (Users.User u in Users.UsersManager.Users)
        //                {
        //                    DebugManager.log(" " + index + ". " + u.Id + " - " + u.ApplicationDisplayName + " - " + u.UID);
        //                    if(u.pairedControllerIds.Count > 0)
        //                    {
        //                        DebugManager.log("       " + GetControllers(u.pairedControllerIds.ToArray()) + ", " + u.UID);
        //                    }
        //                    index++;
        //                }

        //                DebugManager.log(" ");
        //#endif
#endif
    }

    public string GetControllers(ulong[] list)
    {
        string s = "";
        foreach (ulong u in list)
        {
            if (s.Length > 0)
            {
                s += ", ";
            }
            s += u;
        }
        return s;
    }
}
