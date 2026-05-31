
public class UpdateInputs : Behavior
{
    public override void update()
    {
        base.update();

        RewiredManager.Instance.UpdateInputs();
    }
}
