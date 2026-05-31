using UnityEngine;

public class ShowCameraBounds : MonoBehaviour
{
    Camera m_oCamera;

    void OnDrawGizmos()
    {
        if (!enabled)
        {
            return;
        }

        if (m_oCamera == null)
        {
            m_oCamera = GetComponent<Camera>();
            if (m_oCamera == null)
            {
                return;
            }
        }

        float verticalHeightSeen = m_oCamera.orthographicSize * 2.0f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(verticalHeightSeen * m_oCamera.aspect, verticalHeightSeen, 0));
    }

    void Start() { }
}
