
public class TransitionOnLoadDataFinished : StateTransition
{
    public override bool update()
    {
        return SaveManager.Instance.isLoadDone();
    }
}
