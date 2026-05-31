using Sirenix.OdinInspector;
using UnityEngine;

public class MoveCameraTo : TimedEvent
{
    public enum eCamera { Combat, Management }
    public enum ePosition { Transform, Position }

    [SerializeField] eCamera m_eCameraType;
    [SerializeField] ePosition m_ePositionType;

    [SerializeField, ShowIf("@m_ePositionType == ePosition.Transform")] Transform m_oTransform;
    [SerializeField, ShowIf("@m_ePositionType == ePosition.Position")] Vector3 m_vPosition;

    public override void play()
    {
        base.play();

        Transform oCameraTransform;

        if (m_eCameraType == eCamera.Combat)
        {
            oCameraTransform = GameplayManager.Instance.m_oCombatCamera.transform;
        }
        else
        {
            oCameraTransform = GameplayManager.Instance.m_oManagementCamera.transform;
        }

        Vector3 vDestinyPosition;

        if (m_ePositionType == ePosition.Position)
        {
            vDestinyPosition = m_vPosition;
        }
        else
        {
            vDestinyPosition = m_oTransform.position;
        }

        oCameraTransform.position = vDestinyPosition;
    }
}
