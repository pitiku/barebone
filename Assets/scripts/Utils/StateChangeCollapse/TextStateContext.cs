using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StateChangeCollapse
{
    //Accumulates text changes to execute the final result at a specific point saving on costly temporal toggles
    //When not enabled, just let text changes execute immediately
    public class TextStateContext
    {
        private bool m_bEnabled;
        private readonly Dictionary<MonoBehaviour, TextState> m_doTextStates = new();
        private readonly Dictionary<TMP_Text, Color> m_doColorStates = new();

        public void start()
        {
            Debug.Assert(!m_bEnabled, "TextStateContext.start() already called. Missing TextStateContext.end()?");
            m_bEnabled = true;
        }

        public void end()
        {
            Debug.Assert(m_bEnabled, "TextStateContext.end() called without a previous TextStateContext.start()");
            m_bEnabled = false;
            foreach ((MonoBehaviour oTextContainer, TextState oTextState) in m_doTextStates)
            {
                if (!oTextContainer.isNullOrDestroyed())
                {
                    setText(oTextContainer, oTextState);
                }
            }
            foreach ((TMP_Text oTextContainer, Color oColor) in m_doColorStates)
            {
                if (!oTextContainer.isNullOrDestroyed())
                {
                    oTextContainer.color = oColor;
                }
            }
            m_doTextStates.Clear();
        }

        public void setColorDelayed(TMP_Text _oTextContainer, Color _oColor)
        {
            if (m_bEnabled)
            {
                m_doColorStates[_oTextContainer] = _oColor;
            }
            else
            {
                setColor(_oTextContainer, _oColor);
            }
        }

        public static void setColor(TMP_Text _oTextContainer, Color _oColor)
        {
            if (_oTextContainer.color != _oColor)
            {
                _oTextContainer.color= _oColor;
            }
        }

        public void setTextDelayed(MonoBehaviour _oTextContainer, string _sFormat, params object[] _aoParams)
        {
            if (m_bEnabled)
            {
                m_doTextStates[_oTextContainer] = new TextState(_sFormat, _aoParams);
            }
            else
            {
                setText(_oTextContainer, _sFormat, _aoParams);
            }
        }

        public static void setText(MonoBehaviour _oTextContainer, string _sFormat, params object[] _aoParams) => setText(_oTextContainer, new TextState(_sFormat, _aoParams));

        private static void setText(MonoBehaviour _oTextContainer, in TextState _oTextState) => setText(_oTextContainer, _oTextState.Value);

        private static void setText(MonoBehaviour _oTextContainer, string _sText)
        {
            if (_oTextContainer is TMP_Text oTMP_Text)
            {
                if (oTMP_Text.text != _sText)
                {
                    oTMP_Text.SetText(_sText);
                }
            }
            else
            {
                Debug.Assert(false, $"Unhandled text container '{_oTextContainer.name}' of type '{_oTextContainer.GetType()}'");
            }
        }

        private readonly struct TextState
        {
            private readonly string m_sFormat;
            private readonly object[] m_aoParams;

            public string Value => m_aoParams == null ? m_sFormat : string.Format(m_sFormat, m_aoParams);

            public TextState(string _sFormat, object[] _aoParams)
            {
                m_sFormat = _sFormat;
                m_aoParams = _aoParams;
            }
        }
    }
}
