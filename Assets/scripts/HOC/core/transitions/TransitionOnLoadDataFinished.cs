
public class TransitionOnLoadDataFinished : StateTransition
{
    public override bool update()
    {
        return DataManager.Instance.IsLoadDone();
    }
}
