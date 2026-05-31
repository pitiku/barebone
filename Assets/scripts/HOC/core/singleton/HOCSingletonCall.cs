using System;
using System.Collections.Generic;
using System.Reflection;

[System.Serializable]
public class HOCSingletonParameter
{
    public string m_sName;
    public string m_sType;
    public string m_sStringValue;
    public int m_iIntValue;
    public float m_fFloatValue;
    public bool m_bBoolValue;
    public UnityEngine.Object m_oObjectValue;

    public object GetValue()
    {
        switch (m_sType)
        {
            case "System.String":
                return m_sStringValue;
            case "System.Int32":
                return m_iIntValue;
            case "System.Single":
                return m_fFloatValue;
            case "System.Boolean":
                return m_bBoolValue;
            default:
                // Handle UnityEngine objects if the type matches
                if (!string.IsNullOrEmpty(m_sType) && m_oObjectValue != null &&
                    m_sType == m_oObjectValue.GetType().ToString())
                {
                    return m_oObjectValue;
                }
                return null;
        }
    }
}

[System.Serializable]
public class HOCSingletonCall
{
    public string m_sSingleton;
    public string m_sMethod;
    public List<HOCSingletonParameter> m_oParameters = new List<HOCSingletonParameter>();

    private object singletonInstance;
    private MethodInfo methodInfo;

    public void initialize()
    {
        if (string.IsNullOrEmpty(m_sSingleton) || string.IsNullOrEmpty(m_sMethod))
            return;

        Type t = Type.GetType(m_sSingleton).BaseType;
        PropertyInfo p = t.GetProperty("Instance");
        singletonInstance = p.GetValue(null, null);

        if (singletonInstance == null)
            return;

        // Find the method with the correct name
        methodInfo = singletonInstance.GetType().GetMethod(m_sMethod);
    }

    public void call()
    {
        if (methodInfo == null || singletonInstance == null)
            return;

        // Prepare parameters
        object[] parameters = new object[m_oParameters.Count];
        for (int i = 0; i < m_oParameters.Count; i++)
        {
            parameters[i] = m_oParameters[i].GetValue();
        }

        // Invoke the method with parameters
        methodInfo.Invoke(singletonInstance, parameters);
    }

    public Type GetSingletonType()
    {
        if (m_sSingleton.isNullOrEmpty())
        {
            return null;
        }

        return m_sSingleton.Length > 0 ? Type.GetType(m_sSingleton) : null;
    }
}
