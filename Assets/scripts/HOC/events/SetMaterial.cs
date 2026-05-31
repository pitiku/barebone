using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SetMaterial : TimedEvent
{
    [InfoBox("Change material of an Image or a SpriteRenderer")]
    [SerializeField] GameObject m_oObject;
    [SerializeField] Material m_oMaterial;
    public override void play()
    {
        base.play();

        if (m_oObject == null) { return; }

        Image oImage = m_oObject.GetComponent<Image>();
        if (oImage)
        {
            oImage.material = m_oMaterial;
        }
        SpriteRenderer oSPrite = m_oObject.GetComponent<SpriteRenderer>();
        if (oSPrite)
        {
            oSPrite.material = m_oMaterial;
        }
    }
}
