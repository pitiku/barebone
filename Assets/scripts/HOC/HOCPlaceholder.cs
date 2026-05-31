using UnityEngine;

public enum eHOCComponent
{
    State,
    Behavior,
    Event,
    Transition,
}

public class HOCPlaceholder : MonoBehaviour
{
    public eHOCComponent m_iType;
    public string m_sDescription;
}
