using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public class OnComponentAdded
{
    static OnComponentAdded()
    {
        ObjectFactory.componentWasAdded += SetCanvasGroupDefaults;
    }
    static void SetCanvasGroupDefaults(Component comp)
    {
        if (comp is CanvasGroup oCG)
        {
            oCG.blocksRaycasts = false;
            oCG.interactable = false;
        }
        if (comp is MaskableGraphic oImage)
        {
            oImage.raycastTarget = false;
            oImage.maskable = true;
        }
    }
}