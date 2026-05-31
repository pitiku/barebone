using Sirenix.OdinInspector;
using System;

[Flags]
public enum eActivatorByInput
{
    KeyboardMouse = 1,
    Controller = 2,
}

[Serializable]
public class ActivatorDataByInput : ActivatorData
{
    [PropertyOrder(-1)]
    public eActivatorByInput m_eInput;

    public void update(eActivatorByInput _e)
    {
        activate(m_eInput != _e);
    }
}