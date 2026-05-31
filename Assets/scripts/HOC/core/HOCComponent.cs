using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

[Serializable]
public class HOCComponentAnimator : HOCComponent<Animator>
{
    public HOCComponentAnimator() { }
}

[Serializable]
public class HOCComponentTextMeshPro : HOCComponent<TextMeshPro>
{
    public HOCComponentTextMeshPro() { }
}

[Serializable]
public class HOCComponentStatesManager : HOCComponent<GameStateManager>
{
    public HOCComponentStatesManager() { }
}

[Serializable]
public class HOCComponentStateMachine : HOCComponent<StateMachine>
{
    public HOCComponentStateMachine() { }
}

[Serializable]
public class HOCComponentState : HOCComponent<State>
{
    public HOCComponentState() { }
}

[Serializable]
public class HOCTransform : HOCComponent<Transform>
{
    public HOCTransform() { }
}

[Serializable]
public class HOCComponentGeneric : HOCComponent<Component>
{
    public HOCComponentGeneric() { }
}

[Serializable]
public class HOCComponent<T> where T : Component
{
    [SerializeField, LabelText("Type")] private eReferenceType m_iReference;
    [SerializeField, LabelText("Value"), ShowIf("@m_iReference == eReferenceType.Parents || m_iReference == eReferenceType.Direct")] T m_oValue; // this one is public because it nees to be accessed from the property drawer, but shouldn't be accessed directly!!!
    [SerializeField, LabelText("Name"), ShowIf("@m_iReference != eReferenceType.Parents && m_iReference != eReferenceType.Direct")] private string m_sName;

    bool m_bInitialized = false;

    public void setManually(T _o)
    {
        m_oValue = _o;
        m_bInitialized = true;
    }

    public T get()
    {
        if (!m_bInitialized) initialize();

        return m_oValue;
    }

    public virtual void initialize()
    {
        switch (m_iReference)
        {
            case eReferenceType.Direct:
                break;
            case eReferenceType.Parents:
                if (m_oValue == null)
                {
                    Deb.logError("HOCTransform component at some object is set as parents reference but doesnt have a self reference (needed in order to search in its parents)");
                }
                if (m_oValue != null)
                {
                    m_oValue = m_oValue.transform.getComponentInParents<T>(true); ;
                }
                break;
            case eReferenceType.Type:
                Type oType = Type.GetType(m_sName);
                T oTypeO = (T)GameObject.FindAnyObjectByType(oType, FindObjectsInactive.Include);
                if (oTypeO != null)
                {
                    m_oValue = oTypeO.GetComponent<T>();
                }
                break;
            case eReferenceType.Tag:
                GameObject oTagO = GameObject.FindGameObjectWithTag(m_sName);
                if (oTagO != null)
                {
                    m_oValue = oTagO.GetComponent<T>();
                }
                break;
            case eReferenceType.Name:
                if (m_sName.isNullOrEmpty())
                {
                    Deb.logError("HOCTransform component at some object is set as reference by name but name is empty.");
                }
                else
                {
                    GameObject oGO = null;
                    T[] oPossibles = GameObject.FindObjectsByType<T>(0);
                    for (int i = 0; i < oPossibles.Length; ++i)
                    {
                        if (oPossibles[i].name == m_sName)
                        {
                            oGO = oPossibles[i].gameObject;
                            break;
                        }
                    }

                    if (oGO == null)
                    {
                        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
                        {
                            if (t.gameObject.name == m_sName)
                            {
                                oGO = t.gameObject;
                                break;
                            }
                        }
                    }
                    if (oGO != null)
                    {
                        m_oValue = oGO.GetComponent<T>();
                    }
                }
                break;
        }

        m_bInitialized = true;
    }
}
