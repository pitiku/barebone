public class UpdateFadeEnd : State
{
    public override void activate(bool _bActivateChildren = true)
    {
        base.activate(_bActivateChildren);
        FadeManager.Instance.checkNextFade();
    }
}
