using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    public string m_sTag;

    void Awake()
    {
        if (m_sTag.isNullOrEmpty()) { return; }

        GameObject[] gos = GameObject.FindGameObjectsWithTag(m_sTag);
        if (gos.Length > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
