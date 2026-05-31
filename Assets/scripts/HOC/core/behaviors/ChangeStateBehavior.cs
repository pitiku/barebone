
public class ChangeStateBehavior : Behavior
{
    public State m_oNextState;

    public override void update()
    {
        base.update();

        m_oNextState.changeToMe();
    }

    public override void deactivate() { }
}
