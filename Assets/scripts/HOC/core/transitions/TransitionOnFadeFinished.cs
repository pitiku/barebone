public class TransitionOnFadeFinished : StateTransition
{
    public override bool update()
    {
        return FadeManager.Instance.IsFadeFinished();
    }
}
