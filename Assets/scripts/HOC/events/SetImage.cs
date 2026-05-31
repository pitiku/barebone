using UnityEngine;
using UnityEngine.UI;

public class SetImage : TimedEvent
{
    [SerializeField] Image m_oImage;
    [SerializeField] Sprite m_oSprite;

    public override void play()
    {
        base.play();
        m_oImage.sprite = m_oSprite;
    }
}
