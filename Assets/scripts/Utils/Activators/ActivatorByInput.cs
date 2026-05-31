using System;
using UnityEngine;

[Serializable]
public class ActivatorByInput : MonoBehaviour
{
    public ActivatorDataByInput[] m_aoDatas;

    public void Awake()
    {
        this.execute(RewiredManager.USING_GAMEPAD ? eActivatorByInput.Controller : eActivatorByInput.KeyboardMouse);
    }
}

