using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, UnityEvent> m_dEventDictionary = new Dictionary<string, UnityEvent>();

    public void StartListening(string _sEventName, UnityAction _oListener)
    {
        _sEventName = _sEventName.toLower();
        UnityEvent oEvent = null;
        if (m_dEventDictionary.TryGetValue(_sEventName, out oEvent))
        {
            oEvent.AddListener(_oListener);
        }
        else
        {
            oEvent = new UnityEvent();
            oEvent.AddListener(_oListener);
            m_dEventDictionary.Add(_sEventName, oEvent);
        }
    }

    public void StopListening(string _sEventName, UnityAction _oListener)
    {
        _sEventName = _sEventName.toLower();
        UnityEvent oEvent = null;
        if (m_dEventDictionary.TryGetValue(_sEventName, out oEvent))
        {
            oEvent.RemoveListener(_oListener);
        }
    }

    public void TriggerEvent(string _sEventName)
    {
        _sEventName = _sEventName.toLower();
        UnityEvent thisEvent = null;
        if (m_dEventDictionary.TryGetValue(_sEventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    //private static Action<T> m_aoAction;
    // Método para suscribirse a la señal

    private static Dictionary<Type, Action<object>> m_oActionsByType = new Dictionary<Type, Action<object>>();
    private static Dictionary<Type, Type[]> m_oCachedInterfacesByType = new Dictionary<Type, Type[]>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Dispose()
    {
        m_oActionsByType = new Dictionary<Type, Action<object>>();
        m_oCachedInterfacesByType = new Dictionary<Type, Type[]>();
    }
    public static IDisposable Subscribe<T>(Action<T> action)
    {
        Action<object> objectAction = oDelegate => action((T)oDelegate);
        Deb.log("Subscribed to " + typeof(T).Name + " by action " + action.Method.Name, eLogFlags.SIGNALBUS);
        Type paramType = typeof(T);
        if (!m_oActionsByType.TryAdd(paramType, objectAction))
        {
            m_oActionsByType[paramType] += objectAction;
        }

        return new Subscription(paramType, objectAction, action.Method.Name);
    }

    public static void TriggerEventsWithArgs<T>(T t)
    {
        if (m_oActionsByType.TryGetValue(typeof(T), out var action))
        {
            if (action == null) Deb.log("Found a null action in SignalBus " + t.ToString());
            action(t);
        }
        else Deb.log($"Type of {typeof(T)} is trying to be fired as signal but is not subscribed anywhere", eLogFlags.SIGNALBUS);
    }

    public static void AbstractTrigger<T>(T t, bool includeConcrete = true)
    {
        if (includeConcrete) TriggerEventsWithArgs(t);

        Type[] types = GetOrCreateInterfaceArrayForType(typeof(T));
        foreach (Type type in types)
        {
            if (m_oActionsByType.TryGetValue(type, out var actionFromDic))
                actionFromDic?.Invoke(t);
        }
    }

    static Type[] GetOrCreateInterfaceArrayForType(Type type)
    {
        if (m_oCachedInterfacesByType.TryGetValue(type, out Type[] oInterfaces))
            return oInterfaces;

        oInterfaces = type.GetInterfaces();
        m_oCachedInterfacesByType[type] = oInterfaces.Length > 0 ? oInterfaces : Array.Empty<Type>();
        return oInterfaces;
    }

    public static void Unsubscribe(Type paramType, Action<object> action, string sMethodName = null)
    {
        if (m_oActionsByType.ContainsKey(paramType))
        {
            m_oActionsByType[paramType] -= action;
            if (m_oActionsByType[paramType] == null) m_oActionsByType.Remove(paramType);
        }
    }
    // WARNING: Esto es una simple comodidad para disparar sin argumentos...
    // Si un suscriptor está esperando un valor y no hace un null check tendrás un error, cuidado con esto.
    private class Subscription : IDisposable
    {
        private readonly Type m_oParamType;
        private readonly Action<object> m_oBoundAction;
        private readonly Action<object> m_oUnboundAction;
        private string m_sMethodName = "";
        public Subscription(Type oParamType, Action<object> oBoundAction, string sMethodName)
        {
            m_oParamType = oParamType;
            m_oBoundAction = oBoundAction;
            m_sMethodName = sMethodName;
        }
        public void Dispose() => Unsubscribe(m_oParamType, m_oBoundAction, m_sMethodName);

    }
}

public static class EventUtils
{
    public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable) => compositeDisposable.Add(disposable);
}
// puedes desuscribir manualmente cada evento pero para dejar el codigo mas limpio simplemente añades cada señal a esta lista y luego en en OnDestroy/HOC.deactivate() haces un CompsiteDispose.Dispose()
public class CompositeDisposable : IDisposable
{
    List<IDisposable> m_aoDispables = new List<IDisposable>();

    public void Dispose()
    {
        for (int i = 0; i < m_aoDispables.Count; i++)
        {
            m_aoDispables[i].Dispose();
        }

        m_aoDispables.Clear();
    }

    public void Add(IDisposable disposable) => m_aoDispables.Add(disposable);
}
