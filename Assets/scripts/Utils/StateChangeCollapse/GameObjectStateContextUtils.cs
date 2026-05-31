using TMPro;
using UnityEngine;

namespace StateChangeCollapse
{
    //Helper class to reduce code bloat
    public static class GameObjectStateContextUtils
    {
        public static void setActiveDelayed(this GameObject _oObject, bool _bValue)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oGameObjectStateContext.setActive(_oObject, _bValue);
            }
            else
            {
                _oObject.SetActive(_bValue);
            }
        }

        public static bool isActiveSelfDelayed(this GameObject _oObject)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                return oStateChangeCollapseContext.m_oGameObjectStateContext.IsActive(_oObject);
            }
            else
            {
                return _oObject.activeSelf;
            }
        }

        public static void setTextDelayed(this TMP_Text _oTMP_Text, string _sFormat, params object[] _aoParams)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oTextStateContext.setTextDelayed(_oTMP_Text, _sFormat, _aoParams);
            }
            else
            {
                TextStateContext.setText(_oTMP_Text, _sFormat, _aoParams);
            }
        }

        public static void setColorDelayed(this TMP_Text _oTMP_Text, Color _oColor)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oTextStateContext.setColorDelayed(_oTMP_Text, _oColor);
            }
            else
            {
                TextStateContext.setColor(_oTMP_Text, _oColor);
            }
        }

        public static void setValueDelayed(this UnityEngine.UI.Slider _oSlider, float _fValue)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oSliderStateContext.setValueDelayed(_oSlider, _fValue);
            }
            else
            {
                _oSlider.value = _fValue;
            }
        }

        public static void setLayerDelayed(this GameObject _oObject, int _iLayer)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oLayerStateContext.setLayerDelayed(_oObject, _iLayer);
            }
            else
            {
                _oObject.setLayer(_iLayer);
            }
        }

        public static void setAnimationDelayed(this Animator _oAnimator, string _sAnimation, bool _bDontPlayIfAlreadyPlaying = false, int _iLayer = 0, float _fTime = 0.0f)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                oStateChangeCollapseContext.m_oAnimatorStateContext.setAnimatorDelayed(_oAnimator, _sAnimation, _bDontPlayIfAlreadyPlaying, _iLayer, _fTime);
            }
            else
            {
                _oAnimator.play(_sAnimation, _bDontPlayIfAlreadyPlaying, _iLayer, _fTime);
            }
        }

        public static bool isThisAnimationPlayingOrQueuedAndFinished(this Animator _oAnimator, string _sAnimation, bool _bCheckAnimationEnded = false)
        {
            if (StateChangeCollapseContext.Instance is StateChangeCollapseContext oStateChangeCollapseContext)
            {
                if (oStateChangeCollapseContext.m_oAnimatorStateContext.isAnimationQueued(_oAnimator, _sAnimation))
                {
                    return true;
                }
            }
            return Utils.isThisAnimationPlayingOrQueuedAndFinished(_oAnimator, _sAnimation, _bCheckAnimationEnded);
        }
    }
}
