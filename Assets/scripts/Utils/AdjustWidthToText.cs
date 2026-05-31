using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dynamically adjusts the width of a UI LayoutElement based on the combined widths of child components.
/// Supports multiple component types (TextMeshPro, LayoutElement, RectTransform) and allows for
/// custom minimum/maximum width constraints. Includes real-time editor preview of calculations.
/// </summary>
public class AdjustWidthToText : MonoBehaviour
{
    [System.Serializable]
    public class StuffList
    {
        [Tooltip("Components whose width will be summed to calculate the required container width")]
        public List<Component> m_aoComponents = new List<Component>();
    }

    [BoxGroup("Core Settings", true)]
    [Tooltip("The LayoutElement component that will have its width adjusted. Usually the parent container.")]
    public UnityEngine.UI.LayoutElement m_oElement;

    [BoxGroup("Core Settings")]
    [Tooltip("List of component groups. The width is calculated as the maximum of all groups combined.")]
    [ShowIf("@m_oElement != null")]
    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true)]
    public List<StuffList> m_aoStuff = new List<StuffList>();

    [BoxGroup("Core Settings")]
    [Min(0)]
    [Tooltip("Additional padding added to the final calculated width, not surpassing the max.")]
    [ShowIf("@m_oElement != null")]
    public float m_fExtraWidth = 1;

    [BoxGroup("Core Settings")]
    [Tooltip("X = Minimum allowed width, Y = Maximum allowed width (unless overridden)")]
    [ShowIf("@m_oElement != null")]
    public Vector2 m_vWidth = new Vector2(5, 9);

    [BoxGroup("Core Settings")]
    [ShowIf("@m_oElement != null")]
    public bool m_bDontForceWidthFromOutside = false;

    [BoxGroup("Override Settings")]
    [Tooltip("When enabled, objects in 'm_aoOverrideMaxStuff' will be used to calculate a dynamic maximum width")]
    [ShowIf("@m_oElement != null")]
    public bool m_bOverrideMaxWidth = false;

    [BoxGroup("Override Settings")]
    [ShowIf("@m_oElement != null && m_bOverrideMaxWidth")]
    [Tooltip("Components in these groups will determine the maximum width if their combined width exceeds the default maximum")]
    [ListDrawerSettings(ShowIndexLabels = true, ShowPaging = false, ShowItemCount = true)]
    public List<StuffList> m_aoOverrideMaxStuff = new List<StuffList>();

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("The current forced width being applied (-1 = no forced width)")]
    [ShowIf("@m_oElement != null")]
    public float m_fForcedWidth = -1;

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("The calculated width before clamping to min/max values")]
    [ShowIf("@m_oElement != null")]
    public float m_fCalculatedWidth = 0;

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("The final width that will be applied to the LayoutElement")]
    [ShowIf("@m_oElement != null")]
    public float m_fFinalWidth = 0;

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("Current maximum width constraint")]
    [ShowIf("@m_oElement != null")]
    public float m_fCurrentMaxWidth = 0;

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("Total width from primary component groups")]
    [ShowIf("@m_oElement != null")]
    public float m_fPrimaryGroupsWidth = 0;

    [BoxGroup("Debug Info")]
    [ReadOnly]
    [Tooltip("Total width from override component groups")]
    [ShowIf("@m_oElement != null")]
    public float m_fOverrideGroupsWidth = 0;

    private void OnEnable()
    {
        resetForcedWidth();
#if UNITY_EDITOR
        // Enable editor updates even when not playing
        EditorApplication.update += EditorUpdate;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
    }

    private void LateUpdate()
    {
        calculateCustomWidth();
    }

#if UNITY_EDITOR
    private void EditorUpdate()
    {
        // Calculate and display preview in editor when not playing
        if (!Application.isPlaying)
        {
            calculateCustomWidth();
        }
    }
#endif

    public void setWidth(float _fWidth)
    {
        if (!m_bDontForceWidthFromOutside)
        {
            m_fForcedWidth = _fWidth;
            calculateCustomWidth();
        }
    }

    public void resetForcedWidth()
    {
        m_fForcedWidth = -1;
    }

    public void calculateCustomWidth()
    {
        if (m_oElement == null)
        {
            logWarning("LayoutElement is not assigned. Cannot calculate width.");
            return;
        }

        // If a forced width is set, apply it directly
        if (m_fForcedWidth > 0)
        {
            m_fCalculatedWidth = m_fForcedWidth;
            m_fFinalWidth = m_fForcedWidth;
            m_oElement.preferredWidth = m_fForcedWidth;
            return;
        }

        // Set minimum width
        m_oElement.minWidth = m_vWidth.x;

        // Calculate width from primary component groups
        m_fPrimaryGroupsWidth = calculateGroupsWidth(m_aoStuff);
        float fCalculatedWidth = Mathf.Max(m_vWidth.x, m_fPrimaryGroupsWidth);

        // Determine the maximum width constraint
        m_fCurrentMaxWidth = m_vWidth.y;
        if (m_bOverrideMaxWidth)
        {
            m_fOverrideGroupsWidth = calculateGroupsWidth(m_aoOverrideMaxStuff);
            m_fCurrentMaxWidth = Mathf.Max(m_vWidth.y, m_fOverrideGroupsWidth);
        }
        else
        {
            m_fOverrideGroupsWidth = 0;
        }

        // Apply extra width and clamp to constraints
        m_fCalculatedWidth = fCalculatedWidth;
        m_fFinalWidth = Mathf.Clamp(fCalculatedWidth + m_fExtraWidth, m_vWidth.x, m_fCurrentMaxWidth);
        m_oElement.preferredWidth = m_fFinalWidth;
    }

    private float calculateGroupsWidth(List<StuffList> _aoGroups)
    {
        if (_aoGroups == null || _aoGroups.Count == 0)
            return 0;

        float fMaxWidth = 0;

        foreach (StuffList oList in _aoGroups)
        {
            float fCurrentGroupWidth = calculateSingleGroupWidth(oList);
            fMaxWidth = Mathf.Max(fMaxWidth, fCurrentGroupWidth);
        }

        return fMaxWidth;
    }

    private float calculateSingleGroupWidth(StuffList _oList)
    {
        if (_oList?.m_aoComponents == null || _oList.m_aoComponents.Count == 0)
            return 0;

        float fCurrentWidth = 0;

        foreach (Component oComponent in _oList.m_aoComponents)
        {
            if (oComponent == null)
            {
                logWarning("Null component found in StuffList. Skipping.");
                continue;
            }

            // Skip inactive GameObjects
            if (!oComponent.gameObject.activeSelf)
            {
                continue;
            }

            float fComponentWidth = getComponentWidth(oComponent);
            fCurrentWidth += fComponentWidth;
        }

        return fCurrentWidth;
    }

    private float getComponentWidth(Component _oComponent)
    {
        if (_oComponent is TMP_Text oTextComponent)
        {
            return oTextComponent.preferredWidth;
        }
        else if (_oComponent is UnityEngine.UI.LayoutElement oLayoutElement)
        {
            return oLayoutElement.preferredWidth;
        }
        else if (_oComponent is RectTransform oRectTransform)
        {
            return oRectTransform.rect.width;
        }

        logWarning($"Component type '{_oComponent.GetType().Name}' is not supported for width calculation.");
        return 0;
    }

    private void logWarning(string _sMessage)
    {
        Deb.logWarning($"[{gameObject.name}] {_sMessage}");
    }

#if UNITY_EDITOR
    [BoxGroup("Editor Tools")]
    [Button("Recalculate (Editor)")]
    private void EditorRecalculate()
    {
        calculateCustomWidth();
        EditorUtility.SetDirty(this);
    }

    [BoxGroup("Editor Tools")]
    [Button("Log Width Breakdown")]
    private void LogWidthBreakdown()
    {
        Debug.Log($"=== Width Calculation Breakdown for '{gameObject.name}' ===\n" +
            $"Forced Width: {(m_fForcedWidth > 0 ? m_fForcedWidth : "Not Set")}\n" +
            $"Primary Groups Width: {m_fPrimaryGroupsWidth}\n" +
            $"Override Groups Width: {(m_bOverrideMaxWidth ? m_fOverrideGroupsWidth : "Disabled")}\n" +
            $"Extra Width Padding: {m_fExtraWidth}\n" +
            $"Min Width Constraint: {m_vWidth.x}\n" +
            $"Max Width Constraint: {m_fCurrentMaxWidth}\n" +
            $"Calculated Width (before clamp): {m_fCalculatedWidth}\n" +
            $"Final Width (applied): {m_fFinalWidth}\n" +
            $"===================================================\n", gameObject);
    }

    private void OnValidate()
    {
        if (m_vWidth.x < 0)
            m_vWidth.x = 0;

        if (m_vWidth.y < m_vWidth.x)
            m_vWidth.y = m_vWidth.x;

        if (m_fExtraWidth < 0)
            m_fExtraWidth = 0;
    }
#endif
}