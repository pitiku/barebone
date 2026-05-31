using Rewired;

public class TransitionOnAnyButtonPressed : StateTransition
{
    public override bool update()
    {
        for (int i = 0; i < ReInput.players.playerCount; ++i)
        {
            if (ReInput.players.GetPlayer(i).GetAnyButtonDown())
            {
                return true;
            }
        }

        return false;
    }
}
