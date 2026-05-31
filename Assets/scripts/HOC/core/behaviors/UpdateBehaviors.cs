using System.Linq;
using UnityEngine;

public class UpdateBehaviors : Behavior
{
    enum eGetComponentType { Parent, Children }

    public GameObject m_oContainer;
    [SerializeField] eGetComponentType m_eGetComponentFrom;
    Behavior[] m_aoBehaviors;


    public override void activate()
    {
        base.activate();

        if (m_aoBehaviors.isNullOrEmpty())
        {
            if (m_eGetComponentFrom == eGetComponentType.Parent)
            {
                m_aoBehaviors = m_oContainer.GetComponents<Behavior>();
            }
            else
            {
                m_aoBehaviors = m_oContainer.GetComponentsInChildren<Behavior>().Where(oBehavior => oBehavior.transform != transform).ToArray();
            }
        }

        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            m_aoBehaviors[i].activate();
        }
    }

    public override void update()
    {
        base.update();

        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            m_aoBehaviors[i].update();
        }

        bool bAnyBehaviorLeft = false;
        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            if (!m_aoBehaviors[i].m_bFinished)
            {
                bAnyBehaviorLeft = true;
                break;
            }
        }

        if (!bAnyBehaviorLeft)
        {
            m_bFinished = true;
        }
    }

    public override void deactivate()
    {
        base.deactivate();

        for (int i = 0; i < m_aoBehaviors.Length; ++i)
        {
            m_aoBehaviors[i].deactivate();
        }
    }
}
