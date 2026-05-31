using Sirenix.OdinInspector;
using StateChangeCollapse;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum eSaveType { Progression, Run }

public class Comparer
{
    public int m_iMinQuantity;
    public CompareFunction m_eComparison = CompareFunction.GreaterEqual;

    public bool checkFrom(int iActual)
    {
        return check(iActual, m_eComparison, m_iMinQuantity);
    }

    public static bool check(float _iValue1, CompareFunction _eComparison, float _iValue2)
    {
        switch (_eComparison)
        {
            case CompareFunction.GreaterEqual:
                return _iValue1 >= _iValue2;

            case CompareFunction.Greater:
                return _iValue1 > _iValue2;

            case CompareFunction.Less:
                return _iValue1 < _iValue2;

            case CompareFunction.LessEqual:
                return _iValue1 <= _iValue2;

            case CompareFunction.Equal:
                return _iValue1 == _iValue2;

            case CompareFunction.NotEqual:
                return _iValue1 != _iValue2;
        }
        return false;
    }
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    private TKey[] m_oKeys;

    [SerializeField]
    private TValue[] m_oValues;

    private Dictionary<TKey, TValue> m_dDictionary;

    // Constructor
    public SerializableDictionary()
    {
        m_dDictionary = new Dictionary<TKey, TValue>();
    }

    // Adding a key-value pair to the dictionary
    public void Add(TKey oKey, TValue oValue)
    {
        if (m_dDictionary.ContainsKey(oKey))
        {
            m_dDictionary[oKey] = oValue;
        }
        else
        {
            m_dDictionary.Add(oKey, oValue);
        }

        SyncArrays();
    }

    // Adding a key-value pair to the dictionary
    public bool Remove(TKey oKey)
    {
        if (m_dDictionary.ContainsKey(oKey))
        {
            m_dDictionary.Remove(oKey);
            SyncArrays();
            return true;
        }

        return false;
    }

    // Getting a value based on the key
    public TValue Get(TKey oKey)
    {
        return m_dDictionary[oKey];
    }

    public bool ContainsKey(TKey oKey)
    {
        return m_dDictionary.ContainsKey(oKey);
    }

    // ISerializationCallbackReceiver implementation
    public void OnBeforeSerialize()
    {
        // Synchronize dictionary with arrays before serialization
        SyncArrays();
    }

    public void OnAfterDeserialize()
    {
        // Rebuild dictionary after deserialization
        m_dDictionary = new Dictionary<TKey, TValue>();
        for (int i = 0; i < m_oKeys.Length; i++)
        {
            m_dDictionary[m_oKeys[i]] = m_oValues[i];
        }
    }

    // Synchronize the dictionary with the serialized arrays
    private void SyncArrays()
    {
        // Reallocate arrays if necessary
        int count = m_dDictionary.Count;
        if (m_oKeys == null || m_oKeys.Length != count)
        {
            m_oKeys = new TKey[count];
            m_oValues = new TValue[count];
        }

        // Copy key-value pairs from the dictionary to the arrays
        int iIndex = 0;
        foreach (var kvp in m_dDictionary)
        {
            m_oKeys[iIndex] = kvp.Key;
            m_oValues[iIndex] = kvp.Value;
            iIndex++;
        }
    }

    public TKey[] getKeys()
    {
        return m_oKeys;
    }
    public TValue[] getValues()
    {
        return m_oValues;
    }
}

[Serializable]
public class SerializableKeyValuePair<TKey, TValue>
{
    [TableList(ShowIndexLabels = false)][SerializeField] public TKey m_oKey;
    [TableList(ShowIndexLabels = false)][SerializeField] public TValue m_oValue;
}

[Serializable]
public class SerializableKvpCollection<TKey, TValue> : ISerializationCallbackReceiver, IDisposable
{

    [TableList(ShowIndexLabels = true)]

    [SerializeField]
    public List<SerializableKeyValuePair<TKey, TValue>> m_aoKeyValuePairs = new List<SerializableKeyValuePair<TKey, TValue>>();

    public Dictionary<TKey, TValue> m_aoDictionary = new Dictionary<TKey, TValue>();
    public Dictionary<TKey, TValue> Dictionary => m_aoDictionary;
    public bool ContainsKey(TKey _oKey) => m_aoDictionary.ContainsKey(_oKey);

    public bool TryGetValue(TKey _oKey, out TValue _oValue) => m_aoDictionary.TryGetValue(_oKey, out _oValue);
    public virtual TValue this[TKey _oKey]
    {
        get => m_aoDictionary[_oKey];
        set
        {
            if (!m_aoDictionary.TryAdd(_oKey, value))
            {
                m_aoDictionary[_oKey] = value;
                var pair = m_aoKeyValuePairs.Find(p => EqualityComparer<TKey>.Default.Equals(p.m_oKey, _oKey));
                if (pair != null)
                    pair.m_oValue = value;
            }
            else
            {
                m_aoDictionary[_oKey] = value;
                var pair = new SerializableKeyValuePair<TKey, TValue>();
                pair.m_oKey = _oKey;
                pair.m_oValue = value;
                m_aoKeyValuePairs.Add(pair);
            }
        }
    }

    public void Add(TKey _oKey, TValue _oValue)
    {
        if (!m_aoDictionary.TryAdd(_oKey, _oValue))
            return;

        var pair = new SerializableKeyValuePair<TKey, TValue>();
        pair.m_oKey = _oKey;
        pair.m_oValue = _oValue;
        m_aoKeyValuePairs.Add(pair);
    }

    public bool Remove(TKey _oKey)
    {
        if (!m_aoDictionary.TryGetValue(_oKey, out var value))
            return false;

        bool removed = m_aoDictionary.Remove(_oKey);

        int index = m_aoKeyValuePairs.FindIndex(p => EqualityComparer<TKey>.Default.Equals(p.m_oKey, _oKey));
        if (index >= 0)
        {
            m_aoKeyValuePairs.RemoveAt(index);
        }

        return removed;
    }

    public void Clear()
    {
        m_aoDictionary.Clear();
        m_aoKeyValuePairs.Clear();
    }

    public IEnumerable<TKey> Keys => m_aoDictionary.Keys;
    public IEnumerable<TValue> Values => m_aoDictionary.Values;

    public void OnBeforeSerialize()
    {
        // foreach (var kvp in m_aoDictionary)
        // {
        //     var pair = new SerializableKeyValuePair<TKey, TValue>();
        //     pair.m_oKey = kvp.Key;
        //     pair.m_oValue = kvp.Value;
        //     m_aoKeyValuePairs.Add(pair);
        // }
    }

    public virtual void OnAfterDeserialize()
    {
        m_aoDictionary.Clear();
        foreach (var kvp in m_aoKeyValuePairs)
        {
            if (!m_aoDictionary.ContainsKey(kvp.m_oKey))
            {
                m_aoDictionary.Add(kvp.m_oKey, kvp.m_oValue);
            }
            else
            {
                Debug.LogWarning($"Duplicate key: " + kvp.m_oKey + $" wont be added to Dictionary {typeof(TKey).Name} and {typeof(TValue).Name}");
            }

        }
    }
    public void Dispose()
    {
        // m_aoKeyValuePairs.Clear();
        // m_aoDictionary.Clear();
    }
}

/// GAME UTILS here are implemented all static general and helper methods related to this game that will be used a lot and don't have a singleton class to fit in
public static class GameUtils
{
    #region Declarations + initialize

    private static List<GameObject> m_aoTempGameObjects = new List<GameObject>();
    private static Collider[] m_oOverlappingColliders = new Collider[10];

    private static bool m_bInjectedAccept;
    private static bool m_bInjectedChangeHeight;
    private static bool m_bInjectedCancel;
    private static bool m_bHasInjectedCursorWorldPosition;
    private static bool m_bHasInjectedAttackDirection;
    private static Vector2 m_vInjectedCursorWorldPosition;

    public static void initialize()
    {
        clearInjectedInput();
    }

    #endregion

    #region Conditions


    public static bool isNullOrEmpty(this Condition _oCondition)
    {
        return _oCondition == null || _oCondition.GetType() == typeof(Condition);
    }

    public static bool areConditionsMet(this List<Condition> _aoConditions, bool _bAllConditions = true)
    {
        if (_aoConditions.isNullOrEmpty())
        {
            return true;
        }

        for (int i = 0; i < _aoConditions.Count; ++i)
        {
            if (_aoConditions[i].isMet())
            {
                if (!_bAllConditions)
                {
                    return true;
                }
            }
            else
            {
                if (_bAllConditions)
                {
                    return false;
                }
            }
        }

        //if we reach this point, it will mean all conditions are met (if _bAllConditions) or none are met (!_bAllConditions)
        return _bAllConditions;
    }

    #endregion CONDITIONS

    #region Rect Utils

    public static Rect m_rScreenRect = new Rect(0, 0, 1, 1);
    public static Rect m_rSafeZoneScreenRect = new Rect(0, 0, 0.9f, 0.8f);

    public static bool isPositionInCamera(this Vector3 _vPos, Camera _oCamera)
    {
        return m_rScreenRect.Contains(_oCamera.WorldToViewportPoint(_vPos));
    }

    public static bool isPositionV2InCamera(Vector2 _vPos, Camera _oCamera)
    {
        return isPositionInCamera(_vPos.toVector3xz(), _oCamera);
    }

    public static bool isPositionInCameraSafeZone(this Vector3 _vPos)
    {
        return m_rSafeZoneScreenRect.Contains(GameplayManager.Instance.m_oCombatCamera.WorldToViewportPoint(_vPos));
    }

    public static bool isPositionV2InCameraSafeZone(this Vector2 _vPos)
    {
        return isPositionInCameraSafeZone(_vPos.toVector3xz());
    }

    public static bool isTransformInCamera(this Transform _oTransform)
    {
        return m_rScreenRect.Contains(GameplayManager.Instance.m_oCombatCamera.WorldToViewportPoint(_oTransform.position));
    }

    public static Vector3 getVectorRotated(this Camera _oCamera, Vector3 _vV)
    {
        return _oCamera == GameplayManager.Instance.m_oCombatCamera ? _oCamera.transform.rotation * _vV : _vV;
    }

    public static Vector3 getPositionWorldFromCamera(this Camera _oCamera, bool _bTopOrBot, bool _bRightOrLeft, bool _bSafeZone = false)
    {
        Vector3 vOrigin;
        if (_bTopOrBot)
        {
            if (_bRightOrLeft)
            {
                vOrigin = _bSafeZone ? new Vector3(m_rSafeZoneScreenRect.width, m_rSafeZoneScreenRect.height, 1) : new Vector3(1, 1, 1);
            }
            else
            {
                vOrigin = _bSafeZone ? new Vector3(1 - m_rSafeZoneScreenRect.width, m_rSafeZoneScreenRect.height, 1) : new Vector3(0, 1, 1);
            }
        }
        else
        {
            if (_bRightOrLeft)
            {
                vOrigin = _bSafeZone ? new Vector3(m_rSafeZoneScreenRect.width, 1 - m_rSafeZoneScreenRect.height, 1) : new Vector3(1, 0, 1);
            }
            else
            {
                vOrigin = _bSafeZone ? new Vector3(1 - m_rSafeZoneScreenRect.width, 1 - m_rSafeZoneScreenRect.height, 1) : new Vector3(0, 0, 1);
            }
        }

        RaycastHit oHit;
        Vector3 vWorldPos = Vector3.zero;
        if (Physics.Raycast(_oCamera.ViewportToWorldPoint(vOrigin), _oCamera.transform.forward, out oHit, Mathf.Infinity, Layers.ms_maskScene))
        {
            vWorldPos = oHit.point;
        }
        return vWorldPos;
    }

    static List<Vector2> m_avScreenSegments = new List<Vector2>();

    static List<Vector2> getCombatScreenWorldSegments(bool _bSafeZone)
    {
        m_avScreenSegments.Clear();
        m_avScreenSegments.Add(GameplayManager.Instance.m_oCombatCamera.getPositionV2WorldFromCamera(false, false, _bSafeZone));
        m_avScreenSegments.Add(GameplayManager.Instance.m_oCombatCamera.getPositionV2WorldFromCamera(false, true, _bSafeZone));
        m_avScreenSegments.Add(GameplayManager.Instance.m_oCombatCamera.getPositionV2WorldFromCamera(true, true, _bSafeZone));
        m_avScreenSegments.Add(GameplayManager.Instance.m_oCombatCamera.getPositionV2WorldFromCamera(true, false, _bSafeZone));
        return m_avScreenSegments;
    }

    public static Vector2 getPositionV2WorldFromCamera(this Camera _oCamera, bool _bTopOrBot, bool _bRightOrLeft, bool _bSafeZone = false)
    {
        return getPositionWorldFromCamera(_oCamera, _bTopOrBot, _bRightOrLeft, _bSafeZone).xz();
    }

    public static List<Vector2> divideSegmentV2Into(Vector2 _vStart, Vector2 _vEnd, float _fDistanceStep)
    {
        List<Vector2> av = new List<Vector2>();
        Vector2 vDir = _vEnd - _vStart;
        float fMaxDistance = vDir.magnitude;
        int iSteps = Mathf.CeilToInt(fMaxDistance / _fDistanceStep);
        for (int i = 0; i < iSteps; i++)
        {
            av.Add(_vStart + vDir.normalized * _fDistanceStep * i);
        }
        if (!av.Contains(_vEnd)) av.Add(_vEnd);

        // debug
        for (int i = 0; i < av.Count; i++)
        {
            //Deb.Instance.drawCircle(av[i]);
        }
        return av;
    }


    public static Vector3 getPositionInCamera(this Vector3 _vT, bool _bFromCombat)
    {
        Camera _oFromCamera = _bFromCombat ? GameplayManager.Instance.m_oCombatCamera : GameplayManager.Instance.m_oManagementCamera;
        Camera _oToCamera = _bFromCombat ? GameplayManager.Instance.m_oManagementCamera : GameplayManager.Instance.m_oCombatCamera;
        Vector3 vS = _oFromCamera.WorldToScreenPoint(_vT);
        Vector3 vW = _oToCamera.ScreenToWorldPoint(vS);
        vW.z = 0;
        return vW;
    }

    public static Vector3 getPositionInCamera(this Vector2 _vT, bool _bFromCombat)
    {
        return getPositionInCamera(_vT.toVector3xz(), _bFromCombat);
    }

    #endregion RECT UTILS

    #region Misc

    public static string print(this int[] _v)
    {
        string s = "(";
        for (int index = 0; index < _v.Length; ++index)
        {
            s += (index > 0 ? ", " : "") + _v[index];
        }
        s += ")";
        return s;
    }

    public static string print(this float[] _v)
    {
        string s = "(";
        for (int index = 0; index < _v.Length; ++index)
        {
            s += (index > 0 ? ", " : "") + _v[index].toString();
        }
        s += ")";
        return s;
    }

    public static string print(this Vector2 _v)
    {
        return _v.ToString("f2");
    }

    public static string print(this Vector2 _v, int _iDecimals = 2)
    {
        return _v.ToString("f" + _iDecimals);
    }

    public static string print(this Vector3 _v)
    {
        return _v.ToString("f2");
    }

    public static string print(this float _f)
    {
        return _f.ToString("f2");
    }

    public static string print(this float _f, int _iDecimals)
    {
        return _f.ToString("f" + _iDecimals);
    }

    public static void setAudioTimeScale(float _fTimeScale)
    {
        // update audio speed
        AudioSource[] oSources = GameObject.FindObjectsByType<AudioSource>(0);
        for (int i = 0; i < oSources.Length; ++i)
        {
            oSources[i].pitch = _fTimeScale;
        }
    }

    public static void setLayer(this Transform _oT, int _iLayer, bool _bTryUseContext = true)
    {
        if (_bTryUseContext)
        {
            _oT.setLayerUsingContext(_iLayer);
        }
        else
        {
            _oT.setLayerWithoutUsingContext(_iLayer);
        }
    }

    private static void setLayerUsingContext(this Transform _oT, int _iLayer)
    {
        _oT.gameObject.setLayerDelayed(_iLayer);
        for (int iIndex = 0; iIndex < _oT.childCount; ++iIndex)
        {
            _oT.GetChild(iIndex).setLayerUsingContext(_iLayer);
        }
    }

    private static void setLayerWithoutUsingContext(this Transform _oT, int _iLayer)
    {
        _oT.gameObject.setLayer(_iLayer);
        for (int iIndex = 0; iIndex < _oT.childCount; ++iIndex)
        {
            _oT.GetChild(iIndex).setLayerWithoutUsingContext(_iLayer);
        }
    }

    public static void setLightLayer(this Transform _oT, uint _iLayer)
    {
        for (int iIndex = 0; iIndex < _oT.childCount; ++iIndex)
        {
            Renderer[] oRenderer = _oT.GetChild(iIndex).GetComponents<Renderer>();
            for (int i = 0; i < oRenderer.Length; i++)
            {
                oRenderer[i].renderingLayerMask = _iLayer;
            }
            _oT.GetChild(iIndex).setLightLayer(_iLayer);
        }
    }

    #endregion MISC

    #region Length

    public static float totalLength(this Vector2[] _aoPoints)
    {
        float fLength = 0;
        for (int iIndex = 1; iIndex < _aoPoints.Length; ++iIndex)
        {
            fLength += _aoPoints[iIndex].distance(_aoPoints[iIndex - 1]);
        }
        return fLength;
    }

    public static float totalLength(this List<Vector2> _aoPoints)
    {
        float fLength = 0;
        for (int iIndex = 1; iIndex < _aoPoints.Count; ++iIndex)
        {
            fLength += _aoPoints[iIndex].distance(_aoPoints[iIndex - 1]);
        }
        return fLength;
    }

    public static Vector2[] clampPath(this Vector2[] _aoPoints, float _fMaxDistance)
    {
        if (_aoPoints.totalLength() > _fMaxDistance)
        {
            List<Vector2> aoPoints = new List<Vector2>();

            float fTotalLength = 0;
            aoPoints.Add(_aoPoints[0]);
            for (int iIndex = 1; iIndex < _aoPoints.Length; ++iIndex)
            {
                float fPartLength = _aoPoints[iIndex].distance(_aoPoints[iIndex - 1]);

                if (fTotalLength + fPartLength > _fMaxDistance)
                {
                    Vector2 vNewPoint = _aoPoints[iIndex - 1] + ((_aoPoints[iIndex] - _aoPoints[iIndex - 1]).normalized * (_fMaxDistance - fTotalLength));
                    aoPoints.Add(vNewPoint);
                    break;
                }
                else
                {
                    aoPoints.Add(_aoPoints[iIndex]);
                    fTotalLength += fPartLength;
                }
            }

            return aoPoints.ToArray();
        }

        return _aoPoints;
    }

    #endregion LENGTH

    #region Input

    public static bool acceptButton(bool _bCheckMouse = true, bool _bCheckKeyboard = true, bool _bLock = true)
    {
        if (_bLock && _bCheckMouse && consumeInjectedAccept())
        {
            return true;
        }

        if ((_bCheckKeyboard && RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kAccept, _bLock))
            || (_bCheckMouse && RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kAcceptMouse, _bLock)))
        {
            return true;
        }
        return false;
    }

    public static bool acceptButtonIgnoringLock(bool _bCheckMouse = true, bool _bCheckKeyboard = true)
    {
        if ((_bCheckKeyboard && RewiredManager.Instance.GetActionDownAnyPlayerIgnoringLock(RewiredManager.kAccept))
            || (_bCheckMouse && RewiredManager.Instance.GetActionDownAnyPlayerIgnoringLock(RewiredManager.kAcceptMouse)))
        {
            return true;
        }
        return false;
    }

    public static bool cancelButton()
    {
        if (consumeInjectedCancel())
        {
            return true;
        }

        if (RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kCancel, true))
        {
            return true;
        }
        return false;
    }
    public static bool pauseButton(bool _bLock)
    {
        if (RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kStart, _bLock))
        {
            return true;
        }

        return false;
    }

    public static bool extraInfoButton(bool _bLock = true)
    {
        if (RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kInfo, _bLock))
        {
            return true;
        }
        return false;
    }

    public static bool isPressingAttackDirection()
    {
        if (hasInjectedAttackDirection())
        {
            return true;
        }
        return RewiredManager.Instance.GetActionAnyPlayer(RewiredManager.kR3);
    }

    public static bool isPressingDownAttackDirection()
    {
        return RewiredManager.Instance.GetActionDownAnyPlayer(RewiredManager.kR3);
    }

    public static bool isUnpressingAttackDirection()
    {
        if (consumeAttackDirection())
        {
            return true;
        }
        return RewiredManager.Instance.GetActionUpAnyPlayer(RewiredManager.kR3);
    }

    public static bool isPressingChangeHeightButton()
    {
        if (consumeInjectedHeightChange())
        {
            return true;
        }

        return RewiredManager.Instance.GetActionDownAnyPlayer("changeHeight")
            || RewiredManager.Instance.GetActionDownAnyPlayer("changeHeightReverse");
    }

    public static void setGamepadPosition(ref Vector3 _vPosition)
    {
        // Define a speed for the cursor movement
        float cursorSpeed = 300f;

        // --- Inside your update method ---

        // Get gamepad input
        Vector2 vGamepadMovement = RewiredManager.Instance.getControllerCursorMovement(RewiredManager.Instance.m_fLSMovementAmountFreeCursor, RewiredManager.Instance.m_fRSMovementAmountFreeCursor);

        // Update position (scaled by speed and Time.deltaTime for frame-rate independence)
        _vPosition += vGamepadMovement.toVector3xy() * cursorSpeed * Time.deltaTime;

        // Clamp the position to the screen boundaries
        _vPosition.x = Mathf.Clamp(_vPosition.x, 0, Screen.width);
        _vPosition.y = Mathf.Clamp(_vPosition.y, 0, Screen.height);
    }

    public static void injectChangeHeight()
    {
        m_bInjectedChangeHeight = true;
    }

    public static void injectAccept()
    {
        m_bInjectedAccept = true;
    }

    public static void injectAttackDirection(Vector2 _vWorldPosition)
    {
        m_bHasInjectedCursorWorldPosition = true;
        m_vInjectedCursorWorldPosition = _vWorldPosition;
        m_bHasInjectedAttackDirection = true;
    }

    public static void injectAccept(Vector2 _vWorldPosition, bool _bControlClicked)
    {
        injectAccept();
        m_bHasInjectedCursorWorldPosition = true;
        m_vInjectedCursorWorldPosition = _vWorldPosition;
        m_bHasInjectedAttackDirection = _bControlClicked;
    }

    public static void injectCancel()
    {
        m_bInjectedCancel = true;
    }

    public static void clearInjectedInput()
    {
        m_bInjectedAccept = false;
        m_bInjectedCancel = false;
        m_bInjectedChangeHeight = false;
        m_bHasInjectedCursorWorldPosition = false;
        m_bHasInjectedAttackDirection = false;
        m_vInjectedCursorWorldPosition = Vector2.zero;
    }

    public static bool hasInjectedCursorWorldPosition()
    {
        return m_bHasInjectedCursorWorldPosition;
    }

    public static Vector2 getInjectedCursorWorldPosition()
    {
        return m_vInjectedCursorWorldPosition;
    }

    public static bool hasInjectedAttackDirection()
    {
        return m_bHasInjectedAttackDirection;
    }

    static bool consumeInjectedAccept()
    {
        if (!m_bInjectedAccept)
        {
            return false;
        }
        clearInjectedInput();
        return true;
    }

    static bool consumeAttackDirection()
    {
        if (!m_bHasInjectedAttackDirection)
        {
            return false;
        }
        clearInjectedInput();
        return true;
    }

    static bool consumeInjectedHeightChange()
    {
        if (!m_bInjectedChangeHeight)
        {
            return false;
        }

        clearInjectedInput();
        return true;
    }

    static bool consumeInjectedCancel()
    {
        if (!m_bInjectedCancel)
        {
            return false;
        }

        clearInjectedInput();
        return true;
    }

    #endregion

    #region GameEntity
    public static List<T> mixWithLamba<T>(this List<T> aoList, Func<T, bool> func)
    {
        List<T> aoTempEntityList = new List<T>();
        for (int i = 0; i < aoList.Count; i++)
        {
            if (func(aoList[i]))
            {
                aoTempEntityList.Add(aoList[i]);
            }
        }
        return aoTempEntityList;
    }

    public static List<T> mixWithLamba<T>(this T[] aoList, Func<T, bool> func)
    {
        List<T> aoTempEntityList = new List<T>();
        for (int i = 0; i < aoList.Length; i++)
        {
            if (func(aoList[i]))
            {
                aoTempEntityList.Add(aoList[i]);
            }
        }
        return aoTempEntityList;
    }

    public static List<T> checkListWithLambda<T>(this List<T> aoList, Func<T, bool> func)
    {
        for (int i = 0; i < aoList.Count; i++)
        {
            if (!func(aoList[i]))
            {
                aoList.RemoveAt(i);
                i--;
            }
        }
        return aoList;
    }

    public static bool isArea(this string _sText)
    {
        return _sText.Length == 2 && _sText[0] == 'a';
    }

    public static int numberOfTimes<T>(this List<T> _ao, UnityEngine.Object _o)
    {
        int iNumberOfTimes = 0;

        for (int i = 0; i < _ao.Count; ++i)
        {
            if (_ao[i].Equals(_o)) { iNumberOfTimes++; }
        }

        return iNumberOfTimes;
    }

    #endregion

    #region Skills

    public static void removeFarthestPositionsFrom(this List<Vector2> _aoPositions, Vector2 _vOrigin)
    {
        if (_aoPositions.isNullOrEmpty()) return;
        //ProbabilityPair<AIManager.AIScores> iScore = null;
        Vector2 fClosestVector2 = _aoPositions[0];
        for (int i = 1; i < _aoPositions.Count; i++)
        {
            if (_aoPositions[i].distance(_vOrigin) < fClosestVector2.distance(_vOrigin))
            {
                fClosestVector2 = _aoPositions[i];
            }
        }
        if (_aoPositions.Count > 0)
        {
            _aoPositions.removeAllExcept(fClosestVector2);
        }
    }


    #endregion

    #region Map

    public static bool checkIntersections(this List<Vector2> _av, Vector2 _v1, Vector2 _v2, ref Vector2 _vIntersection)
    {
        for (int i = 1; i < _av.Count; ++i)
        {
            if (Utils.isIntersecting(_v1, _v2, _av[i - 1], _av[i], ref _vIntersection))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Others


    public static List<T> getRandoms<T>(this List<T> _aoList, int iMax, string _sContext)
    {
        List<T> aoRandomSelected = new List<T>();
        for (int i = 0; i < iMax; i++)
        {
            aoRandomSelected.Add(_aoList.getRandomElement(GameplayManager.NODE_SEED, _sContext));
        }
        return aoRandomSelected;
    }
    public static void add<T>(this Dictionary<T, int> _aoDict, T eT, int i)
    {
        if (_aoDict.ContainsKey(eT))
        {
            _aoDict[eT] += i;
        }
        else
        {
            _aoDict[eT] = i;
        }
    }
    
    public static void calculateCapsule(this LineRenderer _oLine, Vector2 _vStart, Vector2 _vEnd, float _fRadius)
    {
        int iSegments = 36;
        _oLine.positionCount = iSegments;

        Vector2 vDirection = (_vEnd - _vStart).normalized;
        Vector2 vPerp = Vector2.Perpendicular(vDirection);

        float fAngle = Vector2.SignedAngle(vPerp, Vector2.up);
        float x, y;
        for (int iIndex = 0; iIndex < iSegments; ++iIndex)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * fAngle) * _fRadius;
            y = Mathf.Cos(Mathf.Deg2Rad * fAngle) * _fRadius;
            if (iIndex >= iSegments / 2)
            {
                _oLine.SetPosition(iIndex, _vStart.toVector3xy() + new Vector3(x, y, 0));
            }
            else
            {
                _oLine.SetPosition(iIndex, _vEnd.toVector3xy() + new Vector3(x, y, 0));
            }
            fAngle += 360 / (iSegments - 1);
        }
    }

    public static void calculatePolygon(this LineRenderer _oLine, Vector2[] _avPoints)
    {
        _oLine.positionCount = _avPoints.Length;
        for (int i = 0; i < _oLine.positionCount; ++i)
        {
            _oLine.SetPosition(i, new Vector3(_avPoints[i].x, 0.0f, _avPoints[i].y));
        }
    }

    #endregion

    #region Debug Draw

    public static void DrawPoint(Vector3 _vPoint, float _fSize, Color _cColor, float _fTime = 0)
    {
        Debug.DrawLine(_vPoint - (Vector3.up * _fSize), _vPoint + (Vector3.up * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint - (Vector3.forward * _fSize), _vPoint + (Vector3.forward * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint - (Vector3.right * _fSize), _vPoint + (Vector3.right * _fSize), _cColor, _fTime);
    }

    public static void DrawPoint2(Vector3 _vPoint, float _fSize, Color _cColor, float _fTime = 0)
    {
        Debug.DrawLine(_vPoint - (Vector3.up * _fSize), _vPoint + (Vector3.up * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint - ((Vector3.forward + Vector3.right) * _fSize), _vPoint + ((Vector3.forward + Vector3.right) * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint - ((Vector3.forward - Vector3.right) * _fSize), _vPoint + ((Vector3.forward - Vector3.right) * _fSize), _cColor, _fTime);
    }

    public static void DrawEllipsis(Vector3 _vCenter, Vector3 _vRight, Vector3 _vUp, Vector2 _vRadius, Color _cColor, float _fTime, float _fSteps = 60)
    {
        for (int iIndex = 0; iIndex < _fSteps; ++iIndex)
        {
            float fAngle1 = iIndex * 360f / _fSteps;
            float fAngle2 = (iIndex + 1) * 360f / _fSteps;
            Vector3 v1 = _vCenter + (_vRight * Mathf.Cos(fAngle1) * _vRadius.x) + (_vUp * Mathf.Sin(fAngle1) * _vRadius.y);
            Vector3 v2 = _vCenter + (_vRight * Mathf.Cos(fAngle2) * _vRadius.x) + (_vUp * Mathf.Sin(fAngle2) * _vRadius.y);
            Debug.DrawLine(v1, v2, _cColor, _fTime);
        }
    }

    #endregion

    #region OTHERS

    public static void destroy(this Component oObj, float _fTime = 0.0f, bool _bImmediate = false)
    {
        if (_fTime > 0.0f)
        {
            Component.Destroy(oObj, _fTime);
        }
        else
        {
            if (_bImmediate)
            {
                Component.DestroyImmediate(oObj);
            }
            else
            {
                Component.Destroy(oObj);
            }
        }
    }

    public static List<Vector2> getCircularPoints(Vector2 center, float innerRadius, float outerRadius, int circles, int pointsPerCircle)
    {
        List<Vector2> points = new List<Vector2>();
        // Calcula el ángulo entre cada punto
        float angleStep = 2 * Mathf.PI / pointsPerCircle;
        // Si hay más de 1 círculo, se reparte la diferencia entre el radio exterior e interior.
        float radiusStep = (circles > 1) ? (outerRadius - innerRadius) / circles : innerRadius;

        // Offset fijo para rotar los puntos (opcional)
        float offset = 0;

        for (int i = 0; i < circles; i++)
        {
            // Calcula el radio del círculo actual
            float currentRadius = innerRadius + i * radiusStep;

            for (int j = 0; j < pointsPerCircle; j++)
            {
                // Calcula el ángulo para cada punto sumando el offset
                float angle = j * angleStep + offset;
                float x = center.x + currentRadius * Mathf.Cos(angle);
                float y = center.y + currentRadius * Mathf.Sin(angle);
                points.Add(new Vector2(x, y));
            }

            offset += angleStep / 2f;
        }
        return points;
    }

    public static T getNextItemInList<T>(this List<T> _aoList, T _oCurrent, bool _bReverseSelection)
    {
        //index of the current item in list, if not any current is 0
        int iCurrentSelectedItemIndex = _oCurrent == null ? _aoList.Count - 1 : _aoList.IndexOf((T)_oCurrent);
        for (int iIndex = 0; iIndex < _aoList.Count; ++iIndex)
        {
            int iIndexToCheck;
            // select backward on Team
            if (_bReverseSelection)
            {
                iIndexToCheck = iCurrentSelectedItemIndex - iIndex - 1 < 0 ? _aoList.Count - 1 - iIndex : iCurrentSelectedItemIndex - 1;
            }
            else
            {
                iIndexToCheck = (iCurrentSelectedItemIndex + iIndex + 1) % _aoList.Count;
            }
            if (_aoList[iIndexToCheck] != null)
            {
                return _aoList[iIndexToCheck];
            }
        }
        return default(T);
    }

    public static bool updateAndCheckTimelineStateFinished(this State _oTimeline)
    {
        if (_oTimeline != null)
        {
            _oTimeline.update();
        }

        if (_oTimeline == null || _oTimeline.m_iTimedEventsLeft <= 0)
        {
            return true;
        }
        return false;
    }


    public static void setVersionedComponentFromParent<T>(ref T _oComponent) where T : Behaviour
    {
        Transform oParent = _oComponent.transform.parent;

        if (oParent == null) { return; }

        _oComponent = oParent.GetComponentInChildrenActiveAndEnabled<T>();
    }

    public static void setActive(this GameObject _oGameobject, bool _bIsActive, bool _bDoAnimation = true, bool _bCheckChilds = false)
    {
        CanvasAnimationComponent oComponent = _oGameobject.GetComponent<CanvasAnimationComponent>();
        if (oComponent == null && _bCheckChilds)
        {
            oComponent = _oGameobject.GetComponentInChildren<CanvasAnimationComponent>();
        }
        if (_bIsActive)
        {
            _oGameobject.SetActive(true);
            if (oComponent != null && _bDoAnimation)
            {
                oComponent.activate();
            }
        }
        else
        {
            if (oComponent != null && _bDoAnimation)
            {
                oComponent.deactivate();
            }
            else { _oGameobject.SetActive(false); }
        }
    }

    #endregion OTHERS

    // Added: list of pre-game scene names and fixed implementation.
    // The previous implementation was syntactically broken and used build index;
    // requirement now is to check equality with copied names.
    public static readonly HashSet<string> PRE_GAME_SCENE_NAMES = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "intro_scene",
        "MainMenu",
        "loading"
    };

    public static bool isInPreGameScenes()
    {
        // Get active scene name and compare directly against the predefined set.
        string sceneName = SceneManager.GetActiveScene().name;
        return PRE_GAME_SCENE_NAMES.Contains(sceneName);
    }

    #region Translations

    public static string getCompleteDescription(int _iDifficulty)
    {
        string sCompleteDescription = "";

        int iInitialIndex = _iDifficulty == 0 ? 0 : 1;

        for (int i = iInitialIndex; i <= _iDifficulty; i++)
        {
            string sDescription = GameUtils.translatedDescriptionText(i);

            if (!sDescription.isNullOrEmpty())
            {
                sCompleteDescription += $"{sDescription}\n";
            }
        }

        return sCompleteDescription;
    }

    public static string translatedDescriptionText(int _iDifficulty)
    {
        string sKey = $"loop_{_iDifficulty.ToString()}_description";
        return Utils.getTranslation(sKey);
    }

    public static void substituteTag(ref string _s, string[] _aoStringValues)
    {
        if (!_s.isNullOrEmpty())
        {
            for (int i = 0; i < _aoStringValues.Length; i++)
            {
                string tag = "{v" + (i + 1) +"}";
                _s = _s.Replace(tag, _aoStringValues[i]);
            }
        }
    }

    #endregion

    public static void refreshLayoutGroupsImmediateAndRecursive(this RectTransform root)
    {
        // Get all layout groups in the hierarchy
        foreach (var layoutGroup in root.GetComponentsInChildren<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        // Finally rebuild the root
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}