using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class IsDebugBoolActivatedCondition : Condition
{
    [SerializeField] string m_sName;
    public override bool isMet(ICondition iC = null)
    {
#if UNITY_EDITOR
        return EditorPrefs.GetBool(m_sName);
#else
        return false;
#endif

    }
}
