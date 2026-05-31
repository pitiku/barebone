using Sirenix.OdinInspector;
using UnityEngine;

public class MoveTransformAnimated : Behavior
{
    #region Reference
    public enum eState { Tag, Reference }
    public enum eFinalPosition { Target, InitialPosition }

    [SerializeField, HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText("Tag"), ShowIf("@m_eType == eState.Tag")] CustomTag m_oTag;

    [SerializeField] eState m_eType = eState.Reference;

    [HorizontalGroup("first", Width = 150), LabelWidth(1), LabelText(""), ShowIf("@m_eType == eState.Reference")]
    public Transform m_oObject;
    #endregion Reference

    enum eLockAxis { None, X, Y, Z, XY, XZ, YZ }
    enum ePointType { SinglePoint, MultiplePoint }

    [SerializeField] ePointType m_ePointType = ePointType.SinglePoint;
    [HorizontalGroup("2", Width = 175), LabelWidth(15), LabelText("to"), ShowIf("@m_ePointType == ePointType.SinglePoint")]
    public Transform m_oTarget;
    [HorizontalGroup("2", Width = 175), LabelWidth(15), LabelText("to"), ShowIf("@m_ePointType == ePointType.MultiplePoint")]
    public Transform[] m_aoTarget;

    [HorizontalGroup("first", Width = 75), LabelWidth(28), LabelText("secs")]
    public float m_fDuration;

    [SerializeField] AnimationCurve m_oCurve;
    [SerializeField] eLockAxis m_eLockAxis = eLockAxis.None;
    [SerializeField] eFinalPosition m_eFinalPosition = eFinalPosition.Target;

    Timer m_oTimer;
    Vector3 m_vInitialPosition;
    Vector3 m_vFinalPosition;

    float m_fTotalDistance;
    float m_fTimeToReachNextPoint;
    float m_fStartTimePoint;
    int m_iIndex;

    public override void activate()
    {
        base.activate();

        getReference();
        initMovement();
    }

    void getReference()
    {
        if (m_eType == eState.Tag)
        {
            m_oObject = GameplayManager.Instance.getGameobject(m_oTag.Value).transform;
        }
    }

    void getFinalPosition()
    {
        if (m_ePointType == ePointType.SinglePoint)
        {
            m_vFinalPosition = m_oTarget.position;
        }
        else
        {
            m_vFinalPosition = m_aoTarget[m_iIndex].position;
        }

        Vector3 vPosition = m_vFinalPosition;

        if (m_eFinalPosition == eFinalPosition.Target)
        {
            if (m_ePointType == ePointType.SinglePoint)
            {
                vPosition = m_oTarget.position;
            }
            else
            {
                vPosition = m_aoTarget[m_iIndex].position;
            }
        }
        else { vPosition = m_oObject.position; }

        if (m_eLockAxis == eLockAxis.X || m_eLockAxis == eLockAxis.XY || m_eLockAxis == eLockAxis.XZ)
        {
            vPosition.x = m_vInitialPosition.x;
        }

        if (m_eLockAxis == eLockAxis.Y || m_eLockAxis == eLockAxis.XY || m_eLockAxis == eLockAxis.YZ)
        {
            vPosition.y = m_vInitialPosition.y;
        }

        if (m_eLockAxis == eLockAxis.Z || m_eLockAxis == eLockAxis.YZ || m_eLockAxis == eLockAxis.XZ)
        {
            vPosition.z = m_vInitialPosition.z;
        }

        m_vFinalPosition = vPosition;
    }

    void initMovement()
    {
        m_iIndex = -1;

        if (m_ePointType == ePointType.MultiplePoint)
        {
            float fTotalDistance = Vector2.Distance(m_aoTarget[0].posXZ(), m_oObject.posXZ());

            for (int i = 0; i < m_aoTarget.Length - 1; i++)
            {
                fTotalDistance += Vector2.Distance(m_aoTarget[i].posXZ(), m_aoTarget[i + 1].posXZ());
            }

            m_fTotalDistance = fTotalDistance;

            setNextIndex();
        }

        m_oTimer = new Timer(m_fDuration, eType.StopWhenPaused);
        m_oTimer.start();

        if (m_eFinalPosition == eFinalPosition.Target) { m_vInitialPosition = m_oObject.position; }
        else { m_vInitialPosition = m_oTarget.position; }

        getFinalPosition();

        m_oObject.position = m_vInitialPosition;
    }

    private void setNextIndex()
    {
        m_iIndex++;

        if (m_eFinalPosition == eFinalPosition.Target)
        {
            if (m_iIndex == 0)
            {
                if (m_eFinalPosition == eFinalPosition.Target) { m_vInitialPosition = m_oObject.position; }
                else { m_vInitialPosition = m_oTarget.position; }
            }
            else
            {
                m_vInitialPosition = m_aoTarget[m_iIndex - 1].position;
            }

            getFinalPosition();
        }

        float fCurrentDistanceToCover;

        if (m_iIndex == 0)
        {
            fCurrentDistanceToCover = Vector2.Distance(m_aoTarget[0].posXZ(), new Vector2(m_vInitialPosition.x, m_vInitialPosition.z));
        }
        else
        {
            fCurrentDistanceToCover = Vector2.Distance(m_aoTarget[m_iIndex].posXZ(), m_aoTarget[m_iIndex - 1].posXZ());
        }

        float fPercentage = fCurrentDistanceToCover / m_fTotalDistance;

        m_fStartTimePoint = m_fTimeToReachNextPoint;
        m_fTimeToReachNextPoint += fPercentage * m_fDuration;
    }

    public override void update()
    {
        base.update();

        if (m_oTimer == null || m_oTimer.isFinished())
        {
            return;
        }

        if (m_ePointType == ePointType.SinglePoint)
        {
            m_oObject.position = Vector3.Lerp(m_vInitialPosition, m_vFinalPosition, m_oCurve.Evaluate(m_oTimer.getProgress()));
        }
        else if (m_ePointType == ePointType.MultiplePoint)
        {
            if (m_oTimer.getElapsedTime() >= m_fTimeToReachNextPoint)
            {
                m_oObject.position = m_vFinalPosition;
                if (m_iIndex < m_aoTarget.Length - 1)
                {
                    setNextIndex();
                }
            }
            else
            {
                float fPercentage = (m_oTimer.getElapsedTime() - m_fStartTimePoint) / (m_fTimeToReachNextPoint - m_fStartTimePoint);

                m_oObject.position = Vector3.Lerp(m_vInitialPosition, m_vFinalPosition, m_oCurve.Evaluate(fPercentage));
            }
        }

        if (m_oTimer.isFinished())
        {
            m_oObject.position = m_vFinalPosition;
            m_bFinished = true;
        }
    }

    [Button("calculate time by velocity")]
    private float calculateTimeByVelocity(float _fSpeed)
    {
        float fDistance = 0;

        if (m_ePointType == ePointType.SinglePoint)
        {
            fDistance = Vector2.Distance(m_oTarget.posXZ(), m_oObject.posXZ()) / _fSpeed;
        }
        else
        {
            fDistance += Vector2.Distance(m_aoTarget[0].posXZ(), m_oObject.posXZ());

            for (int i = 0; i < m_aoTarget.Length - 1; i++)
            {
                fDistance += Vector2.Distance(m_aoTarget[i].posXZ(), m_aoTarget[i + 1].posXZ());
            }
        }

        return fDistance / _fSpeed;
    }

    [Button("calculate time by velocity")]
    private float calculateTimeByEndOfStep(int _iIndex)
    {
        int iStepIndex = Mathf.Clamp(_iIndex, 0, m_aoTarget.Length);

        float fTotalDistance = 0;

        for (int i = 0; i < m_aoTarget.Length - 1; i++)
        {
            fTotalDistance += Mathf.Abs((m_aoTarget[i].posXZ() - m_aoTarget[i + 1].posXZ()).magnitude);
        }

        float fDistance = 0;

        fDistance = Vector2.Distance(m_aoTarget[0].posXZ(), m_oObject.posXZ());

        for (int i = 0; i < iStepIndex; i++)
        {
            fDistance += Vector2.Distance(m_aoTarget[i].posXZ(), m_aoTarget[i + 1].posXZ());
        }

        return fDistance / fTotalDistance * m_fDuration;
    }
}
