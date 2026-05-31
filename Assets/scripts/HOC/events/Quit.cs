using UnityEngine;

public class Quit : TimedEvent
{
    public override void play()
    {
        base.play();

        Application.Quit();
    }
}
