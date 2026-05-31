using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using I2.Loc;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.VFX;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System.Globalization;
using StateChangeCollapse;
using System.Runtime.Serialization.Formatters.Binary;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Profile;
#endif

public static class Utils
{
    public static string getParentNames(this GameObject gameObject, bool includeScene = false)
    {
        if (gameObject == null)
            return string.Empty;

        string path = gameObject.name;
        Transform current = gameObject.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        if (includeScene)
        {
            path = gameObject.scene.name + "/" + path;
        }

        return path;
    }
    //
    // Summary:
    //     Determines whether a Unity object is null or "fake null", without ever calling
    //     Unity's own equality operators. This method is useful for checking if a Unity
    //     object is null, destroyed or missing at times when it is not allowed to call
    //     Unity's own equality operators, for example when not running on the main thread.
    //
    //
    // Parameters:
    //   obj:
    //     The Unity object to check.
    //
    // Returns:
    //     True if the object is null, missing or destroyed; otherwise false.
    public static bool isNullOrDestroyed(this System.Object _oObject)
    {
        return object.ReferenceEquals(_oObject, null) ||
            (_oObject is UnityEngine.Object oUnityObject && oUnityObject == null);
    }

    #region STRINGS
    // peta cuando haces mouse over a una entity con un - en el string, por jerarquia de layouts peta cuando haces -X pero no - X o si cambias el simbolo a su valor en ASCII
    public static string minusSignCrashFix(this string _sString) => _sString /*!_sString.isNullOrEmpty() ? _sString.Replace("-", "\u2013") : ""*/;

    public static bool isNullOrEmpty(this string _s)
    {
        return string.IsNullOrEmpty(_s);
    }

    public static string toLower(this string _s, bool _bInvariant = true)
    {
        return _s == null ? null : (_bInvariant ? _s.ToLowerInvariant() : _s.ToLower());
    }

    public static string toUpper(this string _s, bool _bInvariant = true)
    {
        return _s == null ? null : (_bInvariant ? _s.ToUpperInvariant() : _s.ToUpper());
    }

    public static string firstCapitalized(this string input, bool _bInvariant = true)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (input.Length == 1)
            return input.toUpper(_bInvariant);
        return (_bInvariant ? char.ToUpperInvariant(input[0]) : char.ToUpper(input[0])) + input.Substring(1);
    }

    public static string toString(this float _f, int _iDecimals = 2)
    {
        if (_iDecimals < 0)
            _iDecimals = 0;

        string format = "0";

        if (_iDecimals > 0)
            format += "." + new string('#', _iDecimals);

        return _f.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string toString2(this int _i)
    {
        return _i > 9 ? _i.ToString() : "0" + _i.ToString();
    }

    public static string toStringTime(this float _fSeconds)
    {
        int iHours = (int)(_fSeconds / 3600.0f);
        _fSeconds -= iHours * 3600.0f;
        int iMinutes = (int)(_fSeconds / 60.0f);
        _fSeconds -= iMinutes * 60.0f;
        return iHours.toString2() + ":" + iMinutes.toString2() + ":" + ((int)_fSeconds).toString2();
    }

    public static string TrimEnd(this string input, string suffixToRemove)
    {
        if (input != null && suffixToRemove != null
           && input.EndsWith(suffixToRemove))
        {
            return input.Substring(0, input.Length - suffixToRemove.Length);
        }
        else
        {
            return input;
        }
    }

    public static void trimExtensions(ref string[] _as)
    {
        for (int i = 0; i < _as.Length; ++i)
        {
            int index = _as[i].LastIndexOf(".");
            if (index > 0)
            {
                _as[i] = _as[i].Substring(0, index);
            }
        }
    }

    public static string trimExtension(this string _s)
    {
        int index = _s.LastIndexOf(".");
        if (index > 0)
        {
            _s = _s.Substring(0, index);
        }

        return _s;
    }

    public static string reverseString(this string _s)
    {
        char[] charArray = _s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string trimAfter(this string _s, string _sToRemove)
    {
        // Remove everything after founder
        int pos = _s.IndexOf(_sToRemove);
        if (pos >= 0)
        {
            // String after founder
            return _s.Remove(0, pos + _sToRemove.Length);
        }
        return "";
    }

    public static double jaroWinklerDistance(string _sStringA, string _sStringB)
    {
        int sourceLength = _sStringA.Length;
        int targetLength = _sStringB.Length;
        int matchingDistance = (Math.Max(sourceLength, targetLength) / 2) - 1;

        bool[] sourceMatches = new bool[sourceLength];
        bool[] targetMatches = new bool[targetLength];

        int matchingCharacters = 0;

        for (int i = 0; i < sourceLength; i++)
        {
            int start = Math.Max(0, i - matchingDistance);
            int end = Math.Min(i + matchingDistance + 1, targetLength);

            for (int j = start; j < end; j++)
            {
                if (!targetMatches[j] && _sStringA[i] == _sStringB[j])
                {
                    sourceMatches[i] = true;
                    targetMatches[j] = true;
                    matchingCharacters++;
                    break;
                }
            }
        }

        if (matchingCharacters == 0)
        {
            return 0.0;
        }

        int transpositions = 0;
        int k = 0;

        for (int i = 0; i < sourceLength; i++)
        {
            if (sourceMatches[i])
            {
                while (!targetMatches[k])
                {
                    k++;
                }

                if (_sStringA[i] != _sStringB[k])
                {
                    transpositions++;
                }

                k++;
            }
        }

        double jaroSimilarity = ((matchingCharacters / (double)sourceLength) +
                                 (matchingCharacters / (double)targetLength) +
                                 ((matchingCharacters - (transpositions / 2.0)) / matchingCharacters)) / 3.0;

        // Additional boost for common prefix
        int prefixLength = 0;
        int maxPrefixLength = Math.Min(4, Math.Min(sourceLength, targetLength));

        while (prefixLength < maxPrefixLength && _sStringA[prefixLength] == _sStringB[prefixLength])
        {
            prefixLength++;
        }

        double jaroWinklerSimilarity = jaroSimilarity +
            (prefixLength * 0.1 * (1 - jaroSimilarity));

        return jaroWinklerSimilarity;
    }

    public static bool checkStringsSimilarity(string _sStringA, string _sStringToCompare, double _dMaxSim)
    {
        double dIndexS = jaroWinklerDistance(_sStringA, _sStringToCompare);
        bool bSimilar = dIndexS < _dMaxSim;
        if (bSimilar && dIndexS > 0.999)
        {
            Deb.log("Word " + _sStringA + " has missspelling");
        }
        return bSimilar;
    }

    public static Vector3 stringToVector3(this string _sVector)
    {
        // Remove the parentheses
        if (_sVector.StartsWith("(") && _sVector.EndsWith(")"))
        {
            _sVector = _sVector.Substring(1, _sVector.Length - 2);
        }

        // split the items
        string[] asArray = _sVector.Split(',');

        // store as a Vector3
        Vector3 oVector = new Vector3(
            float.Parse(asArray[0]),
            float.Parse(asArray[1]),
            float.Parse(asArray[2]));

        return oVector;
    }

    #endregion

    #region REGEX
    static Regex m_oRegex;
    public static void getChildsRegex(this Transform t, ref List<Transform> _aoResults, string _sRegex)
    {
        m_oRegex = new Regex(_sRegex);
        recursiveGetChildsRegex(t, ref _aoResults);
    }

    static void recursiveGetChildsRegex(Transform t, ref List<Transform> _aoResults)
    {
        if (m_oRegex.IsMatch(t.name))
        {
            _aoResults.Add(t);
            //Deb.log("match " + t.name);
        }

        foreach (Transform c in t)
        {
            recursiveGetChildsRegex(c, ref _aoResults);
        }
    }

    #endregion

    #region TRANSLATIONS
    public static string checkLinebreaks(this string _s)
    {
        if (!string.IsNullOrEmpty(_s) && _s.IndexOf("\\n") >= 0)
        {
            return _s.Replace("\\n", "\n");
        }

        return _s;
    }

    public static bool hasTranslation(this string _sKey, string _sCategory = I2Consts.CATEGORY_I2_MENU_UI)
    {
        string sFullKey = _sCategory == null ? _sKey : $"{_sCategory}/{_sKey}";
        return LocalizationManager.GetTranslation(sFullKey) != null;
    }

    public static string getTranslation(this string _sKey, string _sCategory = I2Consts.CATEGORY_I2_MENU_UI, bool _bApplyParameters = false, string _sOverrideLang = null)
    {
        string sFullKey = _sCategory == null ? _sKey : $"{_sCategory}/{_sKey}";
        string sTranslation = LocalizationManager.GetTranslation(sFullKey, applyParameters: _bApplyParameters, overrideLanguage: _sOverrideLang).checkLinebreaks();

        if (sTranslation == null)
        {
            Deb.logWarning($"[Localisation] Key for {sFullKey} not found.");
        }

        return sTranslation;
    }

    public static string getTranslationSafe(this string _sKey, string _sCategory, bool _bApplyParameters = false, string _sOverrrideLang = null)
    {
        string sTranslation = getTranslation(_sKey, _sCategory, _bApplyParameters, _sOverrrideLang);

        if (sTranslation.isNullOrEmpty()) sTranslation = (_sCategory.isNullOrEmpty() ? "" : _sCategory) + _sKey;

        return sTranslation;
    }

    public static string getTranslationWithParams(this string _sKey)
    {
        return getTranslation(_sKey, null, true);
    }
    #endregion

    #region BITS
    public static Int32 numberOfBitsSetInMask(Int32 mask)
    {
        mask = mask - ((mask >> 1) & 0x55555555);
        mask = (mask & 0x33333333) + ((mask >> 2) & 0x33333333);
        Int32 returnValue = (((mask + (mask >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

        return returnValue;
    }
    #endregion

    #region FOLDERS & FILES

    public static string[] readFile(string _sPath)
    {
        // Check if the file exists
        if (!File.Exists(_sPath)) { return new string[0]; }

        // Open the file and read from it
        using (StreamReader sr = new StreamReader(_sPath))
        {
            List<string> asLines = new List<string>();
            // Read and display lines from the file until the end of the file is reached
            string sLine;

            while ((sLine = sr.ReadLine()) != null)
            {
                asLines.Add(sLine);
            }

            return asLines.ToArray();
        }
    }

    public static bool folderExists(string _sPath)
    {
        return Directory.Exists(_sPath);
    }

    public static void createFolder(string _sPath)
    {
        Directory.CreateDirectory(_sPath);
    }

    public static string[] getProjectFolderElements(this string _sFolder, bool _bTrimExtension = true)
    {
        string[] asAllFiles = Directory.GetFiles(_sFolder, "*", SearchOption.AllDirectories);
        string[] asFiles = new string[asAllFiles.Length / 2];
        int iReal = 0;
        for (int i = 0; i < asAllFiles.Length; ++i)
        {
            if (asAllFiles[i].EndsWith(".meta"))
            {
                continue;
            }

            if (_bTrimExtension)
            {
                int index = asAllFiles[i].LastIndexOf(".");
                if (index > 0)
                {
                    asFiles[iReal] = asAllFiles[i].Substring(0, index);
                }
            }
            else
            {
                asFiles[iReal] = asAllFiles[i];
            }
            // replace the \\ folder slash that we get from GetFiles
            asFiles[iReal] = asFiles[iReal].Replace(@"\", "/");

            ++iReal;
        }

        return asFiles;
    }
#if UNITY_EDITOR
    public static List<T> findAssetsByType<T>(string[] _asFolders) where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string sType = typeof(T).ToString();
        if (sType.Contains("Unity"))
        {
            sType = sType.Substring(12); // delete UnityEngine. string
        }

        string[] guids = AssetDatabase.FindAssets("t:" + sType, _asFolders);
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }
#endif

    public static bool folderContainsElement(this string[] _asFolderElements, string _s, string _sDesiredObjectsTermination)
    {
        for (int i = 0; i < _asFolderElements.Length; ++i)
        {
            string s = _asFolderElements[i].TrimEnd(_sDesiredObjectsTermination);

            if (s.EndsWith(_s))
            {
                return true;
            }
        }

        return false;
    }

    public static void writeToFile(List<string> _aoText, string _sPath)
    {
        using (StreamWriter oWriter = File.CreateText(_sPath))
        {
            for (int i = 0; i < _aoText.Count; i++)
            {
                oWriter.WriteLine(_aoText[i]);
            }
        }
    }

    public static List<string> getFromTxt(string _sPath)
    {
        List<string> aoText = new List<string>();

        using (StreamReader oWriter = File.OpenText(_sPath))
        {
            string sLine = oWriter.ReadLine();
            while (sLine != null)
            {
                aoText.Add(sLine);
            }
        }

        return aoText;
    }

    public static string getDebugFolderPath(string _sFileName)
    {
        string sRootPath = Directory.GetParent(Application.dataPath).FullName;
        sRootPath = Path.Combine(sRootPath, "debug_errors");

        if (!_sFileName.isNullOrEmpty())
        {
            sRootPath = Path.Combine(sRootPath, _sFileName);
        }

        return sRootPath;
    }

    #endregion

    #region TRANSFORMS

    public static void setLayer(this GameObject _oObj, int _iLayer)
    {
        if (_oObj.layer != _iLayer)
        {
            _oObj.layer = _iLayer;
        }
    }

    public static void resetTransform(this Transform _t)
    {
        _t.localEulerAngles = Vector3.zero;
        _t.localPosition = Vector3.zero;
        _t.localScale = Vector3.one;
    }
    public static void copyGlobalScaleTo(this Transform _oFrom, Transform _oTo)
    {
        Vector3 vParentsScale = Vector3.one;
        Transform oParent = _oTo.parent;
        while (oParent != null)
        {
            vParentsScale = vParentsScale.multiply(oParent.localScale);
            oParent = oParent.parent;
        }
        _oTo.localScale = _oFrom.lossyScale.divide(vParentsScale);
    }
    public static void setGlobalScale(this Transform transform, Vector3 globalScale)
    {
        if (globalScale == Vector3.zero || transform.lossyScale == Vector3.zero) return;
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
    
    public static void setGlobalScale(this Transform transform, float fGlobalScale)
    {
        setGlobalScale(transform, new Vector3(fGlobalScale, fGlobalScale, fGlobalScale));
    }

    public static void setPositionXY(this Transform _oT, Vector2 _vPos)
    {
        _oT.position = new Vector3(_vPos.x, _vPos.y, _oT.position.z);
    }

    public static void setPositionXZ(this Transform _oT, Vector2 _vPos)
    {
        _oT.position = new Vector3(_vPos.x, _oT.position.y, _vPos.y);
    }

    public static void lookAtXY(this Transform _oT, Vector2 _vTarget)
    {
        Vector3 vTarget = _vTarget;
        vTarget.z = _oT.position.z;
        _oT.LookAt(vTarget, Vector3.back);
    }

    public static void setZ(this Transform _oT, float _fValue)
    {
        _oT.position = new Vector3(_oT.position.x, _oT.position.y, _fValue);
    }
    public static void localPosXY(this Transform _oT, Vector2 _vPos)
    {
        _oT.localPosition = new Vector3(_vPos.x, _vPos.y, _oT.localPosition.z);
    }
    public static Vector2 posXY(this Transform _oT)
    {
        return _oT.position.xy();
    }
    public static void posXY(this Transform _oT, Vector2 _vPos)
    {
        _oT.position = new Vector3(_vPos.x, _vPos.y, _oT.position.z);
    }

    public static void setLocalPosXZ(this Transform _oT, Vector2 _vPos)
    {
        _oT.localPosition = new Vector3(_vPos.x, _oT.localPosition.y, _vPos.y);
    }

    public static Vector2 posXZ(this Transform _oT)
    {
        return _oT.position.xz();
    }

    public static void posX(this Transform _oT, float _f)
    {
        _oT.position = new Vector3(_f, _oT.position.y, _oT.position.z);
    }
    public static void posY(this Transform _oT, float _f)
    {
        _oT.position = new Vector3(_oT.position.x, _f, _oT.position.z);
    }
    public static void posZ(this Transform _oT, float _f)
    {
        _oT.position = new Vector3(_oT.position.x, _oT.position.y, _f);
    }

    public static void setLocalPositionX(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localPosition;
        vPos.x = _fValue;
        _oT.localPosition = vPos;
    }
    public static void setLocalPositionY(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localPosition;
        vPos.y = _fValue;
        _oT.localPosition = vPos;
    }
    public static void setLocalPositionZ(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localPosition;
        vPos.z = _fValue;
        _oT.localPosition = vPos;
    }
    public static void setLocalScaleX(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localScale;
        vPos.x = _fValue;
        _oT.localScale = vPos;
    }
    public static void setLocalScaleY(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localScale;
        vPos.y = _fValue;
        _oT.localScale = vPos;
    }
    public static void setLocalScaleZ(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.localScale;
        vPos.z = _fValue;
        _oT.localScale = vPos;
    }

    public static void setPositionX(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.position;
        vPos.x = _fValue;
        _oT.position = vPos;
    }
    public static void setPositionY(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.position;
        vPos.y = _fValue;
        _oT.position = vPos;
    }
    public static void setPositionZ(this Transform _oT, float _fValue)
    {
        Vector3 vPos = _oT.position;
        vPos.z = _fValue;
        _oT.position = vPos;
    }

    public static void copyDataTo(this Transform _oFrom, Transform _oTo, bool _bPosition = true, bool _bRotation = true, bool _bScale = false)
    {
        if (_bPosition)
        {
            _oTo.position = _oFrom.position;
        }

        if (_bRotation)
        {
            _oTo.rotation = _oFrom.rotation;
        }

        if (_bScale)
        {
            _oTo.localScale = _oFrom.localScale;
        }
    }

    public static void copyDataFrom(this Transform _oTo, Transform _oFrom, bool _bPosition = true, bool _bRotation = true, bool _bScale = false)
    {
        if (_bPosition)
        {
            _oTo.position = _oFrom.position;
        }

        if (_bRotation)
        {
            _oTo.rotation = _oFrom.rotation;
        }

        if (_bScale)
        {
            _oTo.localScale = _oFrom.localScale;
        }
    }

    public static Transform createChild(this Transform _oT, string _sName, bool _bPosZero = false)
    {
        GameObject oNew = new GameObject(_sName);
        oNew.transform.parent = _oT;
        if (_bPosZero)
        {
            oNew.transform.localPosition = Vector3.zero;
        }
        return oNew.transform;
    }

    public static void setLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void setRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void setTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void setBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static void destroyChildren(this Transform _t)
    {
        for (int iIndex = _t.childCount - 1; iIndex >= 0; --iIndex)
        {
            Transform _tChild = _t.GetChild(iIndex);
            _tChild.SetParent(null);
            _tChild.gameObject.SetActive(false);
            _tChild.gameObject.destroy(0, !Application.isPlaying);
        }
    }

    #endregion

    #region TRANSFORM HIERARCHY

    public static string getRelativePath(this Transform child, Transform root)
    {
        if (child == root)
            return "";

        if (child.parent == root)
            return child.name;

        return getRelativePath(child.parent, root) + "/" + child.name;
    }

    //Return the first immediate child with a name containig _sName
    public static Transform getImmediateChildContainsName(this Transform _oTransform, string _sName)
    {
        int iChildCount = _oTransform.childCount;
        for (int iChildIndex = 0; iChildIndex < iChildCount; ++iChildIndex)
        {
            Transform oChild = _oTransform.GetChild(iChildIndex);
            if (oChild.name.Contains(_sName))
            {
                return oChild;
            }
        }
        return null;
    }

    public static Transform getChild(this Transform t, string name)
    {
        Transform child = t.Find(name);
        if (child != null)
        {
            return child;
        }
        else
        {
            foreach (Transform c in t)
            {
                child = c.getChild(name);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return null;
    }
    public static Transform getOrCreateChild(this Transform t, int _iChild)
    {
        if (t.childCount == 0)
        {
            return null;
        }
        for (int i = t.childCount; i <= _iChild; ++i)
        {
            t.GetChild(0).gameObject.clone(t);
        }
        return t.GetChild(_iChild);
    }
    public static Transform getFirstDisabledChild(this Transform t)
    {
        if (t.childCount == 0)
        {
            return null;
        }

        for (int i = 0; i < t.childCount; i++)
        {
            if (!t.GetChild(i).gameObject.activeSelf)
            {
                return t.GetChild(i);
            }
        }
        return t.GetChild(0);
    }
    public static T getFirstComponentInChildrenOfChildrens<T>(this Transform _t, bool _bAlsoInactive) where T : Component
    {
        foreach (Transform child in _t)
        {
            T oResult = child.GetComponentInChildren<T>(_bAlsoInactive);
            if (oResult != null)
            {
                return oResult;
            }
        }
        return null;
    }

    public static T getFirstComponentInChildren<T>(this Transform _t) where T : Component
    {
        for (int i = 0; i < _t.childCount; i++)
        {
            T oResult = _t.GetChild(i).GetComponent<T>();
            if (oResult != null)
            {
                return oResult;
            }
        }
        return null;
    }

    public static List<T> getFirstComponentsInChildren<T>(this Transform _t) where T : Component
    {
        List<T> aoT = new List<T>();

        for (int i = 0; i < _t.childCount; i++)
        {
            List<T> oResult = _t.GetChild(i).GetComponents<T>().toList();
            if (oResult != null)
            {
                aoT.AddRange(oResult);
            }
        }
        return aoT;
    }

    public static T getComponentInNamedChildren<T>(this Transform _t, string _sName) where T : Component
    {
        foreach (Transform child in _t)
        {
            if (child.name == _sName)
            {
                T oT = child.GetComponent<T>();
                if (oT != null)
                {
                    return oT;
                }
            }
        }

        foreach (Transform child in _t)
        {
            T oT = child.getComponentInNamedChildren<T>(_sName);
            if (oT != null)
            {
                return oT;
            }
        }

        return null;
    }

    public static T[] getComponentsInChildrenLevels<T>(this Transform _t, int _iLevels) where T : Component
    {
        List<T> aoT = new List<T>();
        _t.addComponentsInChildrenToList<T>(ref aoT, _iLevels);
        return aoT.ToArray();
    }

    public static void addComponentsInChildrenToList<T>(this Transform _t, ref List<T> _aoT, int _iLevels) where T : Component
    {
        if (_iLevels <= 0)
        {
            return;
        }

        foreach (Transform child in _t)
        {
            if (child.gameObject.activeSelf)
            {
                if (child.GetComponent<T>() != null)
                {
                    _aoT.Add(child.GetComponent<T>());
                }
                child.addComponentsInChildrenToList<T>(ref _aoT, _iLevels - 1);
            }
        }
    }

    public static List<T> getComponentsInChildrenFirstLevel<T>(this Transform _t) where T : Component
    {
        List<T> aoT = new List<T>();
        foreach (Transform child in _t)
        {
            if (child.gameObject.activeSelf)
            {
                if (child.GetComponent<T>() != null)
                {
                    aoT.AddRange(child.GetComponents<T>());
                }
            }
        }
        return aoT;
    }


    public static T getComponentInHierarchySmartly<T>(this Transform _t, bool _bParents = true) where T : Component
    {
        T c = _t.GetComponent<T>();
        if (c != null)
        {
            return c;
        }

        c = _t.GetComponentInChildren<T>();
        if (c != null)
        {
            return c;
        }

        if (_t.parent == null)
        {
            return null;
        }

        c = _t.parent.GetComponentInParent<T>();
        if (c != null)
        {
            return c;
        }

        return null;
    }

    // like the Unity getComponentInParents with possibility to avoid searching itself
    public static T getComponentInParents<T>(this Transform _t, bool _bIncludeSelf) where T : Component
    {
        if (_bIncludeSelf)
        {
            T c = _t.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
        }

        if (_t.parent != null)
        {
            return _t.parent.getComponentInParents<T>(true);
        }

        return null;
    }

    // like the Unity getComponentInParents with possibility to avoid searching itself
    public static T getComponentInChilds<T>(this Transform _t, bool _bIncludeSelf = false, bool _bIncludeInactive = false) where T : Component
    {
        if (_bIncludeSelf)
        {
            T c = _t.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
        }

        if (_t.childCount > 0)
        {
            return _t.GetChild(0).GetComponentInChildren<T>(_bIncludeInactive);
        }

        return null;
    }

    #endregion

    #region GAME OBJECTS

    public static T[] getChilds<T>(this Transform _oParent) where T : Component
    {
        int iChildCount = _oParent.childCount;

        List<T> aoChilds = new List<T>();

        for (int i = 0; i < iChildCount; i++)
        {
            Transform _oChildTransform = _oParent.GetChild(i);
            T oChildComponent = _oChildTransform.GetComponent<T>();

            if (oChildComponent != null)
            {
                aoChilds.Add(oChildComponent);
            }
        }

        return aoChilds.ToArray();
    }

    public static GameObject[] getChildsGameObjects(this Transform _oParent)
    {
        int iChildCount = _oParent.childCount;

        GameObject[] aoChilds = new GameObject[iChildCount];

        for (int i = 0; i < iChildCount; i++)
        {
            aoChilds[i] = _oParent.GetChild(i).gameObject;
        }

        return aoChilds;
    }

    public static void deleteAllComponents(GameObject _o)
    {
        Component[] aoComponents = _o.GetComponents<Component>();
        for (int iComponentIndex = 0; iComponentIndex < aoComponents.Length; iComponentIndex++)
        {
            if (!(aoComponents[iComponentIndex] is Transform))
            {
                aoComponents[iComponentIndex].GetComponent<Behaviour>().enabled = false;
                GameObject.Destroy(aoComponents[iComponentIndex]);
            }
        }
    }
    public static T createObject<T>(string _sName = "new object")
    {
        GameObject o = new GameObject(_sName, typeof(T));
        return o.GetComponent<T>();
    }

    public static GameObject clone(this GameObject _o, Vector3 _vPos, Quaternion _oQ, Transform _oParent, bool _bRemoveCloneFromName, Transform _oCopyScaleFrom = null)
    {
        GameObject go = (GameObject)GameObject.Instantiate(_o, _vPos, _oQ);
        if (_oCopyScaleFrom != null)
        {
            _oCopyScaleFrom.copyGlobalScaleTo(go.transform);
        }

        if (_bRemoveCloneFromName)
        {
            go.name = _o.name;
        }

        if (_oParent != null)
        {
            go.transform.SetParent(_oParent);
        }

        return go;
    }

    public static GameObject clone(this GameObject _o, Vector3 _vPos, Transform _oParent = null, bool _bRemoveCloneFromName = true, Transform _oCopyScaleFrom = null)
    {
        return _o.clone(_vPos, _o.transform.rotation, _oParent, _bRemoveCloneFromName, _oCopyScaleFrom);
    }

    public static GameObject clone(this GameObject _o, Transform _oParent = null, bool _bRemoveCloneFromName = true, Transform _oCopyScaleFrom = null)
    {
        return _o.clone(_o.transform.position, _o.transform.rotation, _oParent, _bRemoveCloneFromName, _oCopyScaleFrom);
    }

    public static T clone<T>(this T _o, Transform _oParent = null, bool _bRemoveCloneFromName = true, Transform _oCopyScaleFrom = null) where T : MonoBehaviour
    {
        return _o.gameObject.clone(_o.transform.position, _o.transform.rotation, _oParent, _bRemoveCloneFromName, _oCopyScaleFrom).GetComponent<T>();
    }

    public static GameObject getInstance(this GameObject _o, Vector3 _vPos, Quaternion _oRotation, Transform _oParent = null)
    {
        GameObject oGO = _o.getInstance(_vPos, _oParent);
        oGO.transform.rotation = _oRotation;
        return oGO;
    }

    public static GameObject getInstance(this GameObject _o, Vector3 _vPos, Transform _oParent = null)
    {
        GameObject oGO = _o.getInstance();
        oGO.transform.position = _vPos;
        oGO.transform.SetParent(_oParent);
        return oGO;
    }

    public static GameObject getInstance(this GameObject _o, Transform _oParent)
    {
        GameObject oGO = _o.getInstance();
        oGO.transform.SetParent(_oParent);
        return oGO;
    }

    public static GameObject getInstance(this GameObject _o)
    {
        // TODO create some ItemDispatcher (pools, random selectors, custom lists...)
        return _o.clone();
    }

    public static void destroy(this GameObject _oGO, float _fTime = 0.0f, bool _bImmediate = false)
    {
        if (_fTime > 0.0f)
        {
            GameObject.Destroy(_oGO, _fTime);
        }
        else
        {
            if (_bImmediate)
            {
                GameObject.DestroyImmediate(_oGO);
            }
            else
            {
                GameObject.Destroy(_oGO);
            }
        }
    }

    public static void destroy(this Transform _o, float _fTime = 0.0f, bool _bImmediate = false)
    {
        if (_fTime > 0.0f)
        {
            GameObject.Destroy(_o.gameObject, _fTime);
        }
        else
        {
            if (_bImmediate)
            {
                GameObject.DestroyImmediate(_o.gameObject);
            }
            else
            {
                GameObject.Destroy(_o.gameObject);
            }
        }
    }

    public static T getCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType())
        {
            return null; // type mis-match
        }

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public static T addComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().getCopyOf(toAdd) as T;
    }//Example usage  Health myHealth = gameObject.AddComponent<Health>(enemy.health); used to copy component values

    public static Component copyComponent(this GameObject destination, Component original)
    {
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);

        // Use your existing getCopyOf method
        copy.getCopyOf(original);

        return copy;
    }
    #endregion

    #region MONO BEHAVIOURS & COMPONENTS

    public static Vector2 posXY(this MonoBehaviour _oT)
    {
        return _oT.transform.position.xy();
    }

    public static void posXY(this MonoBehaviour _oT, Vector2 _v)
    {
        _oT.transform.position = new Vector3(_v.x, _v.y, _oT.transform.position.z);
    }

    public static Vector2 posXZ(this MonoBehaviour _oT)
    {
        return _oT.transform.position.xz();
    }

    public static void posXZ(this MonoBehaviour _oT, Vector2 _v)
    {
        _oT.transform.position = new Vector3(_v.x, _oT.transform.position.y, _v.y);
    }

    public static float posX(this MonoBehaviour _oT)
    {
        return _oT.transform.position.x;
    }

    public static void posX(this MonoBehaviour _oT, float _f)
    {
        _oT.transform.position = new Vector3(_f, _oT.transform.position.y, _oT.transform.position.z);
    }

    public static float posY(this MonoBehaviour _oT)
    {
        return _oT.transform.position.y;
    }

    public static void posY(this MonoBehaviour _oT, float _f)
    {
        _oT.transform.position = new Vector3(_oT.transform.position.x, _f, _oT.transform.position.z);
    }

    public static float posZ(this MonoBehaviour _oT)
    {
        return _oT.transform.position.z;
    }

    public static bool isNull(this MonoBehaviour _oT)
    {
        return _oT == null;
    }

    public static T instantiate<T>() where T : MonoBehaviour
    {
        GameObject oNew = new GameObject();
        return oNew.AddComponent<T>();
    }

    public static T getComponent<T>(string _sObjectName) where T : Component
    {
        T[] ao = GameObject.FindObjectsByType<T>(0);

        for (int i = 0; i < ao.Length; ++i)
        {
            if (ao[i].name == _sObjectName)
            {
                return ao[i];
            }
        }

        return null;
    }

    public static T getComponentInChildren<T>(this GameObject _oParent, string _sObjectName) where T : Component
    {
        T[] ao = _oParent.GetComponentsInChildren<T>();

        for (int i = 0; i < ao.Length; ++i)
        {
            if (ao[i].name == _sObjectName)
            {
                return ao[i];
            }
        }

        return null;
    }

    public static bool tryGetComponentInChildren<T>(this GameObject _oParent, out T _oComponent) where T : Component
    {
        bool bComponentFound = _oParent.TryGetComponent<T>(out _oComponent);
        if (!bComponentFound)
        {
            Transform oTransform = _oParent.transform;
            int iChildrenCount = oTransform.childCount;
            for (int iChildIndex = 0; iChildIndex < iChildrenCount && !bComponentFound; ++iChildIndex)
            {
                bComponentFound = oTransform.GetChild(iChildIndex).gameObject.tryGetComponentInChildren<T>(out _oComponent);
            }
        }
        return bComponentFound;
    }

    #endregion

    #region RECT
    public static Rect getUnion(this in Rect _oRect1, in Rect _oRect2)
    {
        return new Rect(Mathf.Min(_oRect1.x, _oRect2.x), Mathf.Min(_oRect1.y, _oRect2.y),
            Mathf.Max(_oRect1.width, _oRect2.width), Mathf.Max(_oRect1.height, _oRect2.height));
    }

    public static Vector2[] getVertices(this in Rect _oRect)
    {
        Vector2 v2Min = _oRect.min;
        Vector2 v2Max = _oRect.max;
        return new Vector2[]
        {
            v2Min,
            new (v2Min.x, v2Min.y),
            new (v2Max.x, v2Min.y),
            new (v2Max.x, v2Min.y),
            v2Max,
            new (v2Min.x, v2Max.y),
            new (v2Max.x, v2Max.y),
            new (v2Min.x, v2Max.y),
        };
    }

    public static Vector3[] getVertices(this in Rect _oRect, in Vector3 _v3Offset)
    {
        Vector3 v3Min = (Vector3)_oRect.min + _v3Offset;
        Vector3 v3Max = (Vector3)_oRect.max + _v3Offset;
        return new Vector3[]
        {
            v3Min,
            new (v3Min.x, v3Min.y, v3Max.z),
            new (v3Max.x, v3Min.y, v3Max.z),
            new (v3Max.x, v3Min.y, v3Min.z),
            v3Max,
            new (v3Min.x, v3Max.y, v3Max.z),
            new (v3Max.x, v3Max.y, v3Min.z),
            new (v3Min.x, v3Max.y, v3Min.z),
        };
    }
    #endregion //#region RECT

    #region RECT TRANSFORMS
    public static bool containsScreenPoint(this RectTransform _oT, Vector2 _vPoint, Camera _oCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(_oT, _vPoint, _oCamera);
    }

    public static Bounds getBounds(this RectTransform _oRectTranform)
    {
        Vector3[] av3WorldCornres = new Vector3[4];
        _oRectTranform.GetWorldCorners(av3WorldCornres);
        return getBoundsFromMinMax(av3WorldCornres[0], av3WorldCornres[2]);
    }
    #endregion

    #region BOUNDS
    public static (Vector3 v3Min, Vector3 v3Max) getMinMaxfromOOBB(this in Bounds _oBounds, in Matrix4x4 _oTranform)
    {
        Vector3[] aoVertices = _oBounds.getAABBVertices();
        Vector3 v3Min = Vector3.positiveInfinity;
        Vector3 v3Max = Vector3.negativeInfinity;
        foreach (ref Vector3 v3Vertex in aoVertices.AsSpan())
        {
            Vector3 transformedVector = _oTranform.MultiplyPoint(v3Vertex);
            v3Min = Vector3.Min(v3Min, transformedVector);
            v3Max = Vector3.Max(v3Max, transformedVector);
        }
        return (v3Min, v3Max);
    }
    public static Bounds getBoundsFromMinMax(in Vector3 _v3Min, in Vector3 _v3Max) => new((_v3Min + _v3Max) * 0.5f, _v3Max - _v3Min);

    public static Bounds fromOOBB(this in Bounds _oBounds, in Matrix4x4 _oTranform)
    {
        (Vector3 v3Min, Vector3 v3Max) = _oBounds.getMinMaxfromOOBB(_oTranform);
        return getBoundsFromMinMax(v3Min, v3Max);
    }

    public static Vector3[] getAABBVertices(this in Bounds _oBounds)
    {
        Vector3 v3Min = _oBounds.min;
        Vector3 v3Max = _oBounds.max;
        return new Vector3[]
        {
            v3Min,
            new (v3Min.x, v3Min.y, v3Max.z),
            new (v3Max.x, v3Min.y, v3Max.z),
            new (v3Max.x, v3Min.y, v3Min.z),
            v3Max,
            new (v3Min.x, v3Max.y, v3Max.z),
            new (v3Max.x, v3Max.y, v3Min.z),
            new (v3Min.x, v3Max.y, v3Min.z),
        };
    }

    public static Bounds getUnion(in this Bounds _oBounds1, in Bounds _oBounds2)
    {
        Vector3 v3Min = Vector3.Min(_oBounds1.min, _oBounds2.min);
        Vector3 v3Max = Vector3.Max(_oBounds1.max, _oBounds2.max);
        return getBoundsFromMinMax(v3Min, v3Max);
    }

    public static Bounds getScaledRelative(in this Bounds _oBounds, in Vector3 _v3ReferencePosition, float _fScale)
    {
        return new((_oBounds.center - _v3ReferencePosition) * _fScale + _v3ReferencePosition, _oBounds.size * _fScale);
    }
    #endregion //#region BOUNDS

    #region ARRAYS & LISTS

    // returns true if the sub list is contained in the main list
    // ex: given a L1: {1, 2, 3, 4, 4}
    // the list L2: {1, 2 } is contained in L1 thus L1.contains(L2) = true
    // the list L2: {1, 2, 2 } is not contained in L1 thus L1.contains(L2) = false
    public static bool contains<T>(this List<T> _aoList, List<T> _aoSubList)
    {
        if (_aoSubList.Count > _aoList.Count) { return false; }

        List<T> aoFullList = _aoList.copy();

        int iItemsRemoved = 0;

        for (int i = 0; i < _aoSubList.Count; i++)
        {
            T itItem = _aoSubList[i];

            if (aoFullList.Contains(itItem))
            {
                aoFullList.Remove(itItem);
                iItemsRemoved++;
            }
            else { return false; }
        }

        return iItemsRemoved == _aoSubList.Count;
    }

    public static bool containsAnyFrom<T>(this List<T> _aoList, List<T> _aoSubList)
    {
        if (_aoList == null || _aoSubList == null || _aoList.Count == 0 || _aoSubList.Count == 0)
        {
            return false;
        }
        for (int i = 0; i < _aoSubList.Count; i++)
        {
            if (_aoList.Contains(_aoSubList[i]))
            {
                return true;
            }
        }
        return false;
    }

    public static void remove<T>(this List<T> _aoList, List<T> _aoSubList)
    {
        for (int i = 0; i < _aoSubList.Count; i++)
        {
            T itItem = _aoSubList[i];
            _aoList.remove(itItem);
        }
    }

    public static void removeLast<T>(this List<T> _oa)
    {
        if (_oa.Count > 0)
        {
            _oa.RemoveAt(_oa.Count - 1);
        }
    }
    public static bool remove<T>(this List<T> _oa, T _oItem)
    {
        if (_oItem != null && _oa.Contains(_oItem))
        {
            _oa.Remove(_oItem);
            return true;
        }
        return false;
    }

    public static void remove<T>(this List<T> _oa, Func<T, bool> func)
    {
        for (int i = 0; i < _oa.Count; i++)
        {
            if (func(_oa[i]))
            {
                _oa.RemoveAt(i);
                i--;
            }
        }
    }

    public static bool remove<T>(this List<T> _oa, T[] _aoItemList)
    {
        if (_aoItemList == null || !_aoItemList.Any())
        {
            return false;
        }

        // Convert the list to a HashSet for O(1) lookup
        HashSet<T> itemSet = new HashSet<T>(_aoItemList);

        // Remove all items in _oa that are present in itemSet
        _oa.RemoveAll(item => itemSet.Contains(item));

        return true;
    }

    public static void removeAllExcept<T>(this List<T> _oa, T _oExcept)
    {
        if (_oa.Count > 0)
        {
            for (int i = 0; i < _oa.Count; i++)
            {
                if (!_oa[i].Equals(_oExcept))
                {
                    _oa.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    public static bool isNullOrEmpty(this Array _oa)
    {
        return _oa == null || _oa.Length == 0;
    }

    public static bool isNullOrEmpty<T>(this List<T> _oa)
    {
        return _oa == null || _oa.Count == 0;
    }

    public static bool validateIndex<T>(this List<T> _ao, int _i)
    {
        return !_ao.isNullOrEmpty() && _i >= 0 && _i < _ao.Count;
    }
    
    public static bool validateNotNullIndex<T>(this List<T> _ao, int _i)
    {
        return !_ao.isNullOrEmpty() && _i >= 0 && _i < _ao.Count && _ao[_i] != null;
    }

    public static List<T> toList<T>(this T[] _ao)
    {
        List<T> list = new List<T>();
        for (int i = 0; i < _ao.Length; ++i)
        {
            list.Add(_ao[i]);
        }
        return list;
    }

    public static TKey randomKey<TKey, TValue>(this Dictionary<TKey, TValue> _ao, string _sSeed, List<TKey> _aoExcluding = null)
    {
        Dictionary<TKey, TValue> _aoExcluded = new Dictionary<TKey, TValue>();
        int fRandom = RandomUtils.getInt(_sSeed, 0, _ao.Count, "[Utils] Select key 1");
        int fRandom2 = RandomUtils.getInt(_sSeed, 0, _aoExcluded.Count, "[Utils] Select key 2");
        if (!_aoExcluding.isNullOrEmpty())
        {
            for (int i = 0; i < _ao.Count; i++)
            {
                if (!_aoExcluding.Contains(_ao.ElementAt(i).Key))
                {
                    _aoExcluded[_ao.ElementAt(i).Key] = _ao.ElementAt(i).Value;
                }
            }
            if (_aoExcluded.Count < 1)
            {
                Deb.log("All keys are excluded, sending an excluded");
                return _ao.ElementAt(fRandom).Key;
            }
            return _aoExcluded.ElementAt(fRandom2).Key;
        }
        return _ao.ElementAt(fRandom).Key;
    }

    public static void activateAll(this GameObject[] _ao, bool _bValue, int _iException = -1)
    {
        for (int i = 0; i < _ao.Length; ++i)
        {
            if (_ao[i] != null)
            {
                _ao[i].SetActive(i != _iException ? _bValue : !_bValue);
            }
        }
    }

    public static void activateAll(this IEnumerable<MonoBehaviour> _ao, bool _bValue, int _iException = -1)
    {
        for (int i = 0; i < _ao.Count(); ++i)
        {
            _ao.ElementAt(i).gameObject.SetActive(i != _iException ? _bValue : !_bValue);
        }
    }

    public static void activateAllChilds(this Transform _oT, bool _bValue)
    {
        for (int i = 0; i < _oT.childCount; ++i)
        {
            _oT.GetChild(i).gameObject.SetActive(_bValue);
        }
    }

    public static void enableAll(this MonoBehaviour[] _ao, bool _bValue, int _iException = -1)
    {
        for (int i = 0; i < _ao.Length; ++i)
        {
            _ao[i].enabled = i != _iException ? _bValue : !_bValue;
        }
    }

    public static void enableAll<T>(this List<T> _ao, bool _bValue, int _iException = -1) where T : MonoBehaviour
    {
        for (int i = 0; i < _ao.Count; ++i)
        {
            _ao[i].enabled = i != _iException ? _bValue : !_bValue;
        }
    }

    public static T getNextCircular<T>(this T[] _ao, int _iCurrent)
    {
        if (_iCurrent < _ao.Length - 1)
        {
            return _ao[_iCurrent + 1];
        }

        return _ao[0];
    }

    public static T getPreviousSafe<T>(this T[] _ao, int _iCurrent)
    {
        if (_iCurrent > 0)
        {
            return _ao[_iCurrent - 1];
        }

        return _ao[_ao.Length - 1];
    }

    //next value in enum
    public static T getNextCircular<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum",
                                                       typeof(T).FullName));
        }

        T[] Arr = (T[])Enum.GetValues(_enumValue.GetType());

        int j = (Array.IndexOf<T>(Arr, _enumValue) + 1) % Arr.Length; // <- Modulo % Arr.Length added

        return Arr[j];
    }

    //next value in enum
    public static T getNext<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum",
                                                       typeof(T).FullName));
        }

        T[] Arr = (T[])Enum.GetValues(_enumValue.GetType());
        int iIndex = Array.IndexOf<T>(Arr, _enumValue);
        if (iIndex + 1 == Array.LastIndexOf<T>(Arr, _enumValue))
        {
            return _enumValue;
        }
        int j = (iIndex + 1) % Arr.Length; // <- Modulo % Arr.Length added

        return Arr[j];
    }

    //prev value in enum
    public static T getPreviousSafe<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        T[] values = (T[])Enum.GetValues(_enumValue.GetType());

        int currentIndex = Array.IndexOf(values, _enumValue);
        if (currentIndex == -1)
        {
            throw new ArgumentException(String.Format("Enum value {0} not found in the enum", _enumValue));
        }

        int previousIndex = (currentIndex - 1 + values.Length) % values.Length;
        return values[previousIndex];
    }

    //next value in enum
    public static T incrementIndex<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum",
                                                       typeof(T).FullName));
        }

        T[] Arr = (T[])Enum.GetValues(_enumValue.GetType());
        int currentIndex = Array.IndexOf<T>(Arr, _enumValue);

        if (currentIndex < Arr.Length - 1)
        {
            return Arr[currentIndex + 1];
        }

        return Arr[currentIndex];
    }

    public class BoundedIndex
    {
        private int _value;
        private readonly int _min;
        private readonly int _max;
        public int Value => _value;

        public bool isInMax() => _value == _max;

        public void setIndex(int _iIndex)
        {
            Debug.Assert(_iIndex >= _min && _iIndex <= _max);
            
            _value = _iIndex;
        }
        public BoundedIndex(int min, int max)
        {
            _min = min;
            _max = max;
            _value = min;
        }
        public bool tryToIncrement()
        {
            bool bSuccess = _value + 1 <= _max;
            _value = Math.Clamp(_value + 1, _min, _max);
            return bSuccess;
        }
        public bool TryToDecrement()
        {
            bool bSuccess = _value - 1 >= _min;
            _value = Math.Clamp(_value - 1, _min, _max);
            return bSuccess;
        }

        public void setIndexToLast() => _value = _max;
    }
    //prev value in enum
    public static T decreaseIndex<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        T[] values = (T[])Enum.GetValues(_enumValue.GetType());

        int currentIndex = Array.IndexOf(values, _enumValue);
        if (currentIndex == -1)
        {
            throw new ArgumentException(String.Format("Enum value {0} not found in the enum", _enumValue));
        }

        if (currentIndex > 0)
        {
            return values[currentIndex - 1];
        }
        return values[currentIndex];
    }
    // get enum
    public static List<T> getEnumList<T>(this T _enumValue) where T : struct
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));
        }

        T[] values = (T[])Enum.GetValues(_enumValue.GetType());

        return values.ToList();
    }

    public static List<string> getStringsList<T>(this List<T> _aoList)
    {
        List<string> aoStringList = new List<string>(_aoList.Count);
        foreach (T item in _aoList)
        {
            aoStringList.Add(item.ToString());
        }
        return aoStringList;
    }

    public static List<string> getStringsList<T>(this T[] _aoArray, string _sNullName)
    {
        List<string> aoStringList = new List<string>(_aoArray.Length);
        foreach (T oItem in _aoArray)
        {
            aoStringList.Add(oItem?.ToString() ?? _sNullName);
        }
        return aoStringList;
    }

    public static List<string> getNameList(this List<GameObject> _aoList)
    {
        List<string> aoNameList = new List<string>(_aoList.Count);
        foreach (GameObject oItem in _aoList)
        {
            aoNameList.Add(oItem.name);
        }
        return aoNameList;
    }

    public static List<string> getNameList(this GameObject[] _aoList)
    {
        List<string> aoNameList = new List<string>(_aoList.Length);
        foreach (GameObject oItem in _aoList)
        {
            aoNameList.Add(oItem.name);
        }
        return aoNameList;
    }

    public static List<string> getOwnerNameList<T>(this List<T> _aoList) where T : MonoBehaviour
    {
        List<string> aoOwnerNameList = new List<string>(_aoList.Count);
        foreach (MonoBehaviour oItem in _aoList)
        {
            aoOwnerNameList.Add(oItem.gameObject.name);
        }
        return aoOwnerNameList;
    }

    public static List<string> getOwnerNameList<T>(this T[] _aoList) where T : MonoBehaviour
    {
        List<string> aoOwnerNameList = new List<string>(_aoList.Length);
        foreach (MonoBehaviour oItem in _aoList)
        {
            aoOwnerNameList.Add(oItem.gameObject.name);
        }
        return aoOwnerNameList;
    }

    public static T getNextCircular<T>(this List<T> _ao, int _iCurrent)
    {
        if (_iCurrent < _ao.Count - 1)
        {
            return _ao[_iCurrent + 1];
        }

        return _ao[0];
    }

    public static T getPreviousSafe<T>(this List<T> _ao, int _iCurrent)
    {
        if (_iCurrent > 0)
        {
            return _ao[_iCurrent - 1];
        }

        return _ao[_ao.Count - 1];
    }

    public static int incrementIndex<T>(this List<T> _ao, int _iCurrent)
    {
        if (_iCurrent < _ao.Count - 1)
        {
            return _iCurrent + 1;
        }

        return 0;
    }

    public static int decrementIndex<T>(this List<T> _ao, int _iCurrent)
    {
        if (_iCurrent > 0)
        {
            return _iCurrent - 1;
        }

        return _ao.Count - 1;
    }

    public static void copyTo<T>(this List<T> _ao, List<T> _aoTo)
    {
        for (int i = 0; i < _ao.Count; ++i)
        {
            _aoTo.Add(_ao[i]);
        }
    }

    public static List<T> copy<T>(this List<T> _ao)
    {
        List<T> aoTo = new List<T>();
        for (int i = 0; i < _ao.Count; ++i)
        {
            aoTo.Add(_ao[i]);
        }
        return aoTo;
    }

    public static List<T> addRangeOrInitialize<T>(this List<T> _ao1, List<T> _ao2)
    {
        if (_ao1 == null)
        {
            return _ao2;
        }
        else
        {
            _ao1.AddRange(_ao2);
            return _ao1;
        }
    }

    public static bool addIfNotContains<T>(this List<T> _ao, T _o)
    {
        if (_ao.Contains(_o))
        {
            return false;
        }

        _ao.Add(_o);
        return true;
    }

    public static void addRangeIfNotContains<T>(this List<T> _ao, List<T> _o)
    {
        for (int i = 0; i < _o.Count; i++)
        {
            if (!_ao.Contains(_o[i]))
            {
                _ao.Add(_o[i]);
            }
        }
    }

    public static void mergeListsAND<T>(this List<T> _ao, List<T> _aoTo, ref bool _bInitializeIfEmpty)
    {
        if (_bInitializeIfEmpty && _ao.isNullOrEmpty())
        {
            _ao.AddRange(_aoTo);
            _bInitializeIfEmpty = false;
            return;
        }

        mergeListsAND(_ao, _aoTo);
    }

    public static void mergeListsAND<T>(this List<T> _ao, List<T> _aoTo)
    {
        // Convert the second list to a HashSet for faster lookup
        HashSet<T> set = new HashSet<T>(_aoTo);

        // Use RemoveAll to efficiently remove non-matching elements
        _ao.RemoveAll(item => !set.Contains(item));
    }

    public static bool contains<T>(this T[] _oa, T _o)
    {
        if (_oa == null && _o == null) { return true; }

        for (int i = 0; i < _oa.Length; ++i)
        {
            if (_oa[i].Equals(_o))
            {
                return true;
            }
        }
        return false;
    }

    public static T[] resize<T>(this T[] _oa, T _o)
    {
        Array.Resize(ref _oa, _oa.Length + 1);
        _oa[_oa.Length - 1] = _o;
        return _oa;
    }

    public static int indexOf<T>(this T[] _oa, T _o)
    {
        for (int i = 0; i < _oa.Length; ++i)
        {
            if (_oa[i].Equals(_o))
            {
                return i;
            }
        }
        return -1;
    }

    public static void populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    // get elements present in previous that are not present in current
    public static List<T> getMissingElements<T>(this List<T> _aoResults, List<T> _aoPrevious, List<T> _aoCurrent)
    {
        for (int i = 0; i < _aoPrevious.Count; i++)
        {
            if (!_aoCurrent.Contains(_aoPrevious[i]))
            {
                _aoResults.Add(_aoPrevious[i]);
            }
        }
        return _aoResults;
    }

    public static Vector2[] toVector2XZ(this Vector3[] _av3)
    {
        Vector2[] av2 = new Vector2[_av3.Length];
        for (int i = 0; i < _av3.Length; i++)
        {
            av2[i] = _av3[i].xz();
        }
        return av2;
    }
    public static Vector3[] toVector3YZero(this List<Vector2> _av3)
    {
        Vector3[] av2 = new Vector3[_av3.Count];
        for (int i = 0; i < _av3.Count; i++)
        {
            av2[i] = _av3[i].toVector3xy();
        }
        return av2;
    }

    public static void copyTo<T>(this T[] _oArray, int _iSourceIndex, int _iDestinationIndex)
    {
        Debug.Assert(_iSourceIndex >= _iDestinationIndex, "_iSourceIndex must be higher than _iDestinationIndex so there is enough room to copy");
        _oArray.AsSpan(_iSourceIndex).CopyTo(_oArray.AsSpan(_iDestinationIndex));
    }

    //Replaces all occurences of _oOriginal in _aoElements with _oReplacement
    public static void replace<T>(this Span<T> _aoElements, T _oOriginal, T _oReplacement) where T : IEquatable<T>
    {
        int iCursor = MemoryExtensions.IndexOf(_aoElements, _oOriginal);
        while (iCursor != -1)
        {
            _aoElements[iCursor] = _oReplacement;
            int iNextCursor = MemoryExtensions.IndexOf(_aoElements.Slice(iCursor), _oOriginal);
            iCursor = iNextCursor != -1 ? iCursor + iNextCursor : -1;
        }
    }

    //Replaces in _aoElements all occurences of _oOriginal by _oReplacement. _oReplacement must be of equal of smaller length than _oOriginal.
    //In the later case, in-between data will be shifted towards the start of _aoElements. 
    public static Span<T> replace<T>(this Span<T> _aoElements, ReadOnlySpan<T> _oOriginal, ReadOnlySpan<T> _oReplacement) where T : IEquatable<T>
    {
        int iOriginalLength = _oOriginal.Length;
        int iReplacementLength = _oReplacement.Length;
        int iLengthDelta = iOriginalLength - iReplacementLength;
        Debug.Assert(iLengthDelta >= 0, "_oOriginal is smaller than _oReplacement, so there is enough room in _aoElements");
        bool bReplacementIsSmaller = iLengthDelta > 0;
        int iRemovedElementsCount = 0;
        //Position where to start the search for the next _oOriginal occurrence
        int iCursor = MemoryExtensions.IndexOf(_aoElements, _oOriginal);
        //Position where the next block of data between the current and next _oOriginal occurrence will be witten at
        int iInsertCursor = iCursor;
        while (iCursor != -1)
        {
            int iReplacePosition = iCursor + iLengthDelta;
            //First make a right-justified replacement. It will be later shifted left if needed
            _oReplacement.CopyTo(_aoElements.Slice(iReplacePosition));
            iCursor += iOriginalLength;
            int iNextCursor = MemoryExtensions.IndexOf(_aoElements.Slice(iCursor), _oOriginal);
            if (bReplacementIsSmaller)
            {
                //Shift left the block between the current and next replacement
                if (iNextCursor == -1)
                {
                    //Tail of the collection, shift left any remaining elements
                    _aoElements.Slice(iReplacePosition).CopyTo(_aoElements.Slice(iInsertCursor));
                }
                else
                {
                    int iElementsToMove = iNextCursor + iReplacementLength;
                    _aoElements.Slice(iReplacePosition, iElementsToMove).CopyTo(_aoElements.Slice(iInsertCursor));
                    iInsertCursor += iElementsToMove;
                }
                iRemovedElementsCount += iLengthDelta;
            }
            iCursor = iNextCursor != -1 ? iCursor + iNextCursor : -1;
        }
        return _aoElements.Slice(0, _aoElements.Length - iRemovedElementsCount);
    }

    //Removes all occurrences of _oToRemove in _aoElements, shifting following elements left 
    public static Span<T> removeAll<T>(this Span<T> _aoElements, T _oToRemove) where T : IEquatable<T>
    {
        int iRemovedElementsCount = 0;
        int iCursor = MemoryExtensions.IndexOf(_aoElements, _oToRemove);
        while (iCursor != -1)
        {
            int iRemovePosition = iCursor;
            ++iCursor;
            int iNextCursor = MemoryExtensions.IndexOf(_aoElements.Slice(iCursor), _oToRemove);
            //Shift left the block between the current and next occurrence of _oToRemove
            if (iNextCursor == -1)
            {
                //Tail of the collection, shift left any remaining elements
                _aoElements.Slice(iCursor).CopyTo(_aoElements.Slice(iRemovePosition));
            }
            else
            {
                _aoElements.Slice(iCursor, iNextCursor).CopyTo(_aoElements.Slice(iRemovePosition));
            }
            ++iRemovedElementsCount;

            iCursor = iNextCursor != -1 ? iCursor + iNextCursor : -1;
        }
        return _aoElements.Slice(0, _aoElements.Length - iRemovedElementsCount);
    }
    #endregion

    #region ENUMS
    public static bool contains(this Enum _e, Enum _eValue)
    {
        return _e.HasFlag(_eValue);
    }
    #endregion

    #region INTS & FLOATS
    public static bool isBetween(this float _f, float _fA, float _fB)
    {
        return (_f >= _fA && _f <= _fB) || (_f <= _fA && _f >= _fB);
    }

    public static bool isBetween(this int _i, float _fA, float _fB)
    {
        return (_i >= _fA && _i <= _fB) || (_i <= _fA && _i <= _fB);
    }

    public static bool approximately(this float a, float b, float tolerance)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    public static bool approximately(this float a, float b)
    {
        return a.approximately(b, 0.0001f);
    }

    public static float toFloor(this float a)
    {
        return Mathf.Floor(a);
    }

    public static float toCeil(this float a)
    {
        return Mathf.Ceil(a);
    }

    public static float clamp(this float v, float a, float b)
    {
        return Mathf.Clamp(v, a, b);
    }

    public static float distance(this float _f1, float _f2)
    {
        return Mathf.Abs(_f2 - _f1);
    }

    public static float abs(this float _f)
    {
        return Mathf.Abs(_f);
    }
    #endregion

    #region VECTOR HELPERS
    public static Vector3 toVector3xy(this Vector2 v)
    {
        return new Vector3(v.x, v.y, 0.0f);
    }

    public static Vector3 toVector3xz(this Vector2 v)
    {
        return new Vector3(v.x, 0.0f, v.y);
    }

    public static Vector3 toVector3zy(this Vector2 v)
    {
        return new Vector3(0.0f, v.y, v.x);
    }

    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 toVector3xy(this Vector2 _v, float _fZ)
    {
        return new Vector3(_v.x, _v.y, _fZ);
    }

    public static Vector3 toVector3xz(this Vector2 _v, float _fY)
    {
        return new Vector3(_v.x, _fY, _v.y);
    }

    public static Vector3 getRandomComponents(Vector3 _vMin, Vector3 _vMax, string _sSeed, string _sContext)
    {
        return new Vector3(RandomUtils.range(_sSeed, _vMin.x, _vMax.x, _sContext), RandomUtils.range(_sSeed, _vMin.y, _vMax.y, _sContext), RandomUtils.range(_sSeed, _vMin.z, _vMax.z, _sContext));
    }

    public static bool isBetween(this Vector3 _v, Vector3 _vMin, Vector3 _vMax)
    {
        return _v.x.isBetween(_vMin.x, _vMax.x) && _v.y.isBetween(_vMin.y, _vMax.y) && _v.z.isBetween(_vMin.z, _vMax.z);
    }

    public static bool hasBetween(this Vector2 _v, float _fValue)
    {
        return _fValue >= _v.x && _fValue <= _v.y;
    }

    public static float getPercentage(this Vector2 _v, float _fPercentageXtoY)
    {
        return _v.x + ((_v.y - _v.x) * _fPercentageXtoY);
    }

    public static float clamp(this Vector2 _vLimits, float _fValue)
    {
        return Mathf.Clamp(_fValue, _vLimits.x, _vLimits.y);
    }

    public static bool equals(this Vector2 _v1, Vector2 _v2)
    {
        return (_v2 - _v1).magnitude < Mathf.Epsilon;
    }

    public static float cross(this Vector2 _v1, Vector2 _v2)
    {
        return (_v1.x * _v2.y) - (_v1.y * _v2.x);
    }

    public static Vector3 addValueToAllAxis(this Vector3 _v, float _fValue)
    {
        return new Vector3(_v.x + _fValue, _v.y + _fValue, _v.z + _fValue);
    }
    #endregion

    #region COLORS
    public static Color toColor(this Vector3 _v)
    {
        return new Color(_v.x, _v.y, _v.z, 1.0f);
    }
    public static void setAlpha(this Image _o, float _f)
    {
        _o.color = new Color(_o.color.r, _o.color.g, _o.color.b, _f);
    }


    #endregion

    #region VECTOR MATH
    public static Vector3 multiply(this Vector3 _a, Vector3 _b)
    {
        return new Vector3(_a.x * _b.x, _a.y * _b.y, _a.z * _b.z);
    }

    public static Vector3 divide(this Vector3 _a, Vector3 _b)
    {
        return new Vector3(_a.x / _b.x, _a.y / _b.y, _a.z / _b.z);
    }

    public static float distanceXZ(this Vector3 _v1, Vector3 _v2)
    {
        return Vector2.Distance(_v1.xz(), _v2.xz());
    }

    public static Vector2 directionXZ(this Vector3 _vOrigin, Vector3 _vTarget)
    {
        return (_vTarget.xz() - _vOrigin.xz()).normalized;
    }

    public static float sqrDistance(this Vector3 _v1, Vector3 _v2)
    {
        return Vector3.SqrMagnitude(_v2 - _v1);
    }

    public static float distance(this Vector3 _v1, Vector3 _v2)
    {
        return Vector3.Distance(_v1, _v2);
    }

    public static float sqrDistanceXZ(this Vector3 _v1, Vector3 _v2)
    {
        float x = _v1.x - _v2.x;
        float z = _v1.z - _v2.z;
        return (x * x) + (z * z);
    }

    public static float distance(this Vector2 _v1, Vector2 _v2)
    {
        return Vector2.Distance(_v1, _v2);
    }

    public static float sqrDistance(this Vector2 _v1, Vector2 _v2)
    {
        float x = _v1.x - _v2.x;
        float y = _v1.y - _v2.y;
        return (x * x) + (y * y);
    }

    public static Vector3 randomDirectionXZ(this Vector3 _vDirection, float _fAngle, string _sSeed = null)
    {
        return _vDirection.rotateClockwiseXZ((RandomUtils.value(_sSeed, "") * _fAngle) - (_fAngle * 0.5f));
    }

    public static Vector3 randomDirectionXZ(this Vector3 _vDirection, float _fMinAngle, float _fMaxAngle, string _sSeed, string _sContext)
    {
        //float fValidAngle = _fMaxAngle - _fMinAngle;

        // choose randomly left or right
        float fAngle = RandomUtils.value(_sSeed, "") > 0.5f ? RandomUtils.range(_sSeed, _fMinAngle, _fMaxAngle, _sContext) : -RandomUtils.range(_sSeed, _fMinAngle, _fMaxAngle, _sContext);

        return _vDirection.rotateClockwiseXZ(fAngle);
    }

    public static Vector3 rotateAroundAxis(this Vector3 _v, Vector3 _vUnitAxis, float _fAngleRadians)
    {
        float fCos = Mathf.Cos(_fAngleRadians);
        float fSin = Mathf.Sin(_fAngleRadians);
        return (_v * fCos) + (Vector3.Cross(_vUnitAxis, _v) * fSin) + (_vUnitAxis * (Vector3.Dot(_vUnitAxis, _v) * (1.0f - fCos)));
    }

    public static Vector2 rotateClockwise(this Vector2 _v, float _fAngleDegrees)
    {
        float fAngleRadians = -_fAngleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(fAngleRadians);
        float sin = Mathf.Sin(fAngleRadians);
        return new Vector2((_v.x * cos) - (_v.y * sin), (_v.x * sin) + (_v.y * cos));
    }

    public static Vector2 rotateTowards(this Vector2 _vFrom, Vector2 _vTo, float _fMaxDegrees)
    {
        float fAngle = Vector2.SignedAngle(_vFrom, _vTo);

        if (fAngle.approximately(0.0f))
        {
            return _vFrom;
        }

        if (fAngle > 0.0f)
        {
            if (_fMaxDegrees > fAngle)
            {
                return _vFrom.rotateClockwise(-fAngle);
            }
            else
            {
                return _vFrom.rotateClockwise(-_fMaxDegrees);
            }
        }
        else
        {
            if (-_fMaxDegrees < fAngle)
            {
                return _vFrom.rotateClockwise(-fAngle);
            }
            else
            {
                return _vFrom.rotateClockwise(_fMaxDegrees);
            }
        }
    }

    public static Vector3 rotateClockwiseXZ(this Vector3 _v, float _fAngleDegrees)
    {
        return Quaternion.AngleAxis(_fAngleDegrees, Vector3.up) * _v;
    }

    public static float signedAngle(this Vector2 _v1, Vector2 _v2)
    {
        // UNITY signed angle returns angle counterclockwise so negate it
        return -Vector2.SignedAngle(_v1, _v2);
    }

    public static float unsignedAngle(this Vector2 _v1, Vector2 _v2)
    {
        return Vector2.Angle(_v1, _v2);
    }

    public static int combinationsOfN(this int _iN)
    {
        int iResult = 1;
        for (int i = _iN; i > 0; i--)
        {
            iResult *= i;
        }
        return iResult;
    }

    #endregion

    #region BILINEAR INTERPOLATION
    //  A-----------------B
    //  |                 |
    //  |                 |
    //  |                 |
    //  |  *(u,v)         |
    //  |                 |
    //  D-----------------C
    // 
    // given a rectangle formed by A B C D points have to be in same plane and be axis alligned, with coordinates 0,0 / 0,1 / 1,0 / 1,1

    public static Vector3 bilinearInterpolation(Vector2 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vector3 abx = Vector3.Lerp(a, b, p.x);
        Vector3 dcx = Vector3.Lerp(d, c, p.x);
        return Vector3.Lerp(abx, dcx, p.y);
    }

    public static Vector2 bilinearInterpolation(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Vector2 abx = Vector2.Lerp(a, b, p.x);
        Vector2 dcx = Vector2.Lerp(d, c, p.x);
        return Vector2.Lerp(abx, dcx, p.y);
    }

    public static float bilinearInterpolation(Vector2 p, float a, float b, float c, float d)
    {
        float abx = Mathf.Lerp(a, b, p.x);
        float dcx = Mathf.Lerp(d, c, p.x);
        return Mathf.Lerp(abx, dcx, p.y);
    }

    public static float interpolateRectangleCorners(this Vector2 p, Vector2 _vA, Vector2 _vB, Vector2 _vC, Vector2 _vD, float _f1, float _f2, float _f3, float _f4)
    {
        Vector2 vNormalizedPoint = inverseBilinear(p, _vA, _vB, _vC, _vD);
        return bilinearInterpolation(vNormalizedPoint, _f1, _f2, _f3, _f4);
    }

    // used to convert a point inside 4 points to coordinates that consider those vectors are normalized and axis-alligned, so we can then use bilinear interpolation
    public static Vector2 inverseBilinear(in Vector2 p, in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d)
    {
        Vector2 vResult;

        Vector2 e = b - a;
        Vector2 f = d - a;
        Vector2 g = a - b + c - d;
        Vector2 h = p - a;

        float k2 = g.cross(f);
        float k1 = e.cross(f) + h.cross(g);
        float k0 = h.cross(e);

        // if edges are parallel, this is a linear equation
        if (abs(k2) < 0.001)
        {
            vResult = new Vector2(((h.x * k1) + (f.x * k0)) / ((e.x * k1) - (g.x * k0)), -k0 / k1);
        }
        // otherwise, it's a quadratic
        else
        {
            float w = (k1 * k1) - (4.0f * k0 * k2);
            if (w < 0.0)
            {
                return new Vector2(-99999, -99999);
            }
            w = Mathf.Sqrt(w);

            float ik2 = 0.5f / k2;
            float v = (-k1 - w) * ik2;
            float u = (h.x - (f.x * v)) / (e.x + (g.x * v));

            if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0)
            {
                v = (-k1 + w) * ik2;
                u = (h.x - (f.x * v)) / (e.x + (g.x * v));
            }
            vResult = new Vector2(u, v);
        }

        return vResult;
    }
    #endregion

    #region SERIALIZE
    public static string tryGetString(this Dictionary<string, object> _ao, string _s, string _sDefault = "")
    {
        if (_ao.ContainsKey(_s))
        {
            return (string)_ao[_s];
        }
        return _sDefault;
    }

    public static int tryGetInt(this Dictionary<string, object> _ao, string _s, int _iDefault = 0)
    {
        if (_ao.ContainsKey(_s))
        {
            return (int)_ao[_s];
        }
        return _iDefault;
    }

    public static float tryGetFloat(this Dictionary<string, object> _ao, string _s, float _fDefault = 0)
    {
        if (_ao.ContainsKey(_s))
        {
            return (float)_ao[_s];
        }
        return _fDefault;
    }

    public static bool tryGetBool(this Dictionary<string, object> _ao, string _s, bool _bDefault = false)
    {
        if (_ao.ContainsKey(_s))
        {
            return (bool)_ao[_s];
        }
        return _bDefault;
    }

    public static bool isBasic(this Type _oType)
    {
        return _oType.IsPrimitive || _oType == typeof(string) || _oType.IsEnum;
    }
    #endregion

    #region ANIMATIONS

#if UNITY_EDITOR
    public static bool getState(this Animator _oAnimator, string _sName, out AnimatorState _sState)
    {
        _sState = new AnimatorState();

        AnimatorController ac = _oAnimator.runtimeAnimatorController as AnimatorController;

        foreach (AnimatorControllerLayer acl in ac.layers)
        {
            if (acl.stateMachine.getState(_sName, out _sState))
            {
                return true;
            }
        }

        return false;
    }

    public static bool hasState(this Animator _oAnimator, string _sName, int _iLayer = 0)
    {
        if (_oAnimator == null)
        {
            return false;
        }

        if (_sName.isNullOrEmpty())
        {
            return false;
        }

        if (_iLayer < 0)
        {
            return false;
        }

        if (!(_oAnimator.runtimeAnimatorController is AnimatorController oAnimatorController))
        {
            return false;
        }

        if (_iLayer >= oAnimatorController.layers.Length)
        {
            return false;
        }

        ChildAnimatorState[] aoStates = oAnimatorController.layers[_iLayer].stateMachine.states;
        if (aoStates == null || aoStates.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < aoStates.Length; ++i)
        {
            if (aoStates[i].state != null && aoStates[i].state.name == _sName)
            {
                return true;
            }
        }

        return false;
    }

    public static bool hasState(this Animator _oAnimator, int _iHash, int _iLayer = 0)
    {
        AnimatorController ac = _oAnimator.runtimeAnimatorController as AnimatorController;

        ChildAnimatorState[] aoCAS = ac.layers[_iLayer].stateMachine.states;
        for (int i = 0; i < aoCAS.Length; ++i)
        {
            if (aoCAS[i].state.nameHash == _iHash)
            {
                return true;
            }
        }

        return false;
    }

    public static bool getState(this AnimatorStateMachine _oStateMachine, string _sName, out AnimatorState _sState)
    {
        _sState = new AnimatorState();

        foreach (ChildAnimatorState cas in _oStateMachine.states)
        {
            if (cas.state.name.Equals(_sName))
            {
                //Deb.log("Anim: " + _sName);
                _sState = cas.state;
                return true;
            }
        }

        foreach (ChildAnimatorStateMachine cas in _oStateMachine.stateMachines)
        {
            if (cas.stateMachine.getState(_sName, out _sState))
            {
                return true;
            }
        }

        return false;
    }

#endif

    public static float getStateDuration(this Animator _oAnimator, string _sAnimation)
    {
#if UNITY_EDITOR
        AnimatorState state;
        if (_oAnimator.getState(_sAnimation, out state))
        {
            AnimationClip ac = state.motion as AnimationClip;
            if (state.motion is BlendTree)
            {
                BlendTree bt = state.motion as BlendTree;
                ac = bt.children[0].motion as AnimationClip;
                //Deb.log("Min: " + ac.name);
            }
            else
            {
                ac = state.motion as AnimationClip;
            }

            if (ac != null)
            {
                return ac.length;
            }
            else
            {
                return 0.0f;
            }
        }
        return 0.0f;
#else
        int stateHash = Animator.StringToHash(_sAnimation);
    
        // First check if the state exists
        if (!_oAnimator.HasState(0, stateHash))
            return 0f;
            
        // Option 1: If currently in this state, you can get info directly
        if (_oAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash)
            return _oAnimator.GetCurrentAnimatorStateInfo(0).length;

        return 0f;
#endif
    }

    public static string getPath(this Transform _oTransform)
    {
        if (_oTransform.parent.isNullOrDestroyed()) return _oTransform.name;
        return _oTransform.parent.getPath() + "/" + _oTransform.name;
    }

    public static void play(this Animator _oAnimator, string _sAnimation, bool _bDontPlayIfAlreadyPlaying = false, int _iLayer = 0, float _fTime = 0.0f)
    {
#if UNITY_EDITOR
        if (!_oAnimator.hasState(_sAnimation, _iLayer))
        {
            Deb.logWarning("Animator '" + _oAnimator.name + "' tried to play animation '" + _sAnimation + "' but it doesn't have it. " + _oAnimator.transform.getPath());
        }
#endif
        {
            if (_bDontPlayIfAlreadyPlaying && _oAnimator.isThisAnimationPlayingOrQueuedAndFinished(_sAnimation))
            {
                return;
            }

            if (_oAnimator.gameObject.activeInHierarchy)
            {
                _oAnimator.Play(_sAnimation, _iLayer, _fTime);
            }
        }
    }

    // IMPORTANT TO KNOW: GetNextAnimatorStateInfo returns the next queued state (if there is a transition)
    public static bool isThisAnimationPlayingOrQueuedAndFinished(Animator animator, string animation, bool checkAnimationEnded = false)
    {
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(0);

        // Check if the next state matches
        if (nextStateInfo.shortNameHash != 0 && nextStateInfo.IsName(animation) && (!checkAnimationEnded || nextStateInfo.normalizedTime > 0.999f))
        {
            return true;
        }

        // Check if the current state matches
        if (currentInfo.IsName(animation) && (!checkAnimationEnded || currentInfo.normalizedTime > 0.999f))
        {
            // Next state is different?
            if (nextStateInfo.shortNameHash != 0 && !nextStateInfo.IsName(animation))
            {
                return false;
            }
            return true;
        }
        return false;
    }

    #endregion

    #region PHYSICS 3D
    public static Vector3 bounceVector(this Vector3 _vVelocity, Vector3 _vNormal)
    {
        Vector3 u = Vector3.Dot(_vVelocity, _vNormal) / Vector3.Dot(_vNormal, _vNormal) * _vNormal;
        Vector3 w = _vVelocity - u;
        return w - u;
    }

    public static Vector3 bounceVelocity(this Vector3 _vVelocity, Vector3 _vNormal, float _fRestitution)
    {
        _vVelocity = _vVelocity.bounceVector(_vNormal);
        _vVelocity *= _fRestitution;
        return _vVelocity;
    }

    #endregion

    #region PHYSICS 2D
    public static Vector2 bounceVector(this Vector2 _vVelocity, Vector2 _vNormal)
    {
        Vector2 u = Vector2.Dot(_vVelocity, _vNormal) / Vector2.Dot(_vNormal, _vNormal) * _vNormal;
        Vector2 w = _vVelocity - u;
        return w - u;
    }

    public static Vector2 bounceVelocity(this Vector2 _vVelocity, Vector2 _vNormal, float _fRestitution)
    {
        _vVelocity = _vVelocity.bounceVector(_vNormal);
        _vVelocity *= _fRestitution;
        return _vVelocity;
    }

    public static RaycastHit2D circleCastFirstTo(this Vector2 _vFrom, Vector2 _vTo, float _fRadius, LayerMask _oLayer)
    {
        Vector2 vDirection = _vTo - _vFrom;
        return Physics2D.CircleCast(_vFrom, _fRadius, vDirection.normalized, vDirection.magnitude, _oLayer);
    }

    public static RaycastHit2D[] m_aoHits = new RaycastHit2D[10];
    public static int m_iHits;
    public static bool raycastTo(Vector2 _vFrom, Vector2 _vTo, LayerMask _oLayer)
    {
        Vector2 vToFrom = _vTo - _vFrom;
        m_iHits = Physics2D.RaycastNonAlloc(_vFrom, vToFrom.normalized, m_aoHits, vToFrom.magnitude, _oLayer);
        return m_iHits > 0;
    }
    public static bool raycast(Vector2 _vFrom, Vector2 _vMoved, LayerMask _oLayer)
    {
        m_iHits = Physics2D.RaycastNonAlloc(_vFrom, _vMoved.normalized, m_aoHits, _vMoved.magnitude, _oLayer);
        return m_iHits > 0;
    }

    public static bool raycast(Vector2 _vFrom, Vector2 _vDirection, LayerMask _oLayer, out RaycastHit2D _vFirstHit)
    {
        m_iHits = Physics2D.RaycastNonAlloc(_vFrom, _vDirection.normalized, m_aoHits, float.MaxValue, _oLayer);
        if (m_iHits > 0)
        {
            _vFirstHit = m_aoHits[0];
        }
        else
        {
            _vFirstHit = new RaycastHit2D();
        }

        return m_iHits > 0;
    }

    public static bool circleCastTo(this Vector2 _vFrom, Vector2 _vTo, float _fRadius, LayerMask _oLayer)
    {
        Vector2 vToFrom = _vTo - _vFrom;
        m_aoHits = Physics2D.CircleCastAll(_vFrom, _fRadius, vToFrom.normalized, vToFrom.magnitude, _oLayer);
        m_iHits = m_aoHits.Length;
        return m_iHits > 0;
    }

    public static bool circleCast(this Vector2 _vFrom, Vector2 _vMoved, float _fRadius, LayerMask _oLayer)
    {
        m_aoHits = Physics2D.CircleCastAll(_vFrom, _fRadius, _vMoved.normalized, _vMoved.magnitude, _oLayer);
        m_iHits = m_aoHits.Length;
        return m_iHits > 0;
    }

    public static RaycastHit2D boxcast(Vector2 _vFrom, Vector2 _vTranslation, Vector2 _vSize, int _iLayerMask)
    {
        return Physics2D.BoxCast(_vFrom, _vSize, 0.0f, _vTranslation.normalized, _vTranslation.magnitude, _iLayerMask);
    }

    public static int boxcastFromToAll(Vector2 _vFrom, Vector2 _vTo, Vector2 _vSize, int _iLayerMask, RaycastHit2D[] _aoHits = null)
    {
        return boxcastAll(_vFrom, _vTo - _vFrom, _vSize, _iLayerMask, _aoHits);
    }
    public static int boxcastAll(Vector2 _vPosition, Vector2 _vTranslation, Vector2 _vSize, int _iLayerMask, RaycastHit2D[] _aoHits = null)
    {
        return boxcastAll(_vPosition, _vSize, _vTranslation.normalized, _vTranslation.magnitude, _iLayerMask, _aoHits);
    }
    public static int boxcastAll(Vector2 _vPosition, Vector2 _vSize, Vector2 _vDirection, float _fDistance, int _iLayerMask, RaycastHit2D[] _aoHits = null)
    {
        int i = 0;
        if (_aoHits == null) _aoHits = m_aoHits;
        _aoHits = Physics2D.BoxCastAll(
            _vPosition,
            _vSize,
            0.0f,
            _vDirection,
            _fDistance,
            _iLayerMask
        );
        i = _aoHits.Length;
        return i;
    }

    public static float getSqrDistanceToSegment(this Vector2 p, Vector2 v1, Vector2 v2)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = Vector2.SqrMagnitude(v1 - v2);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2.approximately(0.0f))
        {
            return Vector2.SqrMagnitude(p - v1);   // v == w case
        }
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Mathf.Max(0.0f, Mathf.Min(1.0f, Vector2.Dot(p - v1, v2 - v1) / l2));
        Vector2 projection = v1 + (t * (v2 - v1));  // Projection falls on the segment
        return (p - projection).sqrMagnitude;
    }

    public static float getDistancePointToSegment(this Vector2 p, Vector2 v, Vector2 w)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = Vector2.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2.approximately(0.0f))
        {
            return Vector2.Distance(p, v);   // v == w case
        }
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Mathf.Max(0.0f, Mathf.Min(1.0f, Vector2.Dot(p - v, w - v) / l2));
        Vector2 projection = v + (t * (w - v));  // Projection falls on the segment
        return Vector2.Distance(p, projection);
    }

    // returns true if the point is close enough to be considered part of the segment
    public static bool isPointInsideSegment(this Vector2 p, Vector2 v, Vector2 w)
    {
        float fScalarProduct = Vector2.Dot((p - v).normalized, (w - v).normalized);

        if (Mathf.Abs(fScalarProduct) < 0.99) { return false; }

        return Vector2.Dot((p - v).normalized, (p - w).normalized) < 0;
    }

    // return distance from the point to the nearest segment of a point shape, even if the point is inside the shape
    public static float getSqrDistanceToShape(this Vector2 p, Vector2[] _avShapePoints)
    {
        float fMinimumDistance = float.MaxValue;

        Vector2 v1, v2;
        float fDistance;
        for (int i = 0; i < _avShapePoints.Length; ++i)
        {
            if (i == 0)
            {
                v1 = _avShapePoints[^1];
                fDistance = p.getSqrDistanceToSegment(v1, v1);
            }
            else
            {
                v1 = _avShapePoints[i - 1];
                v2 = _avShapePoints[i];
                fDistance = p.getSqrDistanceToSegment(v1, v2);
            }

            if (fDistance < fMinimumDistance)
            {
                fMinimumDistance = fDistance;
            }
        }

        return fMinimumDistance;
    }

    public static float getDistanceToShape(this Vector2 p, Vector2[] _avShapePoints)
    {
        float fSqrDistance = p.getSqrDistanceToShape(_avShapePoints);
        return Mathf.Sqrt(fSqrDistance);
    }
    public static Vector2 getClosestPointToSegment(this Vector2 point, Vector2 start, Vector2 end, float _fSnapDistance = 0)
    {
        Vector2 segmentDirection = end - start;
        float segmentLengthSquared = segmentDirection.sqrMagnitude;

        // If the segment has zero length, return the start point
        if (segmentLengthSquared <= 0f)
        {
            return start;
        }

        Vector2 pointDirection = point - start;

        // Calculate the normalized distance along the segment
        float t = Mathf.Clamp01(Vector2.Dot(pointDirection, segmentDirection) / segmentLengthSquared);

        // Calculate the closest point on the segment
        Vector2 closestPoint = start + (t * segmentDirection);

        if (_fSnapDistance > 0)
        {
            if (Vector2.Distance(closestPoint, end) - _fSnapDistance <= 0f)
            {
                return end;
            }
            else if (Vector2.Distance(closestPoint, start) - _fSnapDistance <= 0f)
            {
                return start;
            }
        }
        return closestPoint;
    }

    public static List<Vector2> getPointsAlongPathByDistance(this Vector2[] _avPath, float _fDistance)
    {
        float fDistanceLeft = 0;
        List<Vector2> avPoints = new List<Vector2>();
        avPoints.Add(_avPath[^1]);
        for (int i = 0; i < _avPath.Length; i++)
        {
            Vector2 vInitialPoint;
            if (i == 0)
            {
                vInitialPoint = _avPath[^1];
            }
            else
            {
                vInitialPoint = _avPath[i - 1];
            }

            Vector2 vEndPoint = _avPath[i];
            Vector2 vSegmentDirection = (vEndPoint - vInitialPoint).normalized;

            float fSegmentLength = Vector2.Distance(vInitialPoint, vEndPoint);
            float fSegmentDone = 0;
            float fCurrentLength = _fDistance - fDistanceLeft;

            while (fSegmentDone + fCurrentLength <= fSegmentLength)
            {
                fSegmentDone += fCurrentLength;
                avPoints.Add(vInitialPoint + (vSegmentDirection * fSegmentDone));
                fCurrentLength = _fDistance;
            }
            fDistanceLeft = fSegmentLength - fSegmentDone;
        }
        //// debug
        //for (int i = 0; i < avPoints.Count; i++)
        //{
        //    //Deb.Instance.drawCircle(avPoints[i], 0.1f,12,Color.blue,10);
        //}
        return avPoints;
    }

    static List<Vector2> m_avCirclePointsTemp = new List<Vector2>(10);

    public static int isCircleInsideShape(this Vector2[] shapePoints, Vector2 circleCenter, float circleRadius)
    {
        getPointsOnCircle(ref m_avCirclePointsTemp, circleCenter, circleRadius);
        bool bAnyPointOutside = false;
        int iPointsInside = 0;
        for (int i = 0; i < m_avCirclePointsTemp.Count; i++)
        {
            if (shapePoints.isPointInsideShape(m_avCirclePointsTemp[i]))
            {
                iPointsInside++;
            }
            else
            {
                bAnyPointOutside = true;
            }
            if (bAnyPointOutside && iPointsInside > 0) // on contact
            {
                return 0;
            }
        }
        if (bAnyPointOutside)
        {
            return -1; // out of shape
        }
        return 1; // inside shape
    }

    // Checks if a point is inside a shape defined by an array of points
    public static bool isPointInsideShape(this Vector2[] shapePoints, Vector2 point)
    {
        int numPoints = shapePoints.Length;
        bool isInside = false;

        for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
        {
            Vector2 pi = shapePoints[i];
            Vector2 pj = shapePoints[j];

            // Check if the point is intersecting with the shape boundary
            if (((pi.y > point.y) != (pj.y > point.y)) &&
                (point.x < ((pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y)) + pi.x))
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    // Generates evenly distributed points on the circumference of a circle
    public static void getPointsOnCircle(ref List<Vector2> avNewPoints, Vector2 center, float radius, int numPoints = 8)
    {
        avNewPoints.Clear();
        if (radius.approximately(0))
        {
            avNewPoints.Add(center);
            return;
        }
        float angleIncrement = 360f / numPoints;
        Vector2 v = Vector2.zero;
        for (int i = 0; i < numPoints; i++)
        {
            float angle = i * angleIncrement;
            v.x = center.x + (radius * Mathf.Cos(Mathf.Deg2Rad * angle));
            v.y = center.y + (radius * Mathf.Sin(Mathf.Deg2Rad * angle));
            avNewPoints.Add(v);
        }
    }

    public static Vector2 getPointProjectionInSegment(this Vector2 p, Vector2 v, Vector2 w)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = Vector2.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2.approximately(0.0f))
        {
            return v;   // v == w case
        }
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Mathf.Max(0.0f, Mathf.Min(1.0f, Vector2.Dot(p - v, w - v) / l2));
        Vector2 projection = v + (t * (w - v));  // Projection falls on the segment
        return projection;
    }

    public static bool pointReached(Vector2 _vPreviousPos, Vector2 _vPos, Vector2 _vTarget, float _fDistance = 0.2f)
    {
        return _vTarget.getDistancePointToSegment(_vPreviousPos, _vPos) < _fDistance;
    }

    public static bool isIntersecting(Vector2 _v1s, Vector2 _v1e, Vector2 _v2s, Vector2 _v2e, ref Vector2 _vIntersectingPoint)
    {
        //Direction of the lines
        Vector2 l1_dir = (_v1e - _v1s).normalized;
        Vector2 l2_dir = (_v2e - _v2s).normalized;

        //If we know the direction we can get the normal vector to each line
        Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
        Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


        //Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
        //The normal vector is the A, B
        float A = l1_normal.x;
        float B = l1_normal.y;

        float C = l2_normal.x;
        float D = l2_normal.y;

        //To get k we just use one point on the line
        float k1 = (A * _v1s.x) + (B * _v1s.y);
        float k2 = (C * _v2s.x) + (D * _v2s.y);


        //Step 2: are the lines parallel? -> no solutions
        if (IsParallel(l1_normal, l2_normal))
        {
            //Deb.log("The lines are parallel so no solutions!");

            return false;
        }


        //Step 3: are the lines the same line? -> infinite amount of solutions
        //Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
        if (IsOrthogonal(_v1s - _v2s, l1_normal))
        {
            //Deb.log("Same line so infinite amount of solutions!");
            //Return false anyway
            return false;
        }


        //Step 4: calculate the intersection point -> one solution
        float x_intersect = ((D * k1) - (B * k2)) / ((A * D) - (B * C));
        float y_intersect = ((-C * k1) + (A * k2)) / ((A * D) - (B * C));

        _vIntersectingPoint = new Vector2(x_intersect, y_intersect);


        //Step 5: but we have line segments so we have to check if the intersection point is within the segment
        if (IsBetween(_v1s, _v1e, _vIntersectingPoint) && IsBetween(_v2s, _v2e, _vIntersectingPoint))
        {
            //Deb.log("We have an intersection point!");
            return true;
        }

        return false;
    }

    //Are 2 vectors parallel?
    static bool IsParallel(Vector2 v1, Vector2 v2)
    {
        //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
        if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
        {
            return true;
        }

        return false;
    }

    //Are 2 vectors orthogonal?
    static bool IsOrthogonal(Vector2 v1, Vector2 v2)
    {
        //2 vectors are orthogonal is the dot product is 0
        //We have to check if close to 0 because of floating numbers
        if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
        {
            return true;
        }

        return false;
    }

    //Is a point c between 2 other points a and b?
    static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
    {
        bool isBetween = false;

        //Entire line segment
        Vector2 ab = b - a;
        //The intersection and the first point
        Vector2 ac = c - a;

        //Need to check 2 things: 
        //1. If the vectors are pointing in the same direction = if the dot product is positive
        //2. If the length of the vector between the intersection and the first point is smaller than the entire line
        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
        {
            isBetween = true;
        }

        return isBetween;
    }

    public static Vector2 projectPointOnLine(Vector2 _vPoint, Vector2 _v1, Vector2 _v2)
    {
        float m = (float)(_v2.y - _v1.y) / (_v2.x - _v1.x);
        float b = (float)_v1.y - (m * _v1.x);

        float x = ((m * _vPoint.y) + _vPoint.x - (m * b)) / ((m * m) + 1);
        float y = ((m * m * _vPoint.y) + (m * _vPoint.x) + b) / ((m * m) + 1);

        return new Vector2(x, y);
    }

    public static bool projectsOnLine(Vector2 _vPoint, Vector2 _v1, Vector2 _v2)
    {
        float fPercentage = getProjectedPercentage(_vPoint, _v1, _v2);
        return fPercentage >= 0.0f && fPercentage <= 1.0f;
    }

    // project _vPoint over the segment formed by _vStart and _vEnd, and return the percentage of the projection across the line, it can be <0 and >1 if its not in the line
    public static float getProjectedPercentage(Vector2 _vPoint, Vector2 _vStart, Vector2 _vEnd)
    {
        Vector2 vProjection = projectPointOnLine(_vPoint, _vStart, _vEnd);
        float fDistanceStart = vProjection.distance(_vStart);
        float fDistanceEnd = vProjection.distance(_vEnd);

        float fLength = _vEnd.distance(_vStart);
        if (fDistanceEnd > fLength && fDistanceStart < fDistanceEnd) // the point is before start
        {
            return -fDistanceStart / fLength;
        }
        return fDistanceStart / fLength;
    }

    public static bool pointInRectangle(Vector2 _v2Point, Vector2 _v2RectangleCenter, Vector2 _v2Size, float _fRectangleAngle)
    {
        Vector2 v2PointRelative = (_v2Point - _v2RectangleCenter).rotateClockwise(-_fRectangleAngle);
        Vector2 v2HalfSize = _v2Size / 2f;
        return isBetween(v2PointRelative.x, -v2HalfSize.x, v2HalfSize.x) &&
            isBetween(v2PointRelative.y, -v2HalfSize.y, v2HalfSize.y);
    }

    public static bool circleVsCircle(Vector2 _v1, float _fr1, Vector2 _v2, float _fr2)
    {
        float fRadiuses = _fr1 + _fr2;
        return _v1.sqrDistance(_v2) < fRadiuses * fRadiuses;
    }

    public static bool circleVsRectangle(Vector2 _v1, float _fr1, Vector2 _v2, Vector2 _vSize)
    {
        return circleVsCircle(_v1, _fr1, _v2, (_vSize.x + _vSize.y) * 0.25f);
    }

    public static bool rectangleVsRectangle(Vector2 _v1, Vector2 _vSize1, Vector2 _v2, Vector2 _vSize2)
    {
        return false;
    }

    public static float applyAcceleration(this float _fCurrent, float _fDT, float _fAcceleration, float _fFriction)
    {
        float fTerminalSpeed = _fAcceleration / _fFriction;
        float fExp = Mathf.Exp(-_fFriction * _fDT);
        return fTerminalSpeed + ((_fCurrent - fTerminalSpeed) * fExp);
    }

    public static Vector2 applyAcceleration(this Vector2 _vCurrent, float _fDT, Vector2 _vAcceleration, float _fFriction)
    {
        Vector2 vTerminalSpeed = _vAcceleration / _fFriction;
        float fExp = Mathf.Exp(-_fFriction * _fDT);
        return vTerminalSpeed + ((_vCurrent - vTerminalSpeed) * fExp);
    }

    public static float applyAccelerationMaxSpeed(this float _fCurrent, float _fDT, float _fAcceleration, float _fFriction, float _fMaxSpeed)
    {
        float fSpeed = _fCurrent.applyAcceleration(_fDT, _fAcceleration, _fFriction);
        if (fSpeed > _fMaxSpeed)
        {
            fSpeed = _fMaxSpeed;
        }
        else if (fSpeed < -_fMaxSpeed)
        {
            fSpeed = -_fMaxSpeed;
        }

        return fSpeed;
    }

    public static bool contains(this Transform _oT, Vector2 _v)
    {
        float fTop = _oT.position.y + (_oT.localScale.y * 0.5f);
        float fBottom = _oT.position.y - (_oT.localScale.y * 0.5f);
        float fLeft = _oT.position.x - (_oT.localScale.x * 0.5f);
        float fRight = _oT.position.x + (_oT.localScale.x * 0.5f);

        return _v.x.isBetween(fLeft, fRight) && _v.y.isBetween(fTop, fBottom);
    }

    public static bool contains(Vector2 _vPoint, Vector3 _vCenter, Vector2 _vSize)
    {
        float fTop = _vCenter.y + (_vSize.y * 0.5f);
        float fBottom = _vCenter.y - (_vSize.y * 0.5f);
        float fLeft = _vCenter.x - (_vSize.x * 0.5f);
        float fRight = _vCenter.x + (_vSize.x * 0.5f);

        return _vPoint.x.isBetween(fLeft, fRight) && _vPoint.y.isBetween(fTop, fBottom);
    }

    public static bool contains2D(this Bounds _oBounds, Vector2 _vPoint)
    {
        return contains(_vPoint, _oBounds.center, _oBounds.size);
    }

    public static Vector2 keepRectangleInside(this Transform _oT, Vector2 _v, Vector2 _vSize)
    {
        float fTop = _oT.position.y + (_oT.localScale.y * 0.5f);
        float fBottom = _oT.position.y - (_oT.localScale.y * 0.5f);
        float fLeft = _oT.position.x - (_oT.localScale.x * 0.5f);
        float fRight = _oT.position.x + (_oT.localScale.x * 0.5f);

        _v.x = _v.x.clamp(fLeft + (_vSize.x * 0.5f), fRight - (_vSize.x * 0.5f));
        _v.y = _v.y.clamp(fBottom + (_vSize.y * 0.5f), fTop - (_vSize.y * 0.5f));

        return _v;
    }

    public static Vector2 keepAtDistance(this Vector2 _vKeep, Vector2 _vFrom, float _fDistance)
    {
        float fDistance = _vKeep.distance(_vFrom);
        if (fDistance < _fDistance)
        {
            return _vKeep + ((_vKeep - _vFrom).normalized * (_fDistance - fDistance));
        }
        return _vKeep;
    }

    /// remove from _oHits the results that have the collider's parent equal to _oParent
    public static int filterSameParentHits(this RaycastHit2D[] _aoHits, int _iHits, Transform _oParent)
    {
        int iRemoved = 0;
        for (int i = 0; i < _iHits; ++i)
        {
            if (_aoHits[i].collider.transform.parent == _oParent)
            {
                ++iRemoved;
            }
            else
            {
                _aoHits[i - iRemoved] = _aoHits[i];
            }
        }

        return _iHits - iRemoved;
    }

    public static int removeElement<T>(this T[] _aoHits, int _iElements, int _iIndexToRemove)
    {
        // check if the index to remove is inside the array
        if (_iIndexToRemove >= _iElements)
        {
            Deb.logWarning("Utils.removeElement trying to remove index " + _iIndexToRemove + " in array " + _aoHits + " which has less elements.");
            return _iElements;
        }

        for (int i = _iIndexToRemove; i < _iElements - 1; ++i)
        {
            if (i + 1 < _iElements)
            {
                _aoHits[_iIndexToRemove] = _aoHits[i + 1];
            }
        }
        return _iElements - 1;
    }

    // line (NOT SEGMENT!) vs circle
    public static bool lineVSCircle(out Vector2 _vResult, Vector2 p1, Vector2 p2, Vector2 circleCenter, float circleRadius)
    {
        //  get the distance between X and Z on the segment
        Vector2 dp = p2 - p1;

        float a = Vector2.Dot(dp, dp);
        float b = 2 * Vector2.Dot(dp, p1 - circleCenter);
        float c = Vector2.Dot(circleCenter, circleCenter) - (2 * Vector2.Dot(circleCenter, p1)) + Vector2.Dot(p1, p1) - (circleRadius * circleRadius);
        float bb4ac = (b * b) - (4 * a * c);
        if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0)
        {
            // if line does not intersect, return the same point
            _vResult = p2;
            return false;
        }
        float mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
        _vResult = p1 + (mu2 * (p2 - p1));
        return true;
    }

    // SEGMENT vs circle
    public static bool segmentvsCircle(out Vector2 _vResult, Vector2 p1, Vector2 p2, Vector2 circleCenter, float circleRadius)
    {
        _vResult = Vector2.zero;
        // Calculate the direction vector of the segment
        Vector2 segmentDirection = p2 - p1;

        // Calculate the vector from the circle's center to the segment's start point
        Vector2 fromCircleToStart = p1 - circleCenter;

        // Calculate the coefficients of the quadratic equation for intersection
        float a = Vector2.Dot(segmentDirection, segmentDirection);
        float b = 2f * Vector2.Dot(fromCircleToStart, segmentDirection);
        float c = Vector2.Dot(fromCircleToStart, fromCircleToStart) - (circleRadius * circleRadius);

        // Calculate the discriminant
        float discriminant = (b * b) - (4 * a * c);

        // Check if there are real intersections
        if (discriminant >= 0)
        {
            // Calculate the two possible solutions for t (parameter along the segment)
            float t1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

            // Check if the intersections are within the bounds of the line segment
            if (t1 >= 0 && t1 <= 1)
            {
                _vResult = p1 + (t1 * segmentDirection);
                return true;
            }

            if (t2 >= 0 && t2 <= 1)
            {
                _vResult = p1 + (t2 * segmentDirection);
                return true;
            }

        }
        return false;
    }

    // SEGMENT vs cone
    public static bool segmentvsCone(out List<Vector2> _avResult, Vector2 p1, Vector2 p2, Vector2 _vOrigin, Vector2 _vConeCenter, float _fCircleRadius, float _fConeAngle)
    {
        bool _bIsIntersecting = false;
        _avResult = new List<Vector2>();

        // check if intersecting the two laterals of the cone
        Vector2 vToConeCenter = (_vConeCenter - _vOrigin).normalized * _fCircleRadius;
        Vector2 vIntersectingSegment = Vector2.zero;
        Vector2 vLeft = _vOrigin + vToConeCenter.rotateClockwise(-_fConeAngle * 0.5f);
        Vector2 vRight = _vOrigin + vToConeCenter.rotateClockwise(_fConeAngle * 0.5f);
        if (Utils.isIntersecting(p1, p2, _vOrigin, vLeft, ref vIntersectingSegment))
        {
            _avResult.Add(vIntersectingSegment);
            _bIsIntersecting = true;
        }
        if (Utils.isIntersecting(p1, p2, _vOrigin, vRight, ref vIntersectingSegment))
        {
            _avResult.Add(vIntersectingSegment);
            _bIsIntersecting = true;
        }
        // check if intersecting with circle

        // Calculate the direction vector of the segment
        Vector2 segmentDirection = p2 - p1;

        // Calculate the vector from the circle's center to the segment's start point
        Vector2 fromCircleToStart = p1 - _vOrigin;

        // Calculate the coefficients of the quadratic equation for intersection
        float a = Vector2.Dot(segmentDirection, segmentDirection);
        float b = 2f * Vector2.Dot(fromCircleToStart, segmentDirection);
        float c = Vector2.Dot(fromCircleToStart, fromCircleToStart) - (_fCircleRadius * _fCircleRadius);

        // Calculate the discriminant
        float discriminant = (b * b) - (4 * a * c);

        // Check if there are real intersections
        if (discriminant >= 0)
        {
            // Calculate the two possible solutions for t (parameter along the segment)
            float t1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

            // Check if the intersections are within the bounds of the line segment
            if (t1 >= 0 && t1 <= 1)
            {
                Vector2 vIntersectingPoint = p1 + (t1 * segmentDirection);
                Vector2 vToPoint = vIntersectingPoint - _vOrigin;

                // check if point is inside the cone angles
                if (Vector2.Angle(vToConeCenter, vToPoint) < _fConeAngle * 0.5f)
                {
                    _avResult.Add(vIntersectingPoint);
                    return true;
                }
            }

            if (t2 >= 0 && t2 <= 1)
            {
                Vector2 vIntersectingPoint = p1 + (t2 * segmentDirection);
                Vector2 vToPoint = vIntersectingPoint - _vOrigin;

                // check if point is inside the cone angles
                if (Vector2.Angle(vToConeCenter, vToPoint) < _fConeAngle * 0.5f)
                {
                    _avResult.Add(vIntersectingPoint);
                    return true;
                }
            }
        }
        return _bIsIntersecting;
    }

    // line (NOT SEGMENT!) vs circle
    public static bool lineVSCircleAll(out Vector2[] _avResults, Vector2 p1, Vector2 p2, Vector2 circleCenter, float circleRadius)
    {
        //  get the distance between X and Z on the segment
        Vector2 dp = p2 - p1;

        float a = Vector2.Dot(dp, dp);
        float b = 2 * Vector2.Dot(dp, p1 - circleCenter);
        float c = Vector2.Dot(circleCenter, circleCenter) - (2 * Vector2.Dot(circleCenter, p1)) + Vector2.Dot(p1, p1) - (circleRadius * circleRadius);
        float bb4ac = (b * b) - (4 * a * c);
        if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0)
        {
            //  line does not intersect
            _avResults = new Vector2[] { p2, p2 };
            return false;
        }
        float mu1 = (-b + Mathf.Sqrt(bb4ac)) / (2 * a);
        float mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
        _avResults = new Vector2[2];
        _avResults[0] = p1 + (mu1 * (p2 - p1));
        _avResults[1] = p1 + (mu2 * (p2 - p1));

        return true;
    }

    // searches a point in a list of points or polygon
    public static bool polyPointXZ(this List<Vector3> _polyPoints, Vector2 _pointToContain)
    {
        var j = _polyPoints.Count - 1;
        var inside = false;
        for (int i = 0; i < _polyPoints.Count; j = i++)
        {
            Vector3 pi = _polyPoints[i];
            Vector3 pj = _polyPoints[j];
            if (((pi.z <= _pointToContain.y && _pointToContain.y < pj.z) || (pj.z <= _pointToContain.y && _pointToContain.y < pi.z)) &&
                (_pointToContain.x < ((pj.x - pi.x) * (_pointToContain.y - pi.z) / (pj.z - pi.z)) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    public static bool polyPoint2D(this List<Vector2> _polyPoints, Vector2 _pointToContain)
    {
        var j = _polyPoints.Count - 1;
        var inside = false;
        for (int i = 0; i < _polyPoints.Count; j = i++)
        {
            Vector2 pi = _polyPoints[i];
            Vector2 pj = _polyPoints[j];
            if (((pi.y <= _pointToContain.y && _pointToContain.y < pj.y) || (pj.y <= _pointToContain.y && _pointToContain.y < pi.y)) &&
                (_pointToContain.x < ((pj.x - pi.x) * (_pointToContain.y - pi.y) / (pj.y - pi.y)) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    #endregion

    #region ANGLES
    // returns the angle distance in -180 to 180
    public static float angleDistance(float _fFrom, float _fTo)
    {
        float fAngleDistance = ((_fFrom - _fTo + 180 + 360) % 360) - 180;
        return fAngleDistance;
    }

    public static bool isAngleAtDistance(float _fFrom, float _fTo, float _fDistance)
    {
        return Mathf.Abs(angleDistance(_fFrom, _fTo)) < _fDistance;
    }
    #endregion

    #region GEOMETRY

    public static float calculateMeshArea(this MeshFilter _oMeshFilter)
    {
        if (_oMeshFilter == null || _oMeshFilter.sharedMesh == null)
        {
            Deb.logWarning("No se ha asignado un MeshFilter o el MeshFilter no tiene una malla asignada.");
            return 0;
        }

        Mesh mesh = _oMeshFilter.sharedMesh;

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        float fArea = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 vVertexA = vertices[triangles[i]];
            Vector3 vVertexB = vertices[triangles[i + 1]];
            Vector3 vVertexC = vertices[triangles[i + 2]];

            // Calcula el área del triángulo utilizando el producto cruz de dos lados
            Vector3 vSideAB = vVertexB - vVertexA;
            Vector3 vSideAC = vVertexC - vVertexA;
            float fTriangleArea = 0.5f * Vector3.Cross(vSideAB, vSideAC).magnitude;

            fArea += fTriangleArea;
        }
        return fArea;
    }

    public static bool isPointInCircle(Vector2 _vOrigin, float _fCircleRadius, Vector2 _vPoint, float _fPointRadius = 0)
    {
        float fDistance = _vPoint.distance(_vOrigin);
        if (fDistance < _fPointRadius + _fCircleRadius || fDistance.approximately(_fPointRadius + _fCircleRadius, 0.005f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool isPointInCone(float _fConeAngle, Vector2 _vConeCenter, Vector2 _vOrigin, float _fCircleRadius, Vector2 _vPoint, float _fPointRadius = 0)
    {
        if (isPointInCircle(_vOrigin, _fCircleRadius, _vPoint, _fPointRadius))
        {
            Vector2 vToConeCenter = (_vConeCenter - _vOrigin).normalized * _fCircleRadius;
            Vector2 vToPoint = _vPoint - _vOrigin;

            // check if point is inside the cone angles
            if (Vector2.Angle(vToConeCenter, vToPoint) < _fConeAngle * 0.5f || Vector2.Angle(vToConeCenter, vToPoint).approximately(_fConeAngle * 0.5f, 0.005f))
            {
                if (_fPointRadius >= 0.01f) Deb.Instance.drawCircle(vToPoint, _fPointRadius, 6, Color.green, 2f);
                return true;
            }
            else
            {
                // else, check point distance to the two lateral lines of the cone, to see if the point radius touches the cone
                Vector2 vLeft = _vOrigin + vToConeCenter.rotateClockwise(-_fConeAngle * 0.5f);
                Vector2 vRight = _vOrigin + vToConeCenter.rotateClockwise(_fConeAngle * 0.5f);
                Debug.DrawLine(_vOrigin.toVector3xy(), vLeft.toVector3xy(), Color.white, 2f);
                Debug.DrawLine(_vOrigin.toVector3xy(), vRight.toVector3xy(), Color.white, 2f);

                if (getDistancePointToSegment(_vPoint, _vOrigin, vLeft) <= _fPointRadius
                 || getDistancePointToSegment(_vPoint, _vOrigin, vRight) <= _fPointRadius)
                {
                    if (_fPointRadius >= 0.01f) Deb.Instance.drawCircle(vToPoint, _fPointRadius, 6, Color.green, 2f);
                    return true;
                }
            }
        }
        return false;
    }

    public static List<Vector2> getAllPointsInCircle(this Vector2 _vOrigin, float _fRadius, float _fStepDistanceX, float _fMarginPercentage = 0.02f, string _sSeed = null)
    {
        List<Vector2> aoPoints = new List<Vector2>();
        float fRealMargin = _fMarginPercentage * _fRadius * 2;
        float fDiameterUsed = _fRadius * 2 - fRealMargin * 2;

        float fRealStepX = fDiameterUsed / Mathf.FloorToInt(fDiameterUsed / _fStepDistanceX);
        int iRealPartsX = Mathf.FloorToInt(fDiameterUsed / fRealStepX);

        float fStepDistanceY = MathF.Sqrt(0.75f) * fRealStepX; // the value of a in the function h = a^2 + b^2
        float fRealStepY = fDiameterUsed / Mathf.FloorToInt(fDiameterUsed / fStepDistanceY);
        int iRealPartsY = Mathf.FloorToInt(fDiameterUsed / fRealStepY);



        Vector2 vInitialPoint = _vOrigin + Vector2.left * fDiameterUsed / 2 + Vector2.down * fDiameterUsed / 2;
        float fHeightDone = 0;
        float fDistanceDone = 0;
        Vector2 vPoint;
        for (int i = 0; i < iRealPartsY + 1; i++)
        {
            for (int j = 0; j < iRealPartsX + 1; j++)
            {
                vPoint = vInitialPoint + Vector2.up * fHeightDone + Vector2.right * fDistanceDone;
                if (_vOrigin.distance(vPoint) <= _fRadius)
                {
                    aoPoints.Add(vPoint);
                }
                fDistanceDone += fRealStepX;
            }
            if (i % 2 == 0)
            {
                fDistanceDone = fRealStepX / 2;
            }
            else
            {
                fDistanceDone = 0;
            }
            fHeightDone += fRealStepY;
        }
        for (int i = 0; i < aoPoints.Count; i++)
        {
            //Deb.Instance.drawCircle(aoPoints[i], 0.01f, 12, Color.gray, 7);
        }
        //Deb.Instance.drawCircle(_vOrigin, _fRadius, 32, Color.gray, 7);
        return aoPoints;
    }
    #endregion

    #region Math

    public static float round(this float _f)
    {
        return Mathf.Round(_f);
    }

    public static float customRound(this float _f, float fThreshold = 0.495f)
    {
        float fDecimal = _f % 1;
        if (fDecimal >= fThreshold)
        {
            return Mathf.Ceil(_f);
        }
        return Mathf.Floor(_f);
    }

    public static int customRoundInt(this float _f, float fThreshold = 0.495f)
    {
        float fDecimal = _f % 1;
        if (fDecimal >= fThreshold)
        {
            return Mathf.RoundToInt(Mathf.Ceil(_f));
        }
        return Mathf.RoundToInt(Mathf.Floor(_f));
    }

    #endregion

    #region DEBUG DRAW

    public static void DrawPoint2D(Vector2 _vPoint, float _fSize, Color _cColor, float _fTime)
    {
        _fSize *= 0.5f;
        Debug.DrawLine(_vPoint + ((-Vector2.right - Vector2.up).normalized * _fSize), _vPoint + ((Vector2.right + Vector2.up).normalized * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint + ((Vector2.right - Vector2.up).normalized * _fSize), _vPoint + ((-Vector2.right + Vector2.up).normalized * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint + (Vector2.right * _fSize), _vPoint - (Vector2.right * _fSize), _cColor, _fTime);
        Debug.DrawLine(_vPoint + (Vector2.up * _fSize), _vPoint - (Vector2.up * _fSize), _cColor, _fTime);
    }

    #endregion

    #region GAMEPLAY

    public class TrailPause
    {
        float trailTime = 1.0f;
        float pauseTime;
        float resumeTime;

        public TrailRenderer trail;

        public TrailPause(TrailRenderer trail)
        {
            this.trail = trail;
            pauseTrail();
        }
        void pauseTrail()
        {
            pauseTime = Time.time;
            trail.time = Mathf.Infinity;
        }

        public void resumeTrail()
        {
            resumeTime = Time.time;
            trail.time = (resumeTime - pauseTime) + trailTime;
        }
    }
    public static List<Animator> m_aoAnimators;
    public static List<Component> m_aoFXs;
    public static List<TrailPause> m_aoTrails;
    public static List<GameObject> m_aoRootObjects;
    public static List<AudioSource> m_aoSounds;

    public static void pause()
    {
        m_aoRootObjects = getAllRootObjects();
        m_aoAnimators = disableAllBehaviours<Animator>();
        m_aoFXs = disableAllParticles();
        m_aoSounds = pauseSounds();
        m_aoRootObjects = null;
    }

    public static void unpause()
    {
        enableBehaviours(ref m_aoAnimators);
        unpauseSounds(ref m_aoSounds);
        enableAllParticles(ref m_aoFXs);
    }

    public static List<AudioSource> pauseSounds()
    {
        List<AudioSource> oList = new List<AudioSource>();

        foreach (GameObject oObj in m_aoRootObjects)
        {
            AudioSource[] aoSounds = oObj.GetComponentsInChildren<AudioSource>();

            for (int i = 0; i < aoSounds.Length; ++i)
            {
                aoSounds[i].Pause();
                oList.Add(aoSounds[i]);
            }
        }

        return oList;
    }

    public static void unpauseSounds(ref List<AudioSource> aoSounds)
    {
        for (int i = 0; i < aoSounds.Count; ++i)
        {
            if (aoSounds[i] != null) aoSounds[i].UnPause();
        }
        aoSounds = null;
    }

    public static List<T> disableAllBehaviours<T>() where T : UnityEngine.Behaviour
    {
        List<T> oList = new List<T>();

        foreach (GameObject oObj in m_aoRootObjects)
        {
            Behaviour[] aoBehaviours = oObj.GetComponentsInChildren<T>();

            for (int i = 0; i < aoBehaviours.Length; ++i)
            {
                if (aoBehaviours[i].enabled)
                {
                    aoBehaviours[i].enabled = false;

                    oList.Add(aoBehaviours[i] as T);
                }
            }
        }

        return oList;
    }

    public static List<Component> disableAllParticles()
    {
        List<Component> oList = new List<Component>();
        m_aoTrails = new List<TrailPause>();

        foreach (GameObject oObj in m_aoRootObjects)
        {
            ParticleSystem[] aoParticles = oObj.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < aoParticles.Length; ++i)
            {
                if (aoParticles[i].isPlaying)
                {
                    aoParticles[i].Pause(true);

                    oList.Add(aoParticles[i]);
                }
            }
            VisualEffect[] aoVisualEffects = oObj.GetComponentsInChildren<VisualEffect>();

            for (int i = 0; i < aoVisualEffects.Length; ++i)
            {
                if (aoVisualEffects[i].enabled)
                {
                    aoVisualEffects[i].pause = true;

                    oList.Add(aoVisualEffects[i]);
                }
            }

            TrailRenderer[] aoTrailRenderers = oObj.GetComponentsInChildren<TrailRenderer>();

            for (int i = 0; i < aoTrailRenderers.Length; ++i)
            {
                if (aoTrailRenderers[i].enabled)
                {
                    m_aoTrails.Add(new TrailPause(aoTrailRenderers[i]));
                }
            }
        }
        return oList;
    }

    public static void enableBehaviours<T>(ref List<T> aoBehaviours) where T : UnityEngine.Behaviour
    {
        if (aoBehaviours != null)
        {
            for (int i = 0; i < aoBehaviours.Count; ++i)
            {
                if (aoBehaviours[i] != null)
                {
                    aoBehaviours[i].enabled = true;
                }
            }
            aoBehaviours = null;
        }
    }

    public static void enableAllParticles(ref List<Component> aoComponents)
    {
        if (aoComponents != null)
        {
            for (int i = 0; i < aoComponents.Count; ++i)
            {
                if (aoComponents[i] != null)
                {
                    if (aoComponents[i] is ParticleSystem oPS)
                    {
                        oPS.Play(true);
                    }
                    else if (aoComponents[i] is VisualEffect oVE)
                    {
                        oVE.pause = false;
                    }
                }
            }
            for (int i = 0; i < m_aoTrails.Count; i++)
            {
                if (m_aoTrails[i].trail != null)
                {
                    m_aoTrails[i].resumeTrail();
                }
            }
            aoComponents = null;
            m_aoTrails = null;
        }
    }

    public static List<GameObject> getAllRootObjects()
    {
        List<GameObject> aoObjects = new List<GameObject>();
        aoObjects.AddRange(SceneManager.GetSceneAt(SceneManager.sceneCount - 1).GetRootGameObjects());

        GameObject temp = null;
        try
        {
            temp = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(temp);
            Scene dontDestroyOnLoad = temp.scene;
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(temp);
#else
            UnityEngine.Object.Destroy(temp);
#endif
            temp = null;

            aoObjects.AddRange(dontDestroyOnLoad.GetRootGameObjects());
        }
        finally
        {
            if (temp != null)
            {
#if UNITY_EDITOR
                UnityEngine.Object.DestroyImmediate(temp);
#else
                UnityEngine.Object.Destroy(temp);
#endif
            }
        }
        return aoObjects;
    }
    #endregion

    #region ACTIVATORS
    public static void execute(this ActivatorByInput _oActivator, eActivatorByInput _eInput)
    {
        for (int d = 0; d < _oActivator.m_aoDatas.Length; ++d)
        {
            _oActivator.m_aoDatas[d].activate(_oActivator.m_aoDatas[d].m_eInput == _eInput);
        }
    }

    public static void executeActivatorsByInput(eActivatorByInput _eInput)
    {
        ActivatorByInput[] aoActivators = GameObject.FindObjectsByType<ActivatorByInput>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < aoActivators.Length; i++)
        {
            aoActivators[i].execute(_eInput);
        }
    }
    #endregion

    #region Random Points Generation

    /// <summary>
    /// Generates well-distributed random points within a cone.
    /// </summary>
    /// <param name="_fMaxRadius">Radius of the circle of the cone.</param>
    /// <param name="_fStepSize">Minimum spacing between points.</param>
    /// <returns>List of Vector2 points in the cone.</returns>
    public static List<Vector2> generatePointsInConeEqualDistribution(this Vector2 _vInitial, float _fMinRadius, float _fMaxRadius, float _fStepSize, float coneAngle, Vector2 coneDirectionV2)
    {
        List<Vector2> points = new List<Vector2>();

        // Normalize the direction and calculate its angle in radians
        coneDirectionV2.Normalize();
        float coneDirectionRad = Mathf.Atan2(coneDirectionV2.y, coneDirectionV2.x);
        float halfConeAngle = coneAngle / 2f * Mathf.Deg2Rad;

        // Define square bounds based on circle radius
        float minX = -_fMaxRadius;
        float maxX = _fMaxRadius;
        float minY = -_fMaxRadius;
        float maxY = _fMaxRadius;

        // Iterate through the grid points within the square bounds
        for (float x = minX; x <= maxX; x += _fStepSize)
        {
            for (float y = minY; y <= maxY; y += _fStepSize)
            {
                Vector2 point = new Vector2(x, y);

                // Check if the point is within the circle
                if (point.magnitude <= _fMaxRadius && point.magnitude >= _fMinRadius)
                {
                    // Calculate angle between point and cone direction
                    float angleToPoint = Mathf.Atan2(point.y, point.x);

                    // Check if point is within cone angle
                    float angleDifference = Mathf.DeltaAngle(angleToPoint * Mathf.Rad2Deg, coneDirectionRad * Mathf.Rad2Deg);

                    if (Mathf.Abs(angleDifference) <= halfConeAngle * Mathf.Rad2Deg)
                    {
                        points.Add(point + _vInitial);
                    }
                }
            }
        }
        drawPoints(points);
        return points;
    }
    /// <summary>
    /// Generates well-distributed random points within a circle.
    /// </summary>
    /// <param name="_fMaxRadius">Radius of the circle.</param>
    /// <param name="_fStepSize">Minimum spacing between points.</param>
    /// <returns>List of Vector2 points in the circle.</returns>
    public static List<Vector2> generatePointsInCircleEqualDistribution(this Vector2 _vInitial, float _fMinRadius, float _fMaxRadius, float _fStepSize)
    {
        List<Vector2> points = new List<Vector2>();

        // Define the square bounds based on the circle radius
        float minX = -_fMaxRadius;
        float maxX = _fMaxRadius;
        float minY = -_fMaxRadius;
        float maxY = _fMaxRadius;

        // Iterate through a grid of points within the square bounds
        for (float x = minX; x <= maxX; x += _fStepSize)
        {
            for (float y = minY; y <= maxY; y += _fStepSize)
            {
                Vector2 point = new Vector2(x, y);

                // Check if the point is within the circle's radius
                if (point.magnitude <= _fMaxRadius && point.magnitude >= _fMinRadius)
                {
                    points.Add(point + _vInitial);
                }
            }
        }
        //drawPoints(points);
        return points;
    }

    /// <summary>
    /// Generates evenly distributed points inside a rectangle centered on <paramref name="_vOriginCenter"/>.
    /// </summary>
    /// <param name="_vOriginCenter">Rectangle center point.</param>
    /// <param name="_fMaxWidth">Total rectangle width.</param>
    /// <param name="_fMaxHeight">Total rectangle height.</param>
    /// <param name="_fStepSize">Distance between generated points.</param>
    /// <returns>List of Vector2 points in the rectangle.</returns>
    public static List<Vector2> generatePointsInRectangleEqualDistribution(this Vector2 _vOriginCenter, float _fMaxWidth, float _fMaxHeight, float _fStepSize)
    {
        List<Vector2> points = new List<Vector2>();

        if (_fStepSize <= 0f)
        {
            Deb.logWarning("Step size must be > 0.");
            return points;
        }

        float fHalfWidth = Mathf.Abs(_fMaxWidth) * 0.5f;
        float fHalfHeight = Mathf.Abs(_fMaxHeight) * 0.5f;

        for (float x = -fHalfWidth; x <= fHalfWidth; x += _fStepSize)
        {
            for (float y = -fHalfHeight; y <= fHalfHeight; y += _fStepSize)
            {
                points.Add(_vOriginCenter + new Vector2(x, y));
            }
        }

        drawPoints(points);
        return points;
    }

    //Same as above, but return an array instead. Only the first _iPointsCount position are valid
    public static Vector2[] generatePointsInCircleEqualDistribution(this Vector2 _vInitial, float _fMinRadius, float _fMaxRadius, float _fStepSize, out int _iPointsCount)
    {
        int iGridSize = Mathf.CeilToInt((_fMaxRadius - _fMinRadius) / _fStepSize);
        Vector2[] points = new Vector2[iGridSize * iGridSize];

        // Define the square bounds based on the circle radius
        float fSqrMaxRadius = _fMaxRadius * _fMaxRadius;
        float fSqrMinRadius = _fMinRadius * _fMinRadius;

        _iPointsCount = 0;
        float fX = -_fMaxRadius;
        // Iterate through a grid of points within the square bounds
        for (int iX = 0; iX < iGridSize; ++iX)
        {
            float fY = -_fMaxRadius;
            for (int iY = 0; iY < iGridSize; ++iY)
            {
                Vector2 point = new Vector2(fX, fY);

                // Check if the point is within the circle's radius
                float fSqrMangnitude = point.sqrMagnitude;
                if (fSqrMangnitude <= fSqrMaxRadius && fSqrMangnitude >= fSqrMinRadius)
                {
                    points[_iPointsCount++] = point + _vInitial;
                }
                fY += _fStepSize;
            }
            fX += _fStepSize;
        }

        return points;
    }

    /// <summary>
    /// Generates exactly <paramref name="pointCount"/> points uniformly
    /// distributed in an annulus centred at <paramref name="_vInitial"/>.
    /// No external libraries required.
    /// </summary>
    /// <param name="_vInitial">Centre of the ring.</param>
    /// <param name="_fMinRadius">Inner radius (≥0).</param>
    /// <param name="_fMaxRadius">Outer radius (>&lt; inner).</param>
    /// <param name="pointCount">Number of points requested (≥1).</param>
    public static List<Vector2> generatePointsInCircleRandomDistribution(
        this Vector2 _vInitial,
        float _fMinRadius,
        float _fMaxRadius,
        int pointCount)
    {
        if (pointCount <= 0 || _fMaxRadius <= _fMinRadius || _fMinRadius < 0f)
        {
            Deb.logWarning("Invalid parameters.");
            return new List<Vector2>();
        }

        // Allow deterministic or nondeterministic sampling.
        System.Random prng = new System.Random();

        List<Vector2> points = new List<Vector2>(pointCount);
        float rMin2 = _fMinRadius * _fMinRadius;
        float rMax2 = _fMaxRadius * _fMaxRadius;

        for (int i = 0; i < pointCount; i++)
        {
            // 1. draw uniform random numbers
            float u = (float)prng.NextDouble();             // for radius
            float theta = 2f * Mathf.PI * (float)prng.NextDouble(); // for angle

            // 2. inverse-transform sampling for annulus
            float radius = Mathf.Sqrt(u * (rMax2 - rMin2) + rMin2);

            // 3. convert to Cartesian and translate to centre
            Vector2 offset = new Vector2(
                radius * Mathf.Cos(theta),
                radius * Mathf.Sin(theta));

            points.Add(_vInitial + offset);
        }
        //drawPoints(points);

        return points;
    }

    /// <param name="polygon">Boundary vertices of a simple polygon.</param>
    /// <param name="pointCount">Number of random points to return.</param>
    public static List<Vector2> generatePointsInPolygonRandomDistribution(this Vector2[] polygon, int pointCount)
    {
        if (polygon == null || polygon.Length < 3 || pointCount <= 0)
            return new List<Vector2>();

        // 1. Ear-clipping triangulation
        List<Triangle> tris = EarClip(polygon);

        // 2. Build cumulative area array
        float totalArea = 0f;
        foreach (var t in tris)
        {
            t.Area = TriangleArea(t.A, t.B, t.C); // signed positive
            totalArea += t.Area;
        }

        // 3. Weighted sampling + barycentric √ trick
        List<Vector2> result = new List<Vector2>(pointCount);
        for (int i = 0; i < pointCount; ++i)
        {
            float pick = UnityEngine.Random.Range(0f, totalArea);
            Triangle chosen = null;
            foreach (var t in tris)
            {
                if (pick <= t.Area) { chosen = t; break; }
                pick -= t.Area;
            }
            result.Add(RandomPointInTriangle(chosen.A, chosen.B, chosen.C));
        }

        //drawPoints(result);
        return result;
    }

    // ------------- Ear-clipping triangulation ----------------------------------

    private static List<Triangle> EarClip(Vector2[] input)
    {
        // Make a working list of indices so we can remove ears
        List<int> V = new List<int>(input.Length);
        for (int i = 0; i < input.Length; ++i) V.Add(i);

        bool isCCW = PolygonSignedArea(input) > 0;
        if (!isCCW) V.Reverse();          // ensure counter-clockwise for ear test[9]

        List<Triangle> output = new List<Triangle>();

        int guard = 0;                    // failsafe for degenerate data
        while (V.Count > 3 && guard++ < 5000)
        {
            bool earFound = false;
            for (int v = 0; v < V.Count && !earFound; v++)
            {
                int prev = V[(v - 1 + V.Count) % V.Count];
                int curr = V[v];
                int next = V[(v + 1) % V.Count];

                Vector2 A = input[prev];
                Vector2 B = input[curr];
                Vector2 C = input[next];

                if (!IsConvex(A, B, C)) continue;           // must be convex

                bool containsPoint = false;
                for (int j = 0; j < V.Count; j++)
                {
                    int idx = V[j];
                    if (idx == prev || idx == curr || idx == next) continue;
                    if (PointInTriangle(input[idx], A, B, C))
                    { containsPoint = true; break; }
                }
                if (containsPoint) continue;                // not an ear

                // Valid ear → store triangle, clip vertex
                output.Add(new Triangle(A, B, C));
                V.RemoveAt(v);
                earFound = true;
            }
            if (!earFound) break; // polygon not simple
        }
        // last triangle
        if (V.Count == 3)
            output.Add(new Triangle(input[V[0]], input[V[1]], input[V[2]]));

        return output;
    }

    // ------------- Geometry helper functions -----------------------------------

    private static float PolygonSignedArea(Vector2[] p)
    {
        float a = 0f;
        for (int i = 0, n = p.Length; i < n; ++i)
        {
            Vector2 v0 = p[i];
            Vector2 v1 = p[(i + 1) % n];
            a += (v0.x * v1.y - v1.x * v0.y);
        }
        return a * 0.5f;
    }

    private static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        // positive cross product for CCW polygon means convex[9]
        return Cross(b - a, c - b) > 0f;
    }

    private static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        // barycentric sign method, accepts points on edges
        float s1 = Cross(b - a, p - a);
        float s2 = Cross(c - b, p - b);
        float s3 = Cross(a - c, p - c);
        bool hasNeg = (s1 < 0) || (s2 < 0) || (s3 < 0);
        bool hasPos = (s1 > 0) || (s2 > 0) || (s3 > 0);
        return !(hasNeg && hasPos);
    }

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
        => Mathf.Abs(Cross(b - a, c - a)) * 0.5f;

    private static Vector2 RandomPointInTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        float r1 = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f));
        float r2 = UnityEngine.Random.Range(0f, 1f);
        return (1 - r1) * a + r1 * (1 - r2) * b + r1 * r2 * c; // uniform[2][17]
    }

    // ------------- Triangle data holder ----------------------------------------

    private class Triangle
    {
        public Vector2 A, B, C;
        public float Area;
        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        { A = a; B = b; C = c; }
    }

    #endregion Random Points Generation

    #region OTHERS

#if UNITY_EDITOR
    public static T[] getAllAssetsOfType<T>(string _sPath) where T : UnityEngine.Object
    {
        List<T> aoAssetsInPath = new List<T>();

        string[] asGUIDS = AssetDatabase.FindAssets($"t:{typeof(T).ToString()}", new[] { _sPath });

        foreach (string sGUID in asGUIDS)
        {
            string sPath = AssetDatabase.GUIDToAssetPath(sGUID);

            T oAsset = AssetDatabase.LoadAssetAtPath(sPath, typeof(T)) as T;

            aoAssetsInPath.Add(oAsset);
        }

        return aoAssetsInPath.ToArray();
    }

    public static string[] getAllPathAssetsOfType<T>(string _sPath) where T : UnityEngine.Object
    {
        List<string> aoPaths = new List<string>();

        string sType = typeof(T).ToString().Split(".")[^1];
        string[] asGUIDS = AssetDatabase.FindAssets($"t:{sType}", new[] { _sPath });

        foreach (string sGUID in asGUIDS)
        {
            string sPath = AssetDatabase.GUIDToAssetPath(sGUID);

            aoPaths.Add(sPath);
        }

        return aoPaths.ToArray();
    }
#endif
    public static void drawPoints(List<Vector2> aoPoints)
    {
        foreach (Vector2 point in aoPoints)
        {
            Deb.Instance.drawCircle(point, 0.05f, 10, Color.red, 5);
        }
    }


    public static string toRoman(this int _iNumber)
    {
        Dictionary<int, string> dNumberToRoman = new Dictionary<int, string>
        {
            { 1000, "M" },
            { 900, "CM" },
            { 500, "D" },
            { 400, "CD" },
            { 100, "C" },
            { 90, "XC" },
            { 50, "L" },
            { 40, "XL" },
            { 10, "X" },
            { 9, "IX" },
            { 5, "V" },
            { 4, "IV" },
            { 1, "I" }
        };

        if (_iNumber <= 0 || _iNumber > 3999)
        {
            Deb.logWarning("invalid_number in set day");
            return "";
        }

        string sRomanNumber = string.Empty;

        foreach (KeyValuePair<int, string> itPair in dNumberToRoman)
        {
            while (_iNumber >= itPair.Key)
            {
                sRomanNumber += itPair.Value;
                _iNumber -= itPair.Key;
            }
        }

        return sRomanNumber;
    }

    public static T GetComponentInChildrenActiveAndEnabled<T>(this Component _oComponent) where T : Behaviour
    {
        T[] aoComponents = _oComponent.GetComponentsInChildren<T>(false);

        foreach (T itComponent in aoComponents)
        {
            if (itComponent.isActiveAndEnabled) { return itComponent; }
        }

        return null;
    }

    public static T[] GetComponentsInChildrenActiveAndEnabled<T>(this Component _oComponent) where T : Behaviour
    {
        List<T> aoComponentsToReturn = new List<T>();

        T[] aoComponents = _oComponent.GetComponentsInChildren<T>(false);

        foreach (T itComponent in aoComponents)
        {
            if (itComponent.gameObject.activeSelf && itComponent.enabled) { aoComponentsToReturn.Add(itComponent); }
        }

        return aoComponentsToReturn.ToArray();
    }

#if UNITY_EDITOR
    public static void setActiveBuildProfile(BuildProfile _oProfile)
    {
        Debug.Log("setActiveBuildProfile: " + _oProfile);
        BuildProfile.SetActiveBuildProfile(_oProfile);
    }
#endif

#endregion Others

    #region LINE

    public static Vector2 getIntersectionPoint(this Line2D _oLine_1, Line2D _oLine_2)
    {
        Vector2 vIntersectingPOint = Vector2.zero;

        Utils.isIntersecting(_oLine_1.getInitialPos(), _oLine_1.getInitialPos() + _oLine_1.getDirection(),
                                _oLine_2.getInitialPos(), _oLine_2.getInitialPos() + _oLine_2.getDirection(),
                                ref vIntersectingPOint);

        return vIntersectingPOint;
    }

    #endregion LINE

    #region UnityUtils
    public static MemoryStream getMemoryStreamOf(object _o)
    {
        MemoryStream stream = new MemoryStream();
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();

        try
        {
            bformatter.Serialize(stream, _o);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return stream;
    }

    public static T loadObjectFromBuffer<T>(byte[] _oBuffer)
    {
        Stream stream = new MemoryStream(_oBuffer);
        BinaryFormatter bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        T oResult = default(T);

        try
        {
            oResult = (T)bformatter.Deserialize(stream);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        stream.Close();
        return oResult;
    }
    #endregion

    public static int getLength<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Length;
    }

    public static Texture2D toTexture2D(this RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }

    #region DICTIONARIES

    public static void increaseValue<T>(this Dictionary<T, int> _dDictionary, T _oKey, int _iValue)
    {
        if (!_dDictionary.ContainsKey(_oKey))
        {
            _dDictionary.Add(_oKey, _iValue);
        }
        else
        {
            _dDictionary[_oKey] += _iValue;
        }
    }

    public static void increaseValue<T>(this Dictionary<T, float> _dDictionary, T _oKey, float _fValue)
    {
        if (!_dDictionary.ContainsKey(_oKey))
        {
            _dDictionary.Add(_oKey, _fValue);
        }
        else
        {
            _dDictionary[_oKey] += _fValue;
        }
    }

    public static void flagValue<T>(this Dictionary<T, bool> _dDictionary, T _oKey)
    {
        if (!_dDictionary.ContainsKey(_oKey))
        {
            _dDictionary.Add(_oKey, true);
        }
        else
        {
            _dDictionary[_oKey] = true;
        }
    }

    public static G getValue<T,G>(this Dictionary<T, G> _dDictionary, T _oKey, bool _bAddIfDoesNotExist = false) where G : new()
    {
        if (!_dDictionary.ContainsKey(_oKey)) 
        {
            G oDefaultValue = new G();

            if (_bAddIfDoesNotExist)
            {
                _dDictionary.Add(_oKey, oDefaultValue);
            }

            return oDefaultValue; 
        }
        else { return _dDictionary[_oKey]; }
    }

    public static long getUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
[System.Serializable]
public class DictionaryWrapper<Key, TValue>
{
    [TableList][SerializeField] public List<WrapperKeyValuePair<Key, TValue>> Wrapper;
    public bool hasKey(Key key)
    {
        for (int i = 0; i < Wrapper.Count; i++)
        {
            if (Wrapper[i].key.Equals(key))
            {
                return true;
            }
        }
        return false;
    }
    public void add(Key key, TValue value)
    {
        for (int i = 0; i < Wrapper.Count; i++)
        {
            if (Wrapper[i].key.Equals(key))
            {
                Wrapper[i] = new WrapperKeyValuePair<Key, TValue>(key, value);
                return;
            }
        }
        Wrapper.Add(new WrapperKeyValuePair<Key, TValue>(key, value));
    }
    public void remove(Key key)
    {
        for (int i = 0; i < Wrapper.Count; i++)
        {
            if (Wrapper[i].key.Equals(key))
            {
                Wrapper.RemoveAt(i); ;
                return;
            }
        }
    }
    public TValue getValue(Key key)
    {
        for (int i = 0; i < Wrapper.Count; i++)
        {
            if (Wrapper[i].key.Equals(key))
            {
                return Wrapper[i].Value;
            }
        }
        return default(TValue);
    }

    [Button(ButtonSizes.Medium)]
    public void Clear()
    {
        Wrapper.Clear();
    }
}

[System.Serializable]
public struct WrapperKeyValuePair<Key, TValue>
{
    [TableColumnWidth(100, Resizable = false)]
    public Key key;
    [TableColumnWidth(250, Resizable = true)]
    public TValue Value;

    public WrapperKeyValuePair(Key key, TValue value)
    {
        this.key = key;
        this.Value = value;
    }
}

#endregion DICTIONARIES

[System.Serializable]
public class ColorList
{
    [SerializeField]
    public List<Color> m_aoColors = new List<Color>();

    public ColorList() { m_aoColors = new List<Color>(); }

    public void setColors(List<Color> _aoColors) { m_aoColors = _aoColors; }
}

public enum eReference { Tag, Reference }

[Serializable]
public class TagReference
{
    [SerializeField] eReference m_eType;
    [SerializeField, ShowIf("@m_eType == eReference.Tag")] CustomTag m_oTag;

    [SerializeField, ShowIf("@m_eType == eReference.Reference")]
    GameObject m_oObject;

    public GameObject getGameobject()
    {
        return m_eType == eReference.Tag ? GameplayManager.Instance.getGameobject(m_oTag.Value) : m_oObject;
    }
}

[Serializable]
public class GenericReference<T> where T : MonoBehaviour
{
    [SerializeField] eReference m_eType;
    [SerializeField, ShowIf("@m_eType == eReference.Tag")] CustomTag m_oTag;

    [SerializeField, ShowIf("@m_eType == eReference.Reference")]
    T m_oGeneric;

    public T getReference()
    {
        return m_eType == eReference.Tag ? GameplayManager.Instance.getGameobject(m_oTag.Value).GetComponent<T>() : m_oGeneric;
    }

    public eReference Type { get { return m_eType; } }
}

public struct Line2D
{
    Vector2 m_vInitialPos;
    Vector2 m_vDirection;

    public static Line2D Line2DByPositions(Vector2 _vInitialPos, Vector2 _vFinalPos)
    {
        Line2D oLine = new Line2D();

        oLine.m_vInitialPos = _vInitialPos;
        oLine.m_vDirection = (_vFinalPos - _vInitialPos).normalized;

        return oLine;
    }

    public static Line2D Line2DByDirection(Vector2 _vInitialPos, Vector2 _vDirection)
    {
        Line2D oLine = new Line2D();

        oLine.m_vInitialPos = _vInitialPos;
        oLine.m_vDirection = _vDirection;

        return oLine;
    }

    public Vector2 getInitialPos() { return m_vInitialPos; }
    public Vector2 getDirection() { return m_vDirection; }
}