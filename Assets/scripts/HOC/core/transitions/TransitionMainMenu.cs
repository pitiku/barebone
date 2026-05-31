using UnityEngine;

public class TransitionMainMenu : TimedEvent
{
    [SerializeField] State m_oMainMenuState;

    public override void play()
    {
        m_oMainMenuState.changeToMe();
    }
}
