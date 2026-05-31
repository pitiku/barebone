public class SkipNextFadeTimedEvent : TimedEvent
{
    public override void play()
    {
        base.play();

        FadeManager.Instance.skipNextFade();
    }
}
