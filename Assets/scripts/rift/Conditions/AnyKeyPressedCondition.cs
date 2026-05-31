using Rewired;

public class AnyKeyPressedCondition : Condition
{
    public override bool isMet(ICondition iC = null)
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

    public override object Clone()
    {
        return new AnyKeyPressedCondition();
    }
}
