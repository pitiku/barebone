using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIRaycastDebugger : MonoBehaviour
{
    [Header("References")]
    public GraphicRaycaster uiRaycaster;    // El GraphicRaycaster de tu Canvas
    public EventSystem eventSystem;         // El EventSystem de la escena

    [Header("Settings")]
    public KeyCode debugKey = KeyCode.Space; // Tecla para disparar el debug (por defecto: Space)

    private void Awake()
    {
        uiRaycaster = GetComponent<GraphicRaycaster>();
        eventSystem = RewiredManager.Instance.m_rewiredInputModule.GetComponent<EventSystem>();
    }
    void Update()
    {
        // Dispara la depuración al presionar la tecla configurada
        if (Input.GetKeyDown(debugKey))
        {
            DebugRaycastAtPointer();
        }
    }

    void DebugRaycastAtPointer()
    {
        // Construir datos del puntero
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        // Raycast UI
        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(pointerData, results);

        if (results.Count == 0)
        {
            Deb.logWarning("UIRaycastDebugger: No UI elements detected under pointer.");
            return;
        }

        Deb.log("UIRaycastDebugger: Elements under pointer (front to back):");
        for (int i = 0; i < results.Count; i++)
        {
            RaycastResult r = results[i];
            string blocker = (i == 0) ? "<-- First (topmost) blocker" : "";
            Debug.LogFormat(
                "{0}. GameObject: {1}, CanvasRenderer? {2} {3}",
                i + 1,
                r.gameObject.name,
                r.gameObject.GetComponent<CanvasRenderer>() != null,
                blocker
            );
        }
    }
}