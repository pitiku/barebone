using UnityEngine;

public class AdjustWidthToTextChild : MonoBehaviour
{
    private void OnEnable()
    {
        AdjustWidthToText parent = GetComponentInParent<AdjustWidthToText>();
        if (parent != null)
        {
            Component[] aoComponents = gameObject.GetComponents<Component>();
            for (int i = 0; i < aoComponents.Length; i++)
            {
                parent.m_aoStuff.Add(new AdjustWidthToText.StuffList());
                parent.m_aoStuff[^1].m_aoComponents.Add(aoComponents[i]);
            }
        }
    }

    private void OnDisable()
    {
        OnRectTransformRemoved();
    }

    private void OnDestroy()
    {
        OnRectTransformRemoved();
    }

    private void OnRectTransformRemoved()
    {
        AdjustWidthToText parent = GetComponentInParent<AdjustWidthToText>();
        if (parent != null)
        {
            for (int i = 0; i < parent.m_aoStuff.Count; i++)
            {
                if (parent.m_aoStuff[i].m_aoComponents.Contains(this))
                {
                    parent.m_aoStuff.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
