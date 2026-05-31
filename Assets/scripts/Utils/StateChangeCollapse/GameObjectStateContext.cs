using System.Collections.Generic;
using UnityEngine;

namespace StateChangeCollapse
{
    //Accumulates object activation changes to execute the final result at a specific point saving on costly temporal toggles
    //When not enabled, just let activation changes execute immediately
    public class GameObjectStateContext
    {
        private bool m_bEnabled;
        private readonly Dictionary<GameObject, bool> m_bActivationValues = new();

        public void start()
        {
            Debug.Assert(!m_bEnabled, "ObjectActivationContext.start() already called. Missing ObjectActivationContext.end()?");
            m_bEnabled = true;
        }

        public void end()
        {
            Debug.Assert(m_bEnabled, "ObjectActivationContext.end() called without a previous ObjectActivationContext.start()");
            m_bEnabled = false;
            foreach ((GameObject oObject, bool bActivationValue) in m_bActivationValues)
            {
                if (!oObject.isNullOrDestroyed())
                {
                    oObject.SetActive(bActivationValue);
                }
            }
            m_bActivationValues.Clear();
        }

        public void setActive(GameObject _oObject, bool _bValue)
        {
            if (m_bEnabled)
            {
                m_bActivationValues[_oObject] = _bValue;
            }
            else
            {
                _oObject.SetActive(_bValue);
            }
        }

        public bool IsActive(GameObject _oObject)
        {
            bool bIsActive;
            if (!m_bEnabled || !m_bActivationValues.TryGetValue(_oObject, out bIsActive))
            {
                bIsActive = _oObject.activeSelf;
            }
            return bIsActive;
        }
    }
}
