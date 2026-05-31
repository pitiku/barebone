using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSetFloat : TimedEvent
{
    [HorizontalGroup("first", Width = 180, MarginRight = 8, LabelWidth = 1)]
    [LabelText("")]
    public Animator m_oAnimator;
    [HorizontalGroup("first", Width = 100, MarginRight = 0, LabelWidth = 1)]
    [LabelText("")]
    [ValueDropdown("getParams", DropdownWidth = 200)]
    public string m_sParam;
    [HorizontalGroup("first", Width = 60, MarginRight = 8, LabelWidth = 15)]
    [LabelText("->")]
    public float m_fValue;

    public override void play()
    {
        base.play();

        m_oAnimator.SetFloat(m_sParam, m_fValue);
    }


#if UNITY_EDITOR
    IEnumerable getParams()
    {
        List<string> aoParams = new List<string>();

        foreach (AnimatorControllerParameter oParam in m_oAnimator.parameters)
        {
            if (oParam.type == AnimatorControllerParameterType.Float)
            {
                aoParams.Add(oParam.name);
            }
        }

        return aoParams;
    }
#endif
}
