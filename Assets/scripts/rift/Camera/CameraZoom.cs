using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    float m_fEventStartTime;
    float m_fZoomDuration;
    float m_fZoomAmunt;
    AnimationCurve m_oCurrentCurve;
    Camera m_oCamera;
    float m_fInitialSize;
    float m_fMoveMultiplier;
    Vector3 m_vTargetPos;
    Vector3 m_vInitialPos;
    Vector3 m_vInitialCameraPos;

    public void setZoom(float _fDuration, float _fZoomAmount, AnimationCurve _oCurve, Vector2 _vAttackPos, float _fMoveAmount)
    {
        m_fEventStartTime = Time.timeSinceLevelLoad;
        m_oCamera = GetComponent<Camera>();
        m_fInitialSize = m_oCamera.orthographicSize;
        m_fZoomDuration = _fDuration;
        m_fZoomAmunt = _fZoomAmount;
        m_oCurrentCurve = _oCurve;
        m_vInitialPos = m_oCamera.WorldToScreenPoint(Vector3.zero);
        m_vTargetPos = m_oCamera.WorldToScreenPoint(_vAttackPos.toVector3xz());
        m_fMoveMultiplier = _fMoveAmount;
        m_vInitialCameraPos = m_oCamera.transform.position;
    }

    void Update()
    {
        float fTime = Time.timeSinceLevelLoad - m_fEventStartTime;
        float fPerc = fTime / m_fZoomDuration;
        float fValue = m_oCurrentCurve.Evaluate(fPerc);

        m_oCamera.orthographicSize = m_fInitialSize - (fValue * m_fZoomAmunt);
        m_oCamera.transform.position = m_vInitialCameraPos + Vector3.Lerp(Vector3.zero, m_vTargetPos - m_vInitialPos, fValue * m_fMoveMultiplier);
        if (fPerc >= 1)
        {
            m_oCamera.orthographicSize = m_fInitialSize;
            m_oCamera.transform.position = m_vInitialCameraPos;
            Destroy(this);
        }
    }
}
