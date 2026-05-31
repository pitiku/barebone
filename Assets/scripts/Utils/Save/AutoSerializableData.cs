using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DontSaveDataAttribute : Attribute
{
}

public interface IPreSaveSync
{
    void syncBeforeSave();
}

public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}

/// <summary>
/// class that adds automatic serialize/deserialize functionality to all the classes, arrays, lists composed of basic types or other classes. The game data object should inherit from this class,
/// implement a constructor that calls the constructor in this class public SaveData(SerializationInfo info, StreamingContext ctxt) : this() in order to work properly.
/// </summary>
[Serializable]
public class AutoSerializableData : ISerializable
{
    #region MAIN
    const string COUNT = "_COUNT_";
    const string KEY = "_KEY_";
    const string VALUE = "_VALUE_";

    // reference to the object where all the info is put during serialization
    SerializationInfo m_oInfo;

    public AutoSerializableData() { }

    // this constructor is called when Deserialize(stream) is called, here we read the serialized data from the SerializationInfo object
    public AutoSerializableData(SerializationInfo _oInfo, StreamingContext _oCtxt) : this()
    {
        m_oInfo = _oInfo;
        object oObject = this;
        loadFields(ref oObject, "");
    }

    // this method is called when Serialize(stream, object) is called, here we write the data to SerializationInfo object
    public void GetObjectData(SerializationInfo _oInfo, StreamingContext _oCtxt)
    {
        m_oInfo = _oInfo;
        object oObject = this;
        saveFields(ref oObject, "");
    }
    #endregion 

    #region SAVE

    // this method gets all the field of object and processes them depending the type (array, list or individual object)
    void saveFields(ref object _oObject, string _sCompleteName)
    {
        if (_oObject == null)
        {
            return;
        }

        if (ms_bLogs) { ms_sLog.AppendLine("saveFields: " + _sCompleteName); }

        Type oType = _oObject.GetType();
        FieldInfo[] aoFields = oType.GetFields();
        for (int iIndex = 0; iIndex < aoFields.Length; ++iIndex)
        {
            // ignore constants and not serialized fields
            if (aoFields[iIndex].IsLiteral 
                || aoFields[iIndex].IsNotSerialized 
                || Attribute.IsDefined(aoFields[iIndex], typeof(DontSaveDataAttribute)))
            {
                continue;
            }

            Type oFieldType = aoFields[iIndex].FieldType;
            object oValue = aoFields[iIndex].GetValue(_oObject);

            if (oValue == null)
            {
                continue;
            }

            string sFieldName = _sCompleteName + "/" + aoFields[iIndex].Name;
            if (oFieldType.IsArray)
            {
                Array oArray = (Array)oValue;
                saveBasicValue(sFieldName + COUNT, oArray.Length);
                for (int i = 0; i < oArray.Length; ++i)
                {
                    object oToPass = oArray.GetValue(i);
                    saveField(ref oToPass, sFieldName + "/" + i);
                }
            }
            else if (oFieldType.IsGenericType && oFieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList oList = (IList)oValue;
                saveBasicValue(sFieldName + COUNT, oList.Count);
                for (int i = 0; i < oList.Count; ++i)
                {
                    object oToPass = oList[i];
                    saveField(ref oToPass, sFieldName + "/" + i);
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(oFieldType))
            {
                IDictionary oDict = (IDictionary)oValue;
                saveBasicValue(sFieldName + COUNT, oDict.Keys.Count);
                int iKeyIndex = 0;
                foreach (object oDictKey_ in oDict.Keys)
                {
                    object oDictKey = oDictKey_;
                    object oDictValue = oDict[oDictKey];

                    saveField(ref oDictKey, sFieldName + "/" + iKeyIndex + KEY);
                    saveField(ref oDictValue, sFieldName + "/" + iKeyIndex + VALUE);
                    iKeyIndex++;
                }
            }
            else
            {
                saveField(ref oValue, sFieldName);
            }
        }
    }

    // this method checks if is a basic value and saves it, or if its a class container and calls recursively the saveFields method
    void saveField(ref object _oValue, string _sCompleteName)
    {
        if (ms_bLogs) { ms_sLog.AppendLine("saveField: " + _sCompleteName); }

        if (_oValue == null) return;

        if (_oValue.GetType().isBasic())
        {
            saveBasicValue(_sCompleteName, _oValue);
        }
        else
        {
            saveBasicValue(_sCompleteName, true);
            saveFields(ref _oValue, _sCompleteName);
        }
    }

    void saveBasicValue(string _sCompleteName, object _oValue)
    {
        m_oInfo.AddValue(_sCompleteName, _oValue);
    }

    #endregion

    #region LOAD

    // this method gets all the field of object and processes them depending the type (array, list, basic individual or class individual)
    void loadFields(ref object _oObject, string _sCompleteName)
    {
        if (_oObject == null)
        {
            return;
        }

        Type oType = _oObject.GetType();
        FieldInfo[] aoFields = oType.GetFields();
        for (int iFieldIndex = 0; iFieldIndex < aoFields.Length; ++iFieldIndex)
        {
            // ignore constants and not serialized fields
            if (aoFields[iFieldIndex].IsLiteral 
                || aoFields[iFieldIndex].IsNotSerialized
                || Attribute.IsDefined(aoFields[iFieldIndex], typeof(DontSaveDataAttribute)))
            {
                continue;
            }

            Type oFieldType = aoFields[iFieldIndex].FieldType;
            string sFieldName = _sCompleteName + "/" + aoFields[iFieldIndex].Name;

            if (oFieldType.IsArray)
            {
                Type oElementType = oFieldType.GetElementType();
                int iLength = (int)loadBasicValue(sFieldName + COUNT, typeof(int));

                // the object doesn't exist so we create an array of the type needed and with the appropiate instances
                Array oArray = Array.CreateInstance(oElementType, iLength);
                for (int iElementIndex = 0; iElementIndex < iLength; ++iElementIndex)
                {
                    object oElementValue = null;
                    loadField(ref oElementValue, sFieldName + "/" + iElementIndex, oElementType);
                    oArray.SetValue(oElementValue, iElementIndex);
                }
                // assign the array
                aoFields[iFieldIndex].SetValue(_oObject, oArray);
            }
            else if (oFieldType.IsGenericType && oFieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type oElementType = oFieldType.GetGenericArguments()[0];

                IList oList = (IList)aoFields[iFieldIndex].GetValue(_oObject);
                int iLength = (int)loadBasicValue(sFieldName + COUNT, typeof(int));
                for (int i = 0; i < iLength; ++i)
                {
                    object oElementValue = null;
                    loadField(ref oElementValue, sFieldName + "/" + i, oElementType);
                    oList.Add(oElementValue);
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(oFieldType))
            {
                IDictionary oDict = (IDictionary)aoFields[iFieldIndex].GetValue(_oObject);
                int iLength = (int)loadBasicValue(sFieldName + COUNT, typeof(int));

                for (int iKeyIndex = 0; iKeyIndex < iLength; ++iKeyIndex)
                {
                    object oDictKey = null;
                    object oDictValue = null;
                    Type oKeyType = oFieldType.GetGenericArguments()[0];
                    Type oValueType = oFieldType.GetGenericArguments()[1];
                    loadField(ref oDictKey, sFieldName + "/" + iKeyIndex + KEY, oKeyType);
                    loadField(ref oDictValue, sFieldName + "/" + iKeyIndex + VALUE, oValueType);

                    oDict[oDictKey] = oDictValue;
                }
            }
            else
            {
                object oElementValue = null;
                loadField(ref oElementValue, sFieldName, oFieldType);
                aoFields[iFieldIndex].SetValue(_oObject, oElementValue);
            }
        }
    }

    void loadField(ref object _oObject, string _sCompleteName, Type _oFieldType)
    {
        try
        {
            if (_oFieldType.isBasic())
            {
                _oObject = loadBasicValue(_sCompleteName, _oFieldType);
            }
            else
            {
                bool bNull = true;
                try
                {
                    m_oInfo.GetBoolean(_sCompleteName);
                    bNull = false;
                }
                catch { }

                if (!bNull)
                {
                    //Debug.Log(_sCompleteName + " -> " + _oFieldType);
                    _oObject = Activator.CreateInstance(_oFieldType);
                    loadFields(ref _oObject, _sCompleteName);
                }
            }
        }
        catch(Exception)
        {
            Debug.LogError($"[AutoSerializableData] Error when trying to deserialize Object:{_oObject} | Type:{_oFieldType} | Name: {_sCompleteName}");
        }
    }

    // read the type of the object, and then try to get the value. If FieldInfo is null, return the value to add it outside of the method (when called to get Array/List where we can not pass the object to put the value in)
    object loadBasicValue(string _sCompleteName, Type _oType)
    {
        //Debug.Log(_sCompleteName + ", " + _oType);

        object oObject;
        try
        {
            oObject = m_oInfo.GetValue(_sCompleteName, _oType);
        }
        catch // Set default value for type
        {
            if (_oType == typeof(string))
            {
                oObject = "";
            }
            else
            {
                oObject = Activator.CreateInstance(_oType);
            }
        }
        return oObject;
    }

    #endregion

    #region Debug Logs
    static bool ms_bLogs = false;
    static StringBuilder ms_sLog = new StringBuilder();

    [RuntimeInitializeOnLoadMethod]
    static void initStatic()
    {
        ms_bLogs = false;
        ms_sLog = new StringBuilder();
    }

    public static void setLogs(bool _bValue)
    {
        ms_bLogs = _bValue;
        if (_bValue) ms_sLog.Clear();
    }

    public static string getLogs() => ms_sLog.ToString();
    #endregion

    #region Extras

    public static void syncAllBeforeSave(object root)
    {
        syncRecursive(root, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    static void syncRecursive(object obj, HashSet<object> visited)
    {
        if (obj == null)
            return;

        if (!obj.GetType().IsValueType)
        {
            if (visited.Contains(obj))
                return;
            visited.Add(obj);
        }

        if (obj is IPreSaveSync sync)
            sync.syncBeforeSave();

        var t = obj.GetType();

        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.IsLiteral || f.IsNotSerialized)
                continue;
            if (Attribute.IsDefined(f, typeof(DontSaveDataAttribute)))
                continue;

            var v = f.GetValue(obj);
            if (v == null)
                continue;

            if (v is Array arr)
                foreach (var e in arr)
                    syncRecursive(e, visited);
            else if (v is IList list)
                foreach (var e in list)
                    syncRecursive(e, visited);
            else if (v is IDictionary dict)
            {
                foreach (var e in dict.Keys)
                    syncRecursive(e, visited);
                foreach (var e in dict.Values)
                    syncRecursive(e, visited);
            }
            else if (!f.FieldType.isBasic())
                syncRecursive(v, visited);
        }
    }


    #endregion
}