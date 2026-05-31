using UnityEngine;

public class HOCEventsList : MonoBehaviour
{
    [SerializeField] GameObject[] m_aoEventList;

    public GameObject[] EventList { get { return m_aoEventList; } }
}
