using System;
using System.Reflection;

[System.Serializable]
public class HOCSingletonBoolCall : HOCSingletonTypedCall<bool>
{
    public HOCSingletonBoolCall() { }
}

[System.Serializable]
public class HOCSingletonTypedCall<TResult>
{
    public string m_sSingleton;
    public string m_sMethod;

    Func<TResult> func;

    public void initialize()
    {
        Type t = Type.GetType(m_sSingleton).BaseType;
        PropertyInfo p = t.GetProperty("Instance");
        object oObject = p.GetValue(null, null);

        func = (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), oObject, m_sMethod);
    }

    public TResult call()
    {
        return func();
    }

    public Type GetSingletonType()
    {
        return m_sSingleton.Length > 0 ? Type.GetType(m_sSingleton) : null;
    }
}
