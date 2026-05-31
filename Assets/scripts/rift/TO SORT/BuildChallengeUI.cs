using TMPro;
using UnityEngine;

public class BuildChallengeUI : MonoBehaviour
{
    public TextMeshProUGUI m_oText;

    public GameObject m_oDone;
    public GameObject m_oNotDone;

    public void setChallenge(bool _bDone, string _sInfo)
    {
        m_oText.text = _sInfo;
        if (_bDone)
        {
            m_oDone.playHOCEvents();
        }
        else
        {
            m_oNotDone.playHOCEvents();
        }
    }
}