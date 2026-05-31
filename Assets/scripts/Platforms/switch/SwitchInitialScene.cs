using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchInitialScene : MonoBehaviour
{
    bool m_bLoad = true;
    float m_fTimeStarted;
    float m_fMinTime = 2.0f;

    private void Start()
    {
        m_fTimeStarted = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (m_bLoad && Time.realtimeSinceStartup - m_fTimeStarted > m_fMinTime)
        {
            OnScreenLog.Add("Now loading scene");
            Debug.Log("Now loading scene");

            m_bLoad = false;
            SceneManager.LoadSceneAsync("logo", LoadSceneMode.Single);
        }
    }
}
