using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Reflection;

public class AudioManagerTimedEvent : TimedEvent
{
    [ValueDropdown("@getMethods()", DropdownWidth = 200)]
    [HorizontalGroup("first", Width = 130, MarginRight = 8), LabelText("method"), LabelWidth(42)] public string m_sOption;
    [HorizontalGroup("first", Width = 80, MarginRight = 8), LabelText("duration"), LabelWidth(46)] public float m_fDuration;

    public override void play()
    {
        base.play();

        var oMethod = AudioManager.Instance.GetType().GetMethod(m_sOption);
        oMethod.Invoke(AudioManager.Instance, new object[] { m_fDuration });
    }

    IEnumerable<string> getMethods()
    {
        List<string> aoMethodNames = new List<string>();

        MethodInfo[] aoMethods = AudioManager.Instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (MethodInfo oMethod in aoMethods)
        {
            if (oMethod.GetParameters().Length == 1 && oMethod.GetParameters()[0].ParameterType == typeof(float))
            {
                aoMethodNames.Add(oMethod.Name);
            }
        }

        return aoMethodNames.ToArray();
    }
}
