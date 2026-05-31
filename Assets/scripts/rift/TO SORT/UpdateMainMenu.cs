using UnityEngine;

public class UpdateMainMenu : Behavior
{
    [SerializeField] State m_oSelectSlotState;

    public override void activate()
    {
        base.activate();
    }

    public override void update()
    {
        base.update();

        MainMenuManager.Instance.updateMenu();
    }

    public override void deactivate()
    {
        base.deactivate();
    }
}
