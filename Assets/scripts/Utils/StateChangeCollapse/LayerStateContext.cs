using System.Collections.Generic;
using UnityEngine;

namespace StateChangeCollapse
{
    //Accumulates object activation changes to execute the final result at a specific point saving on costly temporal toggles
    //When not enabled, just let activation changes execute immediately
    public class LayerStateContext
    {
        private bool m_bEnabled;
        private readonly Dictionary<GameObject, int> m_iLayerValues = new();

        public void start()
        {
            Debug.Assert(!m_bEnabled, "ObjectActivationContext.start() already called. Missing ObjectActivationContext.end()?");
            m_bEnabled = true;
        }

        public void end()
        {
            Debug.Assert(m_bEnabled, "ObjectActivationContext.end() called without a previous ObjectActivationContext.start()");
            m_bEnabled = false;
            foreach ((GameObject _oObject, int iLayer) in m_iLayerValues)
            {
                if (!_oObject.isNullOrDestroyed())
                {
                    _oObject.setLayer(iLayer);
                }
            }
            m_iLayerValues.Clear();
        }

        public void setLayerDelayed(GameObject _oObject, int _iLayer)
        {
            if (m_bEnabled)
            {
                m_iLayerValues[_oObject] = _iLayer;
            }
            else
            {
                _oObject.setLayer(_iLayer);
            }
        }
    }
}
