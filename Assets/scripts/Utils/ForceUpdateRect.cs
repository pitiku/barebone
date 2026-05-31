using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ForceUpdateRect : MonoBehaviour
{
    RectTransform m_oTransform;
    public bool m_bLocalized;

    private void Awake()
    {
        m_oTransform = GetComponent<RectTransform>();

        if (m_bLocalized) I2.Loc.LocalizationManager.OnLocalizeEvent += updateRect;
    }

    int _waitFrames = 3;
    private void OnEnable()
    {
        _waitFrames = 3;
    }
    private void LateUpdate()
    {
        if (_waitFrames > 0)
        {
            _waitFrames--;
            updateRect();
        }
    }

    private void OnDestroy()
    {
        if (m_bLocalized) I2.Loc.LocalizationManager.OnLocalizeEvent -= updateRect;
    }

    [Button("d")]
    void updateRect()
    {
        if (m_oTransform != null)
        {
            m_oTransform.ForceUpdateRectTransforms();
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_oTransform);
            LayoutRebuilder.MarkLayoutForRebuild(m_oTransform);
        }
    }
}
