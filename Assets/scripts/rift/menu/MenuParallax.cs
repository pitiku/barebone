using System.Collections.Generic;
using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    [SerializeField] Vector2 m_vSpeedXMinMax = new Vector2(.4f, .8f);
    [SerializeField] Vector2 m_vSpeedYMinMax = new Vector2(.4f, .8f);
    [SerializeField] Vector2 m_vMovementXMinMax = new Vector2(.1f, .2f);
    [SerializeField] Vector2 m_vMovementYMinMax = new Vector2(.1f, .2f);

    [SerializeField] float m_fMaxMovementMouseX = .3f;
    [SerializeField] float m_fMaxMovementMouseY = .3f;

    [SerializeField, Tooltip("Smaller value means faster speed"), Min(0)] float m_fSmoothTime = 0.5f;

    List<MenuParallaxItem> m_aoItems;

    Vector2 m_vMouse = Vector2.zero;
    Vector2 m_vVelocity = Vector2.one * 10;

    float m_fMovementX;
    float m_fMovementY;
    float m_fSpeedX;
    float m_fSpeedY;

    void Start()
    {
        m_aoItems = GetComponentsInChildren<MenuParallaxItem>().toList();

        for (int iIndex = 0; iIndex < m_aoItems.Count; ++iIndex)
        {
            m_aoItems[iIndex].initialize();
        }

        m_fMovementX = m_vMovementXMinMax.range();
        m_fMovementY = m_vMovementYMinMax.range();
        m_fSpeedX = m_vSpeedXMinMax.range() * RandomUtils.value("[MenuParallax] Speed X") < 0.5f ? 1 : -1;
        m_fSpeedY = m_vSpeedYMinMax.range() * RandomUtils.value("[MenuParallax] Speed Y") < 0.5f ? 1 : -1;
    }

    void Update()
    {
        Vector2 vMouse;

        if (RewiredManager.USING_GAMEPAD)
        {
            vMouse = RewiredManager.Instance.GetAnyStickRightV2(false);
        }
        else
        {
            vMouse = (new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height) * 2) - Vector2.right - Vector2.up;
            //vMouse = Input.mousePosition.xy() - Vector2.right * Screen.width / 2 - Vector2.up * Screen.height / 2;
            //vMouse = new Vector2(m_fMaxMovementMouseX * vMouse.x / (Screen.width * 0.5f), m_fMaxMovementMouseY * vMouse.y / (Screen.height * 0.5f));
        }
        vMouse = new Vector2(vMouse.x * m_fMaxMovementMouseX, vMouse.y * m_fMaxMovementMouseY);

        m_vMouse = Vector2.SmoothDamp(m_vMouse, vMouse, ref m_vVelocity, m_fSmoothTime);

        float fValueX = Time.time * m_fSpeedX;
        float fValueY = Time.time * m_fSpeedY;
        Vector2 vValue = (Vector2.right * Mathf.Sin(fValueX) * m_fMovementX) + (Vector2.up * Mathf.Cos(fValueY) * m_fMovementY);

        for (int iIndex = 0; iIndex < m_aoItems.Count; ++iIndex)
        {
            m_aoItems[iIndex].updateMovement(m_vMouse + vValue);
        }
    }
}
