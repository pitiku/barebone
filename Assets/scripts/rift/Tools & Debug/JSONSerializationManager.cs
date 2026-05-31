using System;
using UnityEngine;

public class JSONSerializationManager
{
    public static string serialize(object _sRawData, bool _bDebug = false)
    {
        string sSerializedData = JsonUtility.ToJson(_sRawData);
        if (_bDebug)
        {
            Debug.Log($"Serialized JSON: {JsonUtility.ToJson(_sRawData, true)}");
        }

        return sSerializedData;
    }

    public static T deserialize<T>(string _sRawData)
    {
        T oDeserializedData;
        try
        {
            oDeserializedData = JsonUtility.FromJson<T>(_sRawData);
        }
        catch (Exception e)
        {
            Deb.logError($"[Analytics] Failed to generate a {typeof(T)} class instance | Error: {e.GetType()} Message: {e.Message} | Data: {_sRawData}");
            oDeserializedData = default(T);
        }

        return oDeserializedData;

    }

    public static System.Object deserialize(string _sRawData, Type _oType)
    {
        System.Object oObject = Activator.CreateInstance(_oType);

        JsonUtility.FromJsonOverwrite(_sRawData, oObject);

        return oObject;
    }
}
