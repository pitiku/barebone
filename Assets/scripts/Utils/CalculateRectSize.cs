using Sirenix.OdinInspector;
using UnityEngine;

public class CalculateRectSize : MonoBehaviour
{
    RectTransform m_oRectTransform;
    [Button("Calculate")]
    void calculate()
    {
        Awake();
        Update();
    }
    private void Awake()
    {
        m_oRectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        float fMaxY = 0;
        float fMaxX = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform oChild = transform.GetChild(i).GetComponent<RectTransform>();
            if (oChild != null && oChild.gameObject.activeSelf)
            {
                if (fMaxX < getWidth(oChild, getOriginX()))
                {
                    fMaxX = getWidth(oChild, getOriginX());
                }
                if (fMaxY < getHeight(oChild, getOriginY()))
                {
                    fMaxY = getHeight(oChild, getOriginY());
                }
            }
        }
        m_oRectTransform.sizeDelta = new Vector2(fMaxX, fMaxY);

    }

    float getWidth(RectTransform oChild, float _fPosX)
    {
        return (_fPosX + oChild.localPosition.x) + (oChild.localPosition.x - (oChild.pivot.x * oChild.rect.width)) + oChild.rect.width;
    }

    float getHeight(RectTransform oChild, float _fPosY)
    {
        return (_fPosY + oChild.localPosition.y) + oChild.localPosition.y - (oChild.pivot.y * oChild.rect.height) + oChild.rect.height;
    }

    float getOriginX()
    {
        return m_oRectTransform.localPosition.x;
    }

    float getOriginY()
    {
        return m_oRectTransform.localPosition.y;
    }

    private void OnDrawGizmosSelected()
    {
        //m_oRectTransform = GetComponent<RectTransform>();

        //Update();
    }
}
