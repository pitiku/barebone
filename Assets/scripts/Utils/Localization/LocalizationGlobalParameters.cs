using I2.Loc;
using System.Collections.Generic;

public class LocalizationGlobalParameters : Singleton<LocalizationGlobalParameters>, ILocalizationParamsManager
{
    private Dictionary<string, string> m_globalParameters = new Dictionary<string, string>();

    public void CheckInitialized()
    {
        if (!LocalizationManager.ParamManagers.Contains(this))
        {
            LocalizationManager.ParamManagers.Add(this);
            LocalizationManager.LocalizeAll(true);
        }
    }

    public virtual string GetParameterValue(string paramName)
    {
        if (m_globalParameters.ContainsKey(paramName))
        {
            return m_globalParameters[paramName];
        }

        return null;
    }

    public void SetParam(string paramName, string paramValue)
    {
        CheckInitialized();

        m_globalParameters[paramName] = paramValue;
    }
}