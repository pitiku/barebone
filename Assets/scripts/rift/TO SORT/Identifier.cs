#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Sirenix.OdinInspector;

public class Identifier : MonoBehaviour
{
    [ReadOnly] public string m_sID;

    public string ID
    {
        get { return m_sID; }
        set { m_sID = value; }
    }

    public string InstanceID
    {
        get { return ID; }
        set { }
    }

#if UNITY_EDITOR
    [Button("Set ID")]
    private void setID()
    {
        string sAssetPath = AssetDatabase.GetAssetPath(gameObject);
        string sGuid = AssetDatabase.AssetPathToGUID(sAssetPath);
        m_sID = sGuid;
    }
#endif
}
