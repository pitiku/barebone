using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShadowShapesUpdater : MonoBehaviour
{
    public bool m_bExecuteOnStart = true;
    public float m_fBackDistance = 0.1f;
    public float m_fUpDistance = 0.06f;
    public float m_fBackRotation = 10.0f;

    static List<Transform> m_aoShapes = new List<Transform>();

    [Button("move")]
    public void move()
    {
        updateShadowShapes(true);
    }

    [Button("undo")]
    public void undo()
    {
        updateShadowShapes(false);
    }

    [Button("enable mesh renderers")]
    public void amr()
    {
        enableMeshRenderers(true);
    }

    [Button("disable mesh renderers")]
    public void dmr()
    {
        enableMeshRenderers(false);
    }

    void Start()
    {
        enableMeshRenderers(true);

        if (m_bExecuteOnStart)
        {
            updateShadowShapes(true);
        }
    }

    public void enableMeshRenderers(bool _bEnable)
    {
        m_aoShapes.Clear();
        transform.getChildsRegex(ref m_aoShapes, "^3D_"); // get objects starting with 3D_

        for (int i = 0; i < m_aoShapes.Count; ++i)
        {
            m_aoShapes[i].GetComponent<MeshRenderer>().enabled = _bEnable;
        }
    }

    public void updateShadowShapes(bool _bPositiveOrNegative)
    {
        m_aoShapes.Clear();
        transform.getChildsRegex(ref m_aoShapes, "^3D_"); // get objects starting with 3D_

        float fBack = _bPositiveOrNegative ? m_fBackDistance : -m_fBackDistance;
        float fUp = _bPositiveOrNegative ? m_fUpDistance : -m_fUpDistance;
        float fAngle = _bPositiveOrNegative ? m_fBackRotation : -m_fBackRotation;

        for (int i = 0; i < m_aoShapes.Count; ++i)
        {
            m_aoShapes[i].Translate((Vector3.forward * fBack) + (Vector3.up * fUp));
            m_aoShapes[i].RotateAround(transform.position, Vector3.right, fAngle);
        }
    }
}
