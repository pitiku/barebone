using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UpdateMethod : Behavior
{
    [SerializeField] GenericReference<MonoBehaviour> m_oReference;

    [ValueDropdown("Methods"), SerializeField] string[] m_asMethodName;
    List<MethodInfo> m_aoMethods = new();
    MonoBehaviour m_oRference;

    // return a list of the names of the public methods of the given reference
    private IEnumerable<ValueDropdownItem> Methods()
    {
        List<ValueDropdownItem> aoMethodsName = new List<ValueDropdownItem>();

        MethodInfo[] aoMethods = getMethods();

        for (int i = 0; i < aoMethods.Length; i++)
        {
            MethodInfo itMethod = aoMethods[i];
            aoMethodsName.Add(new ValueDropdownItem(itMethod.Name, itMethod.Name));
        }
        return aoMethodsName;
    }

    public MonoBehaviour getReference() { return m_oReference.getReference(); }

    MethodInfo[] getMethods()
    {
        System.Type oType = m_oReference.getReference().GetType();

        return oType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    }

    public MethodInfo getMethod(string _sName)
    {
        MethodInfo[] aoMethods = getMethods();

        for (int i = 0; i < aoMethods.Length; i++)
        {
            MethodInfo itMethod = aoMethods[i];

            if (itMethod.Name == _sName) { return itMethod; }
        }

        return null;
    }

    public override void initialize()
    {
        base.initialize();
        m_oRference = m_oReference.getReference();
        for (int i = 0; i < m_asMethodName.Length; i++)
        {
            m_aoMethods.Add(getMethod(m_asMethodName[i]));
        }
    }

    public override void update()
    {
        base.update();

        for (int i = 0; i < m_aoMethods.Count; i++)
        {
            m_aoMethods[i].Invoke(m_oRference, null);
        }
    }
}

