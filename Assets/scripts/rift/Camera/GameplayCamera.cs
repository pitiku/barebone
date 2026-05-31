using UnityEngine;

#if UNITY_EDITOR
#endif

public class GameplayCamera : SceneSingleton<GameplayCamera>
{
    public Camera m_oCamera;
    public Collider m_oCameraLimits;
    public Rect m_rCameraLimits;

    [Header("Shake")]
    public float m_fShakeAmountMultiplier = 1.0f;
    public float m_fMaxShakeAmount = 30.0f;
    public AnimationCurve m_oShakeOverTime;
    public float m_fShakeTime = 0.3f;

    float m_fShakeTimer;
    [System.NonSerialized] public float m_fCurrentShakeAmount = 0.0f;
    Vector2 m_vShakeOffset;

    [Header("Target")]
    public float m_fFollowTargetSmoothTime = 0.5f;
    public float m_fTargetVerticalOffsetPercentage = 0.2f;
    Vector2 m_vFollowTargetVelocity;
    [HideInInspector] public Transform m_oTarget = null;
    bool m_bFollowTarget = false;

    bool m_bMoving;
    Vector3 m_vTarget;
    float m_fDuration;
    AnimationCurve m_oCurve;
    public Vector3 m_vInitialPosition;
    [HideInInspector] public float m_fInitialSize;
    float m_fStartTime;

    Vector2 m_vLastMousePos;

    void Start()
    {
        //Calculate scene limits for camera
        Ray oRayBottomLeft = m_oCamera.ViewportPointToRay(Vector3.zero);
        Ray oRayTopRight = m_oCamera.ViewportPointToRay(Vector3.one);

        Vector3 vBottomLeft = Vector3.zero;
        Vector3 vTopRight = Vector3.zero;

        RaycastHit oHit;
        if (Physics.Raycast(oRayBottomLeft, out oHit, 200, Layers.ms_maskScene))
        {
            vBottomLeft = oHit.point;
        }

        if (Physics.Raycast(oRayTopRight, out oHit, 200, Layers.ms_maskScene))
        {
            vTopRight = oHit.point;
        }

        if (m_oCameraLimits != null)
        {
            float xMin = m_oCameraLimits.bounds.min.x - vBottomLeft.x;
            float xMax = m_oCameraLimits.bounds.max.x - vTopRight.x;

            float zMin = transform.position.z + m_oCameraLimits.bounds.min.z - vBottomLeft.z;
            float zMax = transform.position.z + m_oCameraLimits.bounds.max.z - vTopRight.z;
            m_rCameraLimits = new Rect(xMin, zMin, xMax - xMin, zMax - zMin);
        }
    }

    public void activate()
    {
        setSize(m_oCamera.orthographicSize);
    }

    public void setSize(float _fSize)
    {
        m_oCamera.orthographicSize = _fSize;
    }

    public void followTarget(bool _bValue, Transform _oTarget = null)
    {
        m_bFollowTarget = _bValue;
        m_oTarget = _oTarget;
        if (m_oTarget != null)
        {
            teleportToTarget();
        }
    }

    public override void update()
    {
        // follow target if needed
        if (m_bFollowTarget)
        {
            Vector2 vPlayerTargetOffseted = m_oTarget.transform.posXY() + (Vector2.up * m_oCamera.orthographicSize * m_fTargetVerticalOffsetPercentage);

            // calculate smoothed position
            Vector2 vSmoothedPosition = Vector2.SmoothDamp(transform.posXY(), vPlayerTargetOffseted, ref m_vFollowTargetVelocity, m_fFollowTargetSmoothTime);
            transform.posXY(vSmoothedPosition);
        }

        //We check the shake option when adding the shake
        m_fShakeTimer += Time.deltaTime;
        if (m_fShakeTimer < m_fShakeTime)
        {
            float fProgress = m_fShakeTimer / m_fShakeTime;
            float fShakeAmount = m_oShakeOverTime.Evaluate(fProgress) * m_fCurrentShakeAmount;
            m_vShakeOffset = RandomUtils.insideUnitCircleUniformly(null) * fShakeAmount;
        }
        else
        {
            m_fCurrentShakeAmount = 0.0f;
            m_vShakeOffset = Vector2.zero;
        }

        m_oCamera.transform.localPosXY(m_vShakeOffset);

        if (m_bMoving)
        {
            float fCurrentTime = Time.realtimeSinceStartup - m_fStartTime;
            float fPerc = Mathf.Clamp01(fCurrentTime / m_fDuration);
            transform.position = Vector3.Lerp(m_vInitialPosition, m_vTarget, m_oCurve.Evaluate(fPerc));
            m_bMoving = fPerc < 1.0f;
        }

        //Zoom
        //m_oCamera.orthographicSize -= Input.mouseScrollDelta.y;
        //m_oCamera.orthographicSize = Mathf.Clamp(m_oCamera.orthographicSize, 10, 50);

        //Check move camera
        if (Input.GetMouseButtonDown(2))
        {
            m_vLastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector2 vMousePos = Input.mousePosition;
            Vector2 vMouseMovement = -(vMousePos - m_vLastMousePos) * 0.25f / m_oCamera.orthographicSize;
            m_vLastMousePos = Input.mousePosition;

            transform.position += (Vector3.right * vMouseMovement.x) + (Vector3.forward * vMouseMovement.y);
        }
        else
        {
            Vector2 vMovement = Vector2.zero;

            if (Input.GetKey(KeyCode.W))
            {
                vMovement.y += 200;
            }

            if (Input.GetKey(KeyCode.A))
            {
                vMovement.x -= 200;
            }

            if (Input.GetKey(KeyCode.S))
            {
                vMovement.y -= 200;
            }

            if (Input.GetKey(KeyCode.D))
            {
                vMovement.x += 200;
            }

            vMovement *= Time.deltaTime / m_oCamera.orthographicSize;

            transform.position += (Vector3.right * vMovement.x) + (Vector3.forward * vMovement.y);
        }

        //Check camera limits
        Vector3 vPosition = transform.position;
        vPosition.x = Mathf.Clamp(vPosition.x, m_rCameraLimits.min.x, m_rCameraLimits.max.x);
        vPosition.z = Mathf.Clamp(vPosition.z, m_rCameraLimits.min.y, m_rCameraLimits.max.y);
        transform.position = vPosition;
    }

    public void teleportToTarget()
    {
        transform.posXY(m_oTarget.transform.posXY());
        m_vFollowTargetVelocity = Vector2.zero;
    }

    public void moveTo(Vector3 _vTarget, float _fDuration, AnimationCurve _oCurve)
    {
        m_oCurve = _oCurve;
        m_fDuration = _fDuration;
        m_vTarget = _vTarget;

        //transform.position = _vTarget;
        m_bMoving = true;
        m_fStartTime = Time.realtimeSinceStartup;
        m_vInitialPosition = transform.position;
    }

    public void shake(float _fShakeAmount)
    {
        // if 0, the shake is annuled
        if (_fShakeAmount.approximately(0.0f))
        {
            m_fCurrentShakeAmount = 0.0f;
            m_fShakeTimer = m_fShakeTime;
            return;
        }

        _fShakeAmount *= m_fShakeAmountMultiplier;
        float fProgress = m_fShakeTimer / m_fShakeTime;
        m_fCurrentShakeAmount = _fShakeAmount + (m_oShakeOverTime.Evaluate(fProgress) * m_fCurrentShakeAmount);
        m_fCurrentShakeAmount = m_fCurrentShakeAmount.clamp(_fShakeAmount, m_fMaxShakeAmount);
        m_fShakeTimer = 0.0f;
    }

    public bool contains(Vector3 _vPoint)
    {
        return _vPoint.isPositionInCamera(m_oCamera);
    }
}
