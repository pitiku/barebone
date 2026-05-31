using Sirenix.OdinInspector;
using UnityEngine;

public class SetGameLocation : TimedEvent
{
    [SerializeField] bool m_bSetToLastGameLocation;
    [SerializeField, ShowIf("@!m_bSetToLastGameLocation")] eGameLocation m_eLocation;
    public override void play()
    {
        base.play();

        if (m_bSetToLastGameLocation)
        {
            GameplayManager.Instance.setToLastGameLocation();
        }
        else
        {
            GameplayManager.Instance.setGameLocation(m_eLocation);
        }
    }
}
