using TMPro;
using UnityEngine;

public class StateVisualDebug : MonoBehaviour
{
    public State[] m_aoStates;

    private Camera m_oCamera;
    private TextMeshPro m_oText;

    public void Awake()
    {
        m_oCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        m_oText = GetComponent<TextMeshPro>();
    }

    public void Update()
    {
        transform.LookAt(m_oCamera.transform.position, m_oCamera.transform.up);

        m_oText.text = "";
        for (int index = 0; index < m_aoStates.Length; ++index)
        {
            State oState = m_aoStates[index];
            if (oState.isActiveAndEnabled && oState.getActiveChildState() != null)
            {
                m_oText.text += (m_oText.text.isNullOrEmpty() ? "" : "\n") + oState.getActiveChildState().name;
            }
        }
    }
}
