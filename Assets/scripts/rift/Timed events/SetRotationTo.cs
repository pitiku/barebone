using UnityEngine;

public class SetRotationTo : TimedEvent
{
    [SerializeReference] TagReference m_oReference;
    [SerializeField] Vector3 m_vRotation;

    public override void play()
    {
        base.play();

        if (m_oReference == null)
            return;

        GameObject target = m_oReference.getGameobject();
        if (target == null)
            return;

        Transform tr = target.transform;
        if (tr == null)
            return;

        tr.rotation = Quaternion.Euler(m_vRotation);
    }
}
