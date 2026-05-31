[System.Serializable]
public class StopAllMusic : TimedEvent
{
    public override void play()
    {
        base.play();

        AudioManager.Instance.stopAllMusic();
    }
}