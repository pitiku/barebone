using UnityEngine;
using UnityEngine.UI;

public class SetColorOfAllChilds : TimedEvent
{
    public Transform m_oParent;
    public Color m_cColor;

    public override void play()
    {
        base.play();

        Component[] aoComponents = m_oParent.GetComponentsInChildren(typeof(Component), true);

        foreach (Component oComponent in aoComponents)
        {
            if (oComponent is Graphic) //Graphics includes TMP_Text, Image and more
            {
                ((Graphic)oComponent).color = m_cColor;
            }
            else if (oComponent.GetComponent<Graphic>() != null)
            {
                oComponent.GetComponent<Graphic>().color = m_cColor;
            }
            else if (oComponent is SpriteRenderer)
            {
                ((SpriteRenderer)oComponent).color = m_cColor;
            }
            else if (oComponent.GetComponent<SpriteRenderer>() != null)
            {
                oComponent.GetComponent<SpriteRenderer>().color = m_cColor;
            }
        }
    }
}
