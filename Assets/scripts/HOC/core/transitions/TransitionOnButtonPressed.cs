using Sirenix.OdinInspector;

public class TransitionOnButtonPressed : StateTransition
{
    [HorizontalGroup("first", Width = 150, MarginRight = 5), LabelText("button"), LabelWidth(38)]
    public string button;
    [HorizontalGroup("2", Width = 50, MarginRight = 5), LabelText("anyPlayer"), LabelWidth(60)]
    public bool m_anyPlayer = true;
    [HorizontalGroup("2", Width = 50, MarginRight = 5), LabelText("anyController"), LabelWidth(80)]
    public bool m_anyController = false;
    [HorizontalGroup("2", Width = 75, MarginRight = 5), LabelText("player"), LabelWidth(39)]
    public int player = 0;

    public override bool update()
    {
        if (m_anyController)
        {
            return RewiredManager.Instance.GetActionDownAnyController(button);
        }
        else if (m_anyPlayer || player == -1)
        {
            return RewiredManager.Instance.GetActionDownAnyPlayer(button);
        }
        else
        {
            return RewiredManager.Instance.GetActionDown(player, button);
        }
    }
}
