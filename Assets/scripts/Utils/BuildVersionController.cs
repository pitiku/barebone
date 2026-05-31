using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BuildVersionController : MonoBehaviour
{
    [Header("Versions")]
    [Title("MAIN")]
    [GUIColor(0.5f, 0.9f, 1.0f, 1.0f)]
    public List<GameObject> m_aoMainObjects;
    [GUIColor(0.5f, 0.9f, 1.0f, 1.0f)]
    public List<MonoBehaviour> m_aoMainComponents;

    [Title("DEMO")]
    [GUIColor(1.0f, 0.4f, 0.4f, 1.0f)]
    public List<GameObject> m_aoDemoObjects;
    [GUIColor(1.0f, 0.4f, 0.4f, 1.0f)]
    public List<Component> m_aoDemoComponents;

    [Header("Language")]
    [Title("Chinese")]
    [GUIColor(0.5f, 0.9f, 1.0f, 1.0f)]
    public List<GameObject> m_aoChineseObjects;

    [Title("English")]
    [GUIColor(1.0f, 0.4f, 0.4f, 1.0f)]
    public List<GameObject> m_aoEnglishObjects;

    private void Awake()
    {
#if UNITY_EDITOR
        checkVersion(false);
#endif
        setLanguage();
        I2.Loc.LocalizationManager.OnLocalizeEvent += setLanguage;

#if !COMPENDIUM && UNITY_EDITOR
        CompendiumManager.Instance?.gameObject.destroy(0,true);
        CompendiumContainer.Instance?.gameObject.destroy(0, true);
#endif
    }

    private void OnDestroy()
    {
        I2.Loc.LocalizationManager.OnLocalizeEvent -= setLanguage;
    }

    #region Version

    public void checkVersion(bool _bImmediate)
    {
#if !VERSION_MAIN
        deleteVersionMain(_bImmediate);
#elif !VERSION_DEMO
        deleteVersionDemo(_bImmediate);
#endif
    }
    void deleteVersionMain(bool _bImmediate)
    {
        for (int i = 0; i < m_aoMainObjects.Count; ++i)
        {
            GameObject itObject = m_aoMainObjects[i];

            if (itObject == null) { continue; }

            itObject.SetActive(false);
            itObject.destroy(0, _bImmediate);
        }

        for (int i = 0; i < m_aoMainComponents.Count; ++i)
        {
            Component itComponent = m_aoMainComponents[i];

            if (itComponent == null) { continue; }

            Behaviour itBehaviour = itComponent as Behaviour;
            if (itBehaviour != null) { itBehaviour.enabled = false; }

            itComponent.destroy(0, _bImmediate);
        }
        m_aoMainObjects.Clear();
        m_aoMainComponents.Clear();
    }

    public void deleteVersionDemo(bool _bImmediate)
    {
        for (int i = 0; i < m_aoDemoObjects.Count; ++i)
        {
            GameObject itObject = m_aoDemoObjects[i];

            if (itObject == null) { continue; }

            itObject.SetActive(false);
            itObject.destroy(0, _bImmediate);
        }

        for (int i = 0; i < m_aoDemoComponents.Count; ++i)
        {
            Component itComponent = m_aoDemoComponents[i];

            if (itComponent == null) { continue; }

            Behaviour itBehaviour = itComponent as Behaviour;
            if (itBehaviour != null) { itBehaviour.enabled = false; }

            itComponent.destroy(0, _bImmediate);
        }
        m_aoDemoObjects.Clear();
        m_aoDemoComponents.Clear();
    }

    #endregion

    #region Language

    void setLanguage()
    {
        string sLanguageCode = I2.Loc.LocalizationManager.CurrentLanguageCode;
        for (int i = 0; i < m_aoChineseObjects.Count; ++i)
        {
            GameObject itObject = m_aoChineseObjects[i];

            if (itObject == null) { continue; }

            itObject.SetActive(sLanguageCode == Localization.CHINESE_CODE);
        }
        for (int i = 0; i < m_aoEnglishObjects.Count; ++i)
        {
            GameObject itObject = m_aoEnglishObjects[i];

            if (itObject == null) { continue; }

            itObject.SetActive(sLanguageCode == Localization.ENGLISH_CODE);
        }
    }

    #endregion

    // ONLY HERE TO ALLOW THE ENABLED CHECKBOX OF STATEBEHAVIORs
    void Start() { }
}