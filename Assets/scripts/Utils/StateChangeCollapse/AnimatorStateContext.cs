using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateChangeCollapse
{
    //Accumulates object activation changes to execute the final result at a specific point saving on costly temporal toggles
    //When not enabled, just let activation changes execute immediately
    public class AnimatorStateContext
    {
        [Serializable]
        public class AnimatorState
        {
            public string m_sAnimation;
            public bool m_bDontPlayIfAlreadyPlaying = false;
            public int m_iLayer = 0;
            public float m_fTime = 0.0f;

            public AnimatorState(string _sAnimation, bool _bDontPlayIfAlreadyPlaying = false, int _iLayer = 0, float _fTime = 0.0f)
            {
                m_sAnimation = _sAnimation;
                m_bDontPlayIfAlreadyPlaying = _bDontPlayIfAlreadyPlaying;
                m_iLayer = _iLayer;
                m_fTime = _fTime;
            }
            public void playOn(Animator _oAnimator)
            {
                _oAnimator.play(m_sAnimation, m_bDontPlayIfAlreadyPlaying, m_iLayer, m_fTime);
            }
        }
        private bool m_bEnabled;
        private readonly Dictionary<Animator, AnimatorState> m_sAnimationStates = new();

        public void start()
        {
            Debug.Assert(!m_bEnabled, "ObjectActivationContext.start() already called. Missing ObjectActivationContext.end()?");
            m_bEnabled = true;
        }

        public void end()
        {
            Debug.Assert(m_bEnabled, "ObjectActivationContext.end() called without a previous ObjectActivationContext.start()");
            m_bEnabled = false;
            foreach ((Animator oAnimator, AnimatorState oAnimationState) in m_sAnimationStates)
            {
                if (!oAnimator.isNullOrDestroyed())
                {
                    oAnimationState.playOn(oAnimator);
                }
            }
            m_sAnimationStates.Clear();
        }

        public void setAnimatorDelayed(Animator _oAnimator, string _sAnimation, bool _bDontPlayIfAlreadyPlaying = false, int _iLayer = 0, float _fTime = 0.0f)
        {
            if (m_bEnabled)
            {
                m_sAnimationStates[_oAnimator] = new(_sAnimation, _bDontPlayIfAlreadyPlaying, _iLayer, _fTime);
            }
            else
            {
                _oAnimator.play(_sAnimation, _bDontPlayIfAlreadyPlaying, _iLayer, _fTime);
            }
        }

        public bool isAnimationQueued(Animator _oAnimator, string _sAnimation)
        {
            if (m_sAnimationStates.ContainsKey(_oAnimator))
            {
                return _sAnimation == m_sAnimationStates[_oAnimator].m_sAnimation;
            }
            return false;
        }
    }
}