using UnityEngine;

public class ActivateGameObjectsVersioned : MonoBehaviour
{
    enum State { Active, Inactive }
    [SerializeField] GameObject[] m_aoGameObjects;

    [SerializeField] State m_eStateStand;
    [SerializeField] State m_eStateDemo;
    [SerializeField] State m_eStateMain;

    private void Start()
    {
#if VERSION_EXHIBITION
        foreach(GameObject oGO in m_aoGameObjects)
        {
            oGO.SetActive(m_eStateStand == State.Active);
        }
#elif VERSION_DEMO
        foreach (GameObject oGO in m_aoGameObjects)
        {
            oGO.SetActive(m_eStateDemo == State.Active);
        }
#else
        foreach (GameObject oGO in m_aoGameObjects)
        {
            oGO.SetActive(m_eStateMain == State.Active);
        }
#endif
    }
}
