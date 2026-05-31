using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StateChangeCollapse
{
    //Accumulates slider changes to execute the final result at a specific point saving on costly temporal toggles
    //When not enabled, just let slider changes execute immediately
    public class SliderStateContext
    {
        private bool m_bEnabled;
        private readonly Dictionary<Slider, float> m_dfSliderValue = new();

        public void start()
        {
            Debug.Assert(!m_bEnabled, "SliderStateContext.start() already called. Missing SliderStateContext.end()?");
            m_bEnabled = true;
        }

        public void end()
        {
            Debug.Assert(m_bEnabled, "SliderStateContext.end() called without a previous SliderStateContext.start()");
            m_bEnabled = false;
            foreach ((Slider oSlider, float fValue) in m_dfSliderValue)
            {
                if (!oSlider.isNullOrDestroyed())
                {
                    oSlider.value = fValue;
                }
            }
            m_dfSliderValue.Clear();
        }

        public void setValueDelayed(Slider _oSlider, float _fValue)
        {
            if (m_bEnabled)
            {
                m_dfSliderValue[_oSlider] = _fValue;
            }
            else
            {
                _oSlider.value = _fValue;
            }
        }
    }
}
