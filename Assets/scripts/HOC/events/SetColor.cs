using UnityEngine;
using UnityEngine.UI;

public class SetColor : TimedEvent
{
    public Component m_oComponent;
    public Color m_cColor;

    public override void play()
    {
        base.play();

        if (m_oComponent is Graphic) //Graphics includes TMP_Text, Image and more
        {
            ((Graphic)m_oComponent).color = m_cColor;
        }
        else if (m_oComponent.GetComponent<Graphic>() != null)
        {
            m_oComponent.GetComponent<Graphic>().color = m_cColor;
        }
        else if (m_oComponent is SpriteRenderer)
        {
            ((SpriteRenderer)m_oComponent).color = m_cColor;
        }
        else if (m_oComponent.GetComponent<SpriteRenderer>() != null)
        {
            m_oComponent.GetComponent<SpriteRenderer>().color = m_cColor;
        }
    }
}
