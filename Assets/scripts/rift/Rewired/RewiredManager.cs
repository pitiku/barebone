using Rewired;
using System.Collections.Generic;
using UnityEngine;

public class RewiredManager : SceneSingleton<RewiredManager>
{
    #region Declarations
    public const int kMaxPlayers = 8;
    private const int kNoPlayer = -1;
    public const string kAccept = "UISubmit";
    public const string kAcceptMouse = "UISubmitWithMouse";
    public const string kCancel = "UICancel";
    public const string kStart = "UIStart";
    public const string kRandom = "UIRandom";
    public const string kDelete = "UIDelete";
    public const string kInfo = "UIInfo";

    public const string kUILeft = "UILeft";
    public const string kUIRight = "UIRight";
    public const string kUIUp = "UIUp";
    public const string kUIDown = "UIDown";

    public const string kAction1 = "button_down";
    public const string kAction2 = "button_right";
    public const string kAction3 = "button_left";
    public const string kAction4 = "button_up";
    public const string kLB = "LB";
    public const string kRB = "RB";
    public const string kLT = "LT";
    public const string kRT = "RT";
    public const string kR3 = "UIR3";
    public const string kL3 = "UIL3";

    public const string kLeftHorizontal = "stickLeftHorizontal";
    public const string kLeftVertical = "stickLeftVertical";
    public const string kRightHorizontal = "stickRightHorizontal";
    public const string kRightVertical = "stickRightVertical";

    public const string kEvent_PlayerJoined = "PlayerJoined";
    public const string kEvent_PlayerLeft = "PlayerLeft";

    //PLAYERS
    public int[] m_playingPlayers = new int[kMaxPlayers];
    public int[] m_playingControllers = new int[kMaxPlayers];
    public bool m_engaging;

    public bool m_allowPlayerJoining = false;
    private int m_lastJoinedPlayerIndex;

    public Rewired.Integration.UnityUI.RewiredStandaloneInputModule m_rewiredInputModule;

    private int m_maxPlayers = kMaxPlayers;

    public bool m_requirePlayers = false;
    public List<int> m_requiredPlayers = new List<int>();

    // OTHER
    public float m_fDirectionThreshold = 0.3f;
    public bool m_bVibrationEnabled = true;

    private bool m_reconnectUsingControlledId = true;
    #endregion

    #region init/destroy
    public void initialize()
    {
        //Reset Players
        for (int playerIndex = 0; playerIndex < kMaxPlayers; ++playerIndex)
        {
            m_playingPlayers[playerIndex] = kNoPlayer;
            m_playingControllers[playerIndex] = kNoPlayer;
        }

        InitializeTimers();

        ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
        ReInput.ControllerConnectedEvent += OnControllerConnected;

#if UNITY_SWITCH
        m_reconnectUsingControlledId = true;
#else
        m_reconnectUsingControlledId = false;
#endif

        PlayerJoins(0, 0);
        PlayerJoins(1, 8);
        PlayerJoins(2, 9);


        //DISABLE JOYSTICKS
        // Disable auto-assignment

#if !CONTROLLER
        ReInput.configuration.autoAssignJoysticks = false;

        for (int iIndex = 0; iIndex < ReInput.players.playerCount; ++iIndex)
        {
            Player oPlayer = ReInput.players.GetPlayer(iIndex);
            foreach (var item in oPlayer.controllers.Joysticks)
            {
                // Disable all joystick maps for a player  
                foreach (var map in oPlayer.controllers.maps.GetMaps(ControllerType.Joystick, item.id))
                {
                    map.enabled = false;
                }
            }
            // Unassign all joysticks from a player  
            oPlayer.controllers.ClearControllersOfType(ControllerType.Joystick);
        }

        // Disable the joystick globally (optional)  
        foreach (Joystick j in ReInput.controllers.Joysticks)
        {
            j.enabled = false;
        }
#endif

        LogAllControllers();
    }

    private void LogAllControllers()
    {
        // Joysticks
        foreach (var joystick in ReInput.controllers.Joysticks)
        {
            Deb.log($"[Rewired] Joystick id={joystick.id} name={joystick.name}");
        }

        // Keyboard (Rewired uses a single keyboard controller)
        var keyboard = ReInput.controllers.Keyboard;
        Deb.log($"[Rewired] Keyboard present={keyboard != null && keyboard.enabled}");

        // Mouse
        var mouse = ReInput.controllers.Mouse;
        Deb.log($"[Rewired] Mouse present={mouse != null && mouse.enabled}");

        // Per-player assignment overview
        foreach (var player in ReInput.players.GetPlayers())
        {
            var js = player.controllers.Joysticks;
            var kb = player.controllers.hasKeyboard;
            var ms = player.controllers.hasMouse;

            // Convert joystick list to IDs properly
            string joystickIds = "";
            for (int i = 0; i < js.Count; ++i)
            {
                if (i > 0) joystickIds += ",";
                joystickIds += js[i].id.ToString();
            }

            Deb.log($"[Rewired] Player {player.id}: joysticks=[{joystickIds}] keyboard={kb} mouse={ms}");
        }
    }

    private void OnDestroy()
    {
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
    }
    #endregion

    public int getID(string _sAction)
    {
        return ReInput.mapping.GetActionId(_sAction);
    }

    public void SetRewiredInputPlayers(int[] players)
    {
        m_rewiredInputModule.RewiredPlayerIds = players;
    }

    public void UpdateInputs()
    {
        UpdateLocks();

        UpdateActionTimers();

        CheckNewJoins();

        updateRift();
    }

    #region PLAYERS
    public void Reset()
    {
        ResetPlayers(true);
    }

    public void ResetPlayers(bool resetMenuPlayer = false)
    {
        for (int playerIndex = resetMenuPlayer ? 0 : 1; playerIndex < kMaxPlayers; ++playerIndex)
        {
            PlayerLeaves(playerIndex);
        }
        m_requiredPlayers.Clear();
    }

    public void RequirePlayingPlayers(bool value)
    {
        m_requirePlayers = value;
        if (!value)
        {
            m_requiredPlayers.Clear();
        }
    }

    public void AllowPlayerJoining(bool value)
    {
        m_allowPlayerJoining = value;
    }

    public void PlayerJoiningAllowed()
    {
        AllowPlayerJoining(true);
    }

    public void PlayerJoiningNotAllowed()
    {
        AllowPlayerJoining(false);
    }

    public void StartEngagement()
    {
        ResetPlayers(true);
        m_engaging = true;
        AllowPlayerJoining(true);
    }

    private void CheckNewJoins()
    {
        if (m_locked)
        {
            return;
        }

        //Check Required players
        if (m_requirePlayers && m_requiredPlayers.Count > 0)
        {
            for (int rewiredIndex = 0; rewiredIndex < ReInput.players.playerCount; ++rewiredIndex)
            {
                if (GetAnyButtonButStartDown(rewiredIndex))
                {
                    if (System.Array.IndexOf(m_playingPlayers, rewiredIndex) == -1)
                    {
                        PlayerJoins(m_requiredPlayers[0], rewiredIndex);
                        m_requiredPlayers.RemoveAt(0);
                        //Only one join per frame
                        break;
                    }
                }
            }
        }
        //Normal joining
        else if (m_allowPlayerJoining || !HasMenuPlayer())
        {
            for (int rewiredIndex = 0; rewiredIndex < ReInput.players.playerCount; ++rewiredIndex)
            {
                if (GetJoinButtonDown(rewiredIndex))
                {
                    if (GetPlayingPlayersCount() < m_maxPlayers && (!IsPlayerPlaying(rewiredIndex) || !HasMenuPlayer()))
                    {
                        bool success = true;
#if UNITY_XBOXONE && !UNITY_EDITOR
                        if(m_engaging)
                        {
                            success = false;

                            foreach (Joystick joystick in ReInput.players.GetPlayer(rewiredIndex).controllers.Joysticks)
                            {
                                Rewired.Platforms.XboxOne.XboxOneGamepadExtension extension = joystick.GetExtension<Rewired.Platforms.XboxOne.XboxOneGamepadExtension>();
                                if (extension != null)
                                {
                                    if(XboxOneManager.Instance.Engage(extension.xboxOneJoystickId))
                                    {
                                        success = true;
                                        m_engaging = false;
                                        m_allowPlayerJoining = false;
                                        break;
                                    }
                                }
                            }
                        }
#endif
                        if (success)
                        {
                            if (IsPlayerPlaying(rewiredIndex))
                            {
                                //Remove the new menu player from his previous position
                                int menuPlayerPreviousIndex = System.Array.IndexOf(m_playingPlayers, rewiredIndex);
                                PlayerLeaves(menuPlayerPreviousIndex);

                                //Shift the players to fill the new menu player place
                                for (int playerIndex = menuPlayerPreviousIndex + 1; playerIndex < kMaxPlayers; ++playerIndex)
                                {
                                    int movingRewiredIndex = m_playingPlayers[playerIndex];
                                    if (movingRewiredIndex != kNoPlayer)
                                    {
                                        PlayerLeaves(playerIndex);
                                        PlayerJoins(movingRewiredIndex);
                                    }
                                }
                            }

                            PlayerJoins(rewiredIndex);
                            //Only one join per frame
                            break;
                        }
                    }
                }
            }
        }
    }

    public bool IsPlayerPlaying(int rewiredIndex)
    {
        return System.Array.IndexOf(m_playingPlayers, rewiredIndex) > -1;
    }

    public bool HasMenuPlayer()
    {
        return m_playingPlayers[0] != kNoPlayer;
    }

    public bool MissingRequiredPlayers()
    {
        return m_requirePlayers && m_requiredPlayers.Count > 0;
    }

    public void SetMaxPlayers(int maxPlayers = kMaxPlayers)
    {
        m_maxPlayers = maxPlayers;
    }

    public Player GetPlayer(int playerIndex)
    {
        return ReInput.players.GetPlayer(GetRewiredIndex(playerIndex));
    }

    private int GetFirstAvailablePlayerSlot()
    {
        for (int slot = 0; slot < m_playingPlayers.Length; ++slot)
        {
            if (m_playingPlayers[slot] == kNoPlayer)
            {
                return slot;
            }
        }
        return -1;
    }

    private void PlayerJoins(int slot, int rewiredIndex)
    {
        m_playingPlayers[slot] = rewiredIndex;
        if (ReInput.players.GetPlayer(rewiredIndex).controllers.joystickCount > 0)
        {
            m_playingControllers[slot] = ReInput.players.GetPlayer(rewiredIndex).controllers.Joysticks[0].id;
        }
        m_lastJoinedPlayerIndex = slot;
        EventManager.Instance.TriggerEvent(kEvent_PlayerJoined);

        //Menu player
        if (slot == 0)
        {
            SetRewiredInputPlayers(new[] { rewiredIndex });
        }
    }

    public int PlayerJoinsDebug(int rewiredIndex)
    {
        return PlayerJoins(rewiredIndex);
    }

    private int PlayerJoins(int rewiredIndex)
    {
        int availableSlot = GetFirstAvailablePlayerSlot();
        PlayerJoins(availableSlot, rewiredIndex);
        return availableSlot;
    }

    public void PlayerLeaves(int playerIndex)
    {
        if (m_playingPlayers[playerIndex] != kNoPlayer)
        {
            //Deb.log("PlayerLeaves: " + playerIndex);
            m_playingPlayers[playerIndex] = kNoPlayer;
            m_playingControllers[playerIndex] = kNoPlayer;
            EventManager.Instance.TriggerEvent(kEvent_PlayerLeft);

            //Menu player
            if (playerIndex == 0)
            {
                SetRewiredInputPlayers(new int[] { });
            }
        }
    }

    private int GetPlayingPlayersCount()
    {
        int count = 0;
        for (int index = 0; index < kMaxPlayers; ++index)
        {
            count += m_playingPlayers[index] == kNoPlayer ? 0 : 1;
        }
        return count;
    }

    public bool IsPlayerJoined(int _iPlayerIndex)
    {
        return m_playingPlayers[_iPlayerIndex] != kNoPlayer;
    }

    public bool IsRewiredIndexJoined(int _iRewiredIndex)
    {
        for (int index = 0; index < m_playingPlayers.Length; ++index)
        {
            if (m_playingPlayers[index] == _iRewiredIndex)
            {
                return true;
            }
        }
        return false;
    }

    private int GetRewiredIndex(int playerIndex)
    {
        return m_playingPlayers[playerIndex];
    }

    public int GetPlayerCount()
    {
        return GetPlayingPlayersCount();
    }

    public int GetJoiningPlayerIndex()
    {
        return m_lastJoinedPlayerIndex;
    }

    #endregion

    #region LOCK STUFF

    //LOCKS
    private bool m_locked = false;
    private float m_lastLockedTimestamp;
    private float m_lockDuration = 0.5f;

    private bool m_anyPlayerInputLocked = false;
    private int[] m_playerInputLockedFrames = new int[kMaxPlayers];
    private bool[] m_playerInputLocked = new bool[kMaxPlayers];

    public void LockInputs(float _fTime = 0)
    {
        m_locked = true;
        m_lockDuration = Mathf.Max(_fTime, m_lastLockedTimestamp + m_lockDuration - Time.time);
        m_lastLockedTimestamp = Time.time;
    }

    public void LockInputsLong()
    {
        LockInputs(float.MaxValue);
    }

    public void UnlockInputs()
    {
        m_locked = false;
        m_lastLockedTimestamp = 0;
        m_lockDuration = 0;
    }

    public void LockPlayerInputs(int _iFrames, int _iPlayerIndex = -1, bool _bLockImmediately = true)
    {
        m_anyPlayerInputLocked = true;
        if (_iPlayerIndex == -1) // lock all indexes
        {
            for (int i = 0; i < kMaxPlayers; ++i)
            {
                m_playerInputLocked[i] = _bLockImmediately;
                m_playerInputLockedFrames[i] = _iFrames;
            }
        }
        else
        {
            m_playerInputLocked[_iPlayerIndex] = _bLockImmediately;
            m_playerInputLockedFrames[_iPlayerIndex] = _iFrames;
        }
    }

    private void UpdateLocks()
    {
        m_locked = m_lastLockedTimestamp + m_lockDuration > Time.time;

        if (m_anyPlayerInputLocked)
        {
            m_anyPlayerInputLocked = false;
            for (int i = 0; i < kMaxPlayers; ++i)
            {
                m_playerInputLocked[i] = --m_playerInputLockedFrames[i] > 0;
                m_anyPlayerInputLocked = m_playerInputLocked[i];
            }
        }
    }

    #endregion

    #region INPUT HELPERS

    bool anyMouseUsed()
    {
        return anyMouseButtonPressed() || Rewired.ReInput.controllers.Mouse.GetAxis(0).abs() > 0.01f;
    }

    bool anyMouseButtonPressed()
    {
        return Rewired.ReInput.controllers.Mouse.GetAnyButtonDown();
    }

    bool anyJoystickUsed()
    {
        for (int iIndex = 0; iIndex < 8; ++iIndex)
        {
            if (ReInput.players.GetPlayer(iIndex).GetAnyButtonDown())
            {
                return true;
            }
        }

        return false;
    }

    public bool anyKeyPressed()
    {
        return Rewired.ReInput.controllers.GetAnyButtonDown();
    }

    public bool anyKeyHeld()
    {
        return Rewired.ReInput.controllers.GetAnyButton();
    }


    private bool GetJoinButtonDown(int _iRewiredIndex)
    {
        if (HasMenuPlayer())
        {
            return ReInput.players.GetPlayer(_iRewiredIndex).GetButtonDown(kAccept)
                || ReInput.players.GetPlayer(_iRewiredIndex).GetButtonDown(kStart);
        }
        else
        {
            return ReInput.players.GetPlayer(_iRewiredIndex).GetAnyButtonDown();
        }
    }

    private bool GetAnyButtonButStartDown(int _iRewiredIndex)
    {
        return ReInput.players.GetPlayer(_iRewiredIndex).GetAnyButtonDown() && !ReInput.players.GetPlayer(_iRewiredIndex).GetButtonDown(kStart);
    }

    private bool CheckPlayer(int playerIndex, bool _bIgnoreLock = false)
    {
        return (_bIgnoreLock || !m_locked) && playerIndex != -1 && !m_playerInputLocked[playerIndex] && GetRewiredIndex(playerIndex) != kNoPlayer;
    }

    public bool GetActionTimedPressDown(int playerIndex, string action, float _fTime)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonTimedPressDown(action, _fTime);
    }

    public bool GetActionDown(int playerIndex, int _iAction)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonDown(_iAction);
    }
    public bool GetActionDown(int playerIndex, string action)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonDown(action);
    }

    public bool GetAction(int playerIndex, string action)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButton(action);
    }
    public bool GetAction(int playerIndex, int _iAction)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButton(_iAction);
    }

    public bool GetActionUp(int playerIndex, string action)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonUp(action);
    }
    public bool GetActionUp(int playerIndex, int _iAction)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonUp(_iAction);
    }

    public bool GetActionRepeating(int playerIndex, string action)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonRepeating(action);
    }
    public bool GetActionRepeating(int playerIndex, int _iAction)
    {
        return CheckPlayer(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonRepeating(_iAction);
    }

    public bool GetActionDownAnyPlayer(string action, bool _bLockInputs = false)
    {
#if UNITY_EDITOR
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= UnityEditor.Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= UnityEditor.Handles.GetMainGameViewSize().y - 1) return false;
#elif UNITY_STANDALONE
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) return false;
#endif
        for (int playerIndex = 0; playerIndex < m_playingPlayers.Length; ++playerIndex)
        {
            if (GetActionDown(playerIndex, action))
            {
                if (_bLockInputs)
                {
                    LockInputs();
                }

                return true;
            }
        }
        return false;
    }

    public bool GetActionDownAnyPlayerIgnoringLock(string action)
    {
#if UNITY_EDITOR
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= UnityEditor.Handles.GetMainGameViewSize().x - 1 || Input.mousePosition.y >= UnityEditor.Handles.GetMainGameViewSize().y - 1) return false;
#elif UNITY_STANDALONE
        if (Input.mousePosition.x == 0 || Input.mousePosition.y == 0 || Input.mousePosition.x >= Screen.width - 1 || Input.mousePosition.y >= Screen.height - 1) return false;
#endif
        for (int playerIndex = 0; playerIndex < m_playingPlayers.Length; ++playerIndex)
        {
            if (CheckPlayer(playerIndex, true) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).GetButtonDown(action))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetActionAnyPlayer(string action)
    {
        for (int playerIndex = 0; playerIndex < m_playingPlayers.Length; ++playerIndex)
        {
            if (GetAction(playerIndex, action))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetActionUpAnyPlayer(string action)
    {
        for (int playerIndex = 0; playerIndex < m_playingPlayers.Length; ++playerIndex)
        {
            if (GetActionUp(playerIndex, action))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetActionRepeatingAnyPlayer(string action)
    {
        for (int playerIndex = 0; playerIndex < m_playingPlayers.Length; ++playerIndex)
        {
            if (GetActionRepeating(playerIndex, action))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetActionDownAnyController(string action)
    {
        for (int rewiredIndex = 0; rewiredIndex < ReInput.players.playerCount; ++rewiredIndex)
        {
            if (ReInput.players.GetPlayer(rewiredIndex).GetButtonDown(action))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetMenuPlayerActionDown(string action)
    {
        return GetActionDown(0, action);
    }

    public bool GetMenuPlayerAction(string action)
    {
        return GetAction(0, action);
    }

    public bool GetMenuPlayerActionUp(string action)
    {
        return GetActionUp(0, action);
    }

    public bool GetMenuPlayerActionRepeating(string action)
    {
        return GetActionRepeating(0, action);
    }

    #endregion

    #region STICKS

    public Vector2 GetStickV2Rewired(int _iRewiredIndex, string _sH, string _sV)
    {
        Player player = ReInput.players.GetPlayer(_iRewiredIndex);
        Vector2 vResult = new Vector2(player.GetAxis(_sH), player.GetAxis(_sV));
        if (vResult.sqrMagnitude > 0)
        {
            if (vResult.sqrMagnitude > 1.0f)
            {
                vResult.Normalize();
            }
        }

        return vResult;
    }

    public Vector2 GetStickV2(int _iPlayer, string _sH, string _sV)
    {
        if (!CheckPlayer(_iPlayer))
        {
            return Vector2.zero;
        }

        return GetStickV2Rewired(GetRewiredIndex(_iPlayer), _sH, _sV);
    }

    public Vector2 GetLeftStickV2(int _iPlayer)
    {
        return GetStickV2(_iPlayer, kLeftHorizontal, kLeftVertical);
    }

    public Vector2 GetRightStickV2(int _iPlayer)
    {
        return GetStickV2(_iPlayer, kRightHorizontal, kRightVertical);
    }


    public Vector3 GetStickV3(int _iPlayer, string _sHorizontal, string _sVertical)
    {
        if (!CheckPlayer(_iPlayer))
        {
            return Vector3.zero;
        }

        Player player = ReInput.players.GetPlayer(GetRewiredIndex(_iPlayer));
        Vector3 vResult = new Vector3(player.GetAxis(_sHorizontal), 0.0f, player.GetAxis(_sVertical));
        if (vResult.sqrMagnitude > 1.0f)
        {
            vResult.Normalize();
        }

        return vResult;
    }

    public Vector3 GetLeftStickV3(int _iPlayer)
    {
        return GetStickV3(_iPlayer, kLeftHorizontal, kLeftVertical);
    }

    public Vector3 GetRightStickV3(int _iPlayer)
    {
        return GetStickV3(_iPlayer, kRightHorizontal, kRightVertical);
    }

    public bool IsPressingDown(int _iPlayer)
    {
        return GetLeftStickV2(_iPlayer).y < -m_fDirectionThreshold;
    }

    public bool IsPressingUp(int _iPlayer)
    {
        return GetLeftStickV2(_iPlayer).y > m_fDirectionThreshold;
    }

    // Any stick stuff
    public Vector2 GetAnyStickV2(bool _bRightOrLeft, bool _bIncludeDPad)
    {
        for (int iIndex = 0; iIndex < 8; ++iIndex)
        {
            Vector2 vStick = GetStickV2Rewired(iIndex, _bRightOrLeft ? kRightHorizontal : kLeftHorizontal, _bRightOrLeft ? kRightVertical : kLeftVertical);
            if (_bIncludeDPad)
            {
                vStick += getDPADVector();
                if (vStick.magnitude > 1)
                {
                    vStick.Normalize();
                }
            }
            // stick has moved
            if (vStick.magnitude > 0.01f)
            {
                return vStick;
            }
        }

        return Vector2.zero;
    }

    public Vector2 GetAnyStickLeftV2(bool _bIncludeDPad)
    {
        return GetAnyStickV2(false, _bIncludeDPad);
    }

    public Vector2 GetAnyStickRightV2(bool _bIncludeDPad)
    {
        return GetAnyStickV2(true, _bIncludeDPad);
    }

    public Vector2 getDPADVector()
    {
        Vector2 _oV = Vector2.zero;
        if (GetActionDownAnyPlayer("d_padLeft"))
        {
            _oV.x = -1;
        }
        if (GetActionDownAnyPlayer("d_padRight"))
        {
            _oV.x = 1;
        }
        if (GetActionDownAnyPlayer("d_padUp"))
        {
            _oV.y = 1;
        }
        if (GetActionDownAnyPlayer("d_padDown"))
        {
            _oV.y = -1;
        }
        return _oV;
    }

    public Vector2 getControllerCursorMovement(float _fLAmount, float _fRAmount)
    {
        Vector2 oVL = GetAnyStickLeftV2(true);
        Vector2 oVR = GetAnyStickRightV2(true);

        return (oVL * _fLAmount) + (oVR * _fRAmount);
    }

    public bool anyStickJustPressed()
    {
        return leftStickJustPressed() || rightStickJustPressed();
    }
    public bool anyDPadJustPressed()
    {
        return getDPADVector() != Vector2.zero;
    }
    public bool anyStickJustReleased()
    {
        return leftStickJustReleased() || rightStickJustReleased();
    }

    public bool leftStickJustPressed()
    {
        return !m_bLeftStickPressedPrevious && m_bLeftStickPressed;
    }
    bool leftStickJustReleased()
    {
        return m_bLeftStickPressedPrevious && !m_bLeftStickPressed;
    }

    public bool rightStickJustPressed()
    {
        return !m_bRightStickPressedPrevious && m_bRightStickPressed;
    }
    bool rightStickJustReleased()
    {
        return m_bRightStickPressedPrevious && !m_bRightStickPressed;
    }
    #endregion

    #region ACTION TIMERS
    //TIMERS
    private Dictionary<int, float>[] m_aoActionDownTimers = new Dictionary<int, float>[kMaxPlayers];
    private List<int> m_aiActionTimersKeys = new List<int>();

    private void InitializeTimers()
    {
        for (int iPlayerIndex = 0; iPlayerIndex < kMaxPlayers; ++iPlayerIndex)
        {
            m_aoActionDownTimers[iPlayerIndex] = new Dictionary<int, float>();
        }
    }

    private void UpdateActionTimers()
    {
        // update last down times, to use in actions that require time windows (ie shooting)
        for (int playerIndex = 0; playerIndex < kMaxPlayers; ++playerIndex)
        {
            for (int i = 0; i < m_aiActionTimersKeys.Count; ++i)
            {
                int iKey = m_aiActionTimersKeys[i];
                m_aoActionDownTimers[playerIndex][iKey] += Time.deltaTime;
                if (GetActionDown(playerIndex, iKey))
                {
                    m_aoActionDownTimers[playerIndex][iKey] = 0.0f;
                }
            }
        }
    }

    public void resetActionTimers()
    {
        for (int playerIndex = 0; playerIndex < kMaxPlayers; ++playerIndex)
        {
            for (int i = 0; i < m_aiActionTimersKeys.Count; ++i)
            {
                int iKey = m_aiActionTimersKeys[i];
                m_aoActionDownTimers[playerIndex][iKey] = float.MaxValue;
            }
        }
    }

    public void AddActionTimer(int _iPlayerIndex, int _iAction)
    {
        m_aoActionDownTimers[_iPlayerIndex][_iAction] = 0.0f;
    }

    public bool GetActionDown(int playerIndex, int _iAction, float _fPreTime)
    {
        if (CheckPlayer(playerIndex))
        {
            return m_aoActionDownTimers[playerIndex][_iAction] < _fPreTime;
        }
        return false;
    }

    public bool AreInputsLocked { get { return m_locked; } }
    #endregion

    #region VIBRATION
    public void setVibration(int _iPlayerIndex, float _fIntensity0To1, float _fDuration, bool _bStopOtherMotors = false)
    {
        if (!m_bVibrationEnabled)
        {
            return;
        }

        Player oPlayer = ReInput.players.GetPlayer(GetRewiredIndex(_iPlayerIndex));
        if (oPlayer != null)
        {
            oPlayer.SetVibration(0, _fIntensity0To1 * PlatformManager.Current.getVibrationMultiplier(), _fDuration, _bStopOtherMotors);
        }
    }
    #endregion

    #region CONNECT/DISCONNECT

    public void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        CheckDisconnectedPlayers();
    }

    public void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        CheckReconnectedPlayers(args.controllerId);
    }

    public void CheckPlayersAndControllers()
    {
        CheckDisconnectedPlayers();

        CheckReconnectedPlayers();
    }

    private void CheckDisconnectedPlayers()
    {
        for (int playerIndex = 0; playerIndex < kMaxPlayers; ++playerIndex)
        {
            if (GetRewiredIndex(playerIndex) != kNoPlayer && !m_requiredPlayers.Contains(playerIndex) && ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).controllers.joystickCount <= 0 && !ReInput.players.GetPlayer(GetRewiredIndex(playerIndex)).controllers.hasKeyboard)
            {
                if (m_requirePlayers)
                {
                    m_requiredPlayers.Add(playerIndex);
                }
                else
                {
                    PlayerLeaves(playerIndex);
                }
            }
        }
    }

    private void CheckReconnectedPlayers(int controllerId = -1)
    {
        //This flag is used in switch only
        if (m_reconnectUsingControlledId)
        {
            bool found = false;
            for (int requiredIndex = 0; requiredIndex < m_requiredPlayers.Count; ++requiredIndex)
            {
                if (m_playingControllers[m_requiredPlayers[requiredIndex]] == controllerId)
                {
                    //Search the player with the controller we want
                    for (int rewiredPlayerIndex = 0; rewiredPlayerIndex < ReInput.players.playerCount; ++rewiredPlayerIndex)
                    {
                        if (ReInput.players.GetPlayers()[rewiredPlayerIndex].controllers.joystickCount > 0 && ReInput.players.GetPlayers()[rewiredPlayerIndex].controllers.Joysticks[0].id == controllerId)
                        {
                            if (GetRewiredIndex(m_requiredPlayers[requiredIndex]) != rewiredPlayerIndex)
                            {
                                //Swap the players
                                int one = System.Array.IndexOf(m_playingPlayers, rewiredPlayerIndex);
                                int two = System.Array.IndexOf(m_playingPlayers, GetRewiredIndex(m_requiredPlayers[requiredIndex]));
                                int aux = m_playingPlayers[one];
                                m_playingPlayers[one] = m_playingPlayers[two];
                                m_playingPlayers[two] = aux;
                                Deb.log("Reconnect: Players swapped P" + one + " and P" + two);
                            }
                            else
                            {
                                //Rewired made it ok
                                Deb.log("Reconnect: players OK");
                            }

                            m_requiredPlayers.RemoveAt(requiredIndex);
                            found = true;
                            break;
                        }
                    }
                    break;
                }
            }

            //If the controller is not one of the disconnected controllers, we try the normal reconnect (from handheld to docked f.e.)
            if (!found)
            {
                for (int requiredIndex = 0; requiredIndex < m_requiredPlayers.Count; ++requiredIndex)
                {
                    if (ReInput.players.GetPlayer(GetRewiredIndex(m_requiredPlayers[requiredIndex])).controllers.joystickCount > 0 || ReInput.players.GetPlayer(GetRewiredIndex(m_requiredPlayers[requiredIndex])).controllers.hasKeyboard)
                    {
                        m_requiredPlayers.RemoveAt(requiredIndex);
                        break;
                    }
                }
            }
        }
        else
        {
            for (int requiredIndex = 0; requiredIndex < m_requiredPlayers.Count; ++requiredIndex)
            {
                if (ReInput.players.GetPlayer(GetRewiredIndex(m_requiredPlayers[requiredIndex])).controllers.joystickCount > 0 || ReInput.players.GetPlayer(GetRewiredIndex(m_requiredPlayers[requiredIndex])).controllers.hasKeyboard)
                {
                    m_requiredPlayers.RemoveAt(requiredIndex);
                    break;
                }
            }
        }
    }

    #endregion

    #region DEBUG

#if DEBUG
    private bool m_debugEnabled = false;
    private string m_logText = "";
    private float m_logHeight;
    private GUIStyle m_logStyle = null;

    public void Update()
    {
        if (GetMenuPlayerAction("LB") && GetMenuPlayerAction("RB") && GetMenuPlayerActionDown("UICancel"))
        {
            m_debugEnabled = !m_debugEnabled;
        }
    }

    void OnGUI()
    {
        if (m_debugEnabled)
        {
            if (m_logStyle == null)
            {
                m_logStyle = GUI.skin.GetStyle("Label");
                m_logStyle.fontSize = 12;
                m_logStyle.alignment = TextAnchor.UpperLeft;
                m_logStyle.wordWrap = false;
            }

            m_logText = "";
            m_logHeight = 0;

            AddLogLine("Rewired Players: ");
            for (int index = 0; index < ReInput.players.playerCount; ++index)
            {
                if (ReInput.players.GetPlayers()[index].controllers.joystickCount > 0)
                {
                    AddLogLine("R" + index + ": C" + ReInput.players.GetPlayers()[index].controllers.Joysticks[0].id);
                }
                else
                {
                    AddLogLine("R" + index + ": -");
                }
            }

            AddLogLine("RewiredInputModule Players: ");
            for (int index = 0; index < m_rewiredInputModule.RewiredPlayerIds.Length; ++index)
            {
                AddLogLine("  R" + m_rewiredInputModule.RewiredPlayerIds[index].ToString());
            }

            AddLogLine("ReInput controllers: ");
            for (int index = 0; index < ReInput.controllers.GetControllerCount(ControllerType.Joystick); ++index)
            {
                AddLogLine("  C" + ReInput.controllers.GetControllers(ControllerType.Joystick)[index].id.ToString() + " - " + ReInput.controllers.GetControllers(ControllerType.Joystick)[index].name);
            }

            AddLogLine("Playing: ");
            for (int index = 0; index < m_playingPlayers.Length; ++index)
            {
                AddLogLine("  Player P" + index + ": R" + GetRewiredIndex(index) + ", C" + m_playingControllers[index]);
            }

            AddLogLine("Required: ");
            for (int index = 0; index < m_requiredPlayers.Count; ++index)
            {
                AddLogLine("  Required P" + m_requiredPlayers[index]);
            }
            AddLogLine("");
            AddLogLine("");

            GUI.color = Color.black;
            GUI.Label(new Rect(Screen.width - 199, 21, 200, m_logHeight), m_logText, m_logStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width - 200, 20, 200, m_logHeight), m_logText, m_logStyle);
        }
    }

    private void AddLogLine(string line)
    {
        m_logText += line + "\n";
        m_logHeight += m_logStyle.lineHeight * 1.1f;
    }
#endif
#endregion

    #region RIFT EXCLUSIVE

    bool m_bGamepadActive = false;
    public static bool USING_GAMEPAD => Instance != null && Instance.m_bGamepadActive;

    [Header("GAMEPAD")]
    public float m_fLSMovementAmountTeamScreen = 0.35f;
    public float m_fRSMovementAmountTeamScreen = 0.05f;

    public float m_fLSMovementAmountFreeCursor = 0.35f;
    public float m_fRSMovementAmountFreeCursor = 0.05f;

    public float m_fLSMovementAmountRotation = 5f;
    public float m_fRSMovementAmountRotation = 1f;

    bool m_bLeftStickPressed;
    bool m_bLeftStickPressedPrevious;
    bool m_bRightStickPressed;
    bool m_bRightStickPressedPrevious;
    float m_vStickPressedThreshold = 0.5f;
    float m_vStickUnpressedThreshold = 0.3f;

    void updateRift()
    {
        m_bLeftStickPressedPrevious = m_bLeftStickPressed;
        m_bRightStickPressedPrevious = m_bRightStickPressed;

        // cuando el joystick LEFT pasa del threshold de pressed se pone a true, pero si se ha moviod el joystick un poco que no haga doble click erróneo (comprueba si es mayor al threshold de unpressed y si en el anterior frame estaba a true)
        if (GetAnyStickLeftV2(false).magnitude > m_vStickPressedThreshold ||
            (m_bLeftStickPressedPrevious &&
            GetAnyStickLeftV2(false).magnitude > m_vStickUnpressedThreshold))
        {
            m_bLeftStickPressed = true;
        }
        else if (GetAnyStickLeftV2(false).magnitude < m_vStickUnpressedThreshold) // releasing
        {
            m_bLeftStickPressed = false;
        }
        // cuando el joystick RIGHT pasa del threshold de pressed se pone a true, pero si se ha moviod el joystick un poco que no haga doble click erróneo (comprueba si es mayor al threshold de unpressed y si en el anterior frame estaba a true)
        if (GetAnyStickRightV2(false).magnitude > m_vStickPressedThreshold ||
            (m_bRightStickPressedPrevious &&
            GetAnyStickRightV2(false).magnitude > m_vStickUnpressedThreshold))
        {
            m_bRightStickPressed = true;
        }
        else if (GetAnyStickRightV2(false).magnitude < m_vStickUnpressedThreshold) // releasing
        {
            m_bRightStickPressed = false;
        }
    }

    #endregion
}