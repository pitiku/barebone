public class LockPlayerInput : TimedEvent
{
    public int frames;
    public int player = -1;

    public override void play()
    {
        base.play();

        RewiredManager.Instance.LockPlayerInputs(frames, player);
    }
}
