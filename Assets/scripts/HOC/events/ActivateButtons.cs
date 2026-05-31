using UnityEngine.UI;

[System.Serializable]
public class ActivateButtons : TimedEvent
{
    public bool m_interactable = true;
    public bool m_enabled = true;
    public Button[] m_buttons;

    public override void play()
    {
        base.play();

        for (int index = 0; index < m_buttons.Length; ++index)
        {
            if (m_buttons[index] != null)
            {
                m_buttons[index].interactable = m_interactable;
                m_buttons[index].enabled = m_enabled;
            }
        }
    }
}
