using UnityEngine;
using UnityEngine.UI;

public class FadeManager : StateMachineSingleton<FadeManager>
{
    [SerializeField] Image m_oFadeImage_Overlay;
    [SerializeField] Image m_oFadeImage_BehindUI;

    Image m_oCurrentImage;
    Animator m_oCurrentAnimator;

    [SerializeField] string m_sBlackAnim;
    [SerializeField] string m_sClearAnim;
    [SerializeField] string m_toBlackTrigger;
    [SerializeField] string m_fromBlackTrigger;
    [SerializeField] State m_oFadingToState;
    [SerializeField] State m_oFadingFromState;
    [SerializeField] State m_oFadingEnd;

    Color m_fadeColor;
    float m_fadeDuration;

    private bool m_bLastFadeToBlack = false;
    bool m_bFadeOutAfter = false;
    float m_fDurationAfter = 0.5f;

    bool m_bIsFading = false;
    float m_fStartedTime;

    bool m_bSkipNextFade = false;

    public override void Awake()
    {
        base.Awake();

        setCurrentOverlay();
    }

    public override void update()
    {
        base.update();

        if (m_bIsFading && (Time.time - m_fStartedTime) >= m_fadeDuration)
        {
            m_bIsFading = false;
            CheckEvents();
            if (m_bFadeOutAfter)
            {
                m_bFadeOutAfter = false;
                Fade(false);
            }
            else
            {
                m_oFadingEnd.changeToMe();
            }
        }
    }

    public void checkNextFade()
    {
        if (m_bFadeOutAfter)
        {
            Fade(false, m_fadeColor, m_fDurationAfter);
            m_bFadeOutAfter = false;
        }
    }

    public void fadeInOut(float _fInDuration, float _fOutDuration, Color _oColor)
    {
        m_bFadeOutAfter = false;

        if (_fInDuration > 0.0f)
        {
            Fade(true, _oColor, _fInDuration);

            if (_fOutDuration > 0.0f)
            {
                m_bFadeOutAfter = true;
                m_fDurationAfter = _fOutDuration;
            }
        }
        else if (_fOutDuration > 0.0f)
        {
            Fade(false, _oColor, _fOutDuration);
        }
    }

    public void Fade(bool fadeToBlack)
    {
        if (m_bIsFading && (Time.time - m_fStartedTime) >= m_fadeDuration)
        {
            CheckEvents();
        }

        m_oCurrentAnimator = m_oCurrentImage.GetComponent<Animator>();

        if (m_bSkipNextFade)
        {
            Deb.log("Fade skipped");
            m_bSkipNextFade = false;
            return;
        }

        m_bIsFading = true;
        m_fStartedTime = Time.time;

        m_bLastFadeToBlack = fadeToBlack;

        m_oCurrentImage.color = m_fadeColor;

        m_oCurrentAnimator.speed = 1.0f / m_fadeDuration;
        m_oCurrentAnimator.SetTrigger(fadeToBlack ? m_toBlackTrigger : m_fromBlackTrigger);
        m_oCurrentAnimator.Update(0.0f);
        m_oCurrentAnimator.enabled = true;

        RewiredManager.Instance.LockInputs(m_fadeDuration * 1.1f);

        if (fadeToBlack) { m_oFadingToState.changeToMe(); }
        else { m_oFadingFromState.changeToMe(); }

#if !UNITY_EDITOR && UNITY_SWITCH
		// Switch CG
		if(!fadeToBlack && UnityEngine.Scripting.GarbageCollector.GCMode == UnityEngine.Scripting.GarbageCollector.Mode.Disabled)
		{
			UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Enabled;
			System.GC.Collect();
			UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Disabled;
		}
#endif
    }

    public void Fade(bool fadeToBlack, Color fadeColor, float fadeDuration)
    {
        if (fadeDuration.approximately(0f))
        {
            fadeColor.a = fadeToBlack ? 1f : 0f;
            m_oCurrentImage.color = fadeColor;
            m_oCurrentImage.enabled = true;

            m_oCurrentAnimator = m_oCurrentImage.GetComponent<Animator>();
            m_oCurrentAnimator.enabled = false;
        }
        else
        {
            SetFadeColor(fadeColor);
            SetFadeDuration(fadeDuration);
            Fade(fadeToBlack);
        }
    }

    public void FadeToBlack(float fadeDuration)
    {
        Fade(true, Color.black, fadeDuration);
    }

    public void FadeFromBlack(float fadeDuration)
    {
        Fade(false, Color.black, fadeDuration);
    }

    void SetFadeColor(Color fadeColor)
    {
        m_fadeColor = fadeColor;
    }

    void SetFadeDuration(float fadeDuration)
    {
        m_fadeDuration = fadeDuration;
    }

    public bool IsFadeFinished()
    {
        return !m_bIsFading;
    }

    public void CheckEvents()
    {
        if (m_bLastFadeToBlack)
        {
            EventManager.Instance.TriggerEvent(EventsType.DisableGameobjects);
            EventManager.Instance.TriggerEvent(EventsType.FadeToBlackFinished);
        }
        else { EventManager.Instance.TriggerEvent(EventsType.FadeFromBlackFinished); }
    }

    public void setCurrentOverlay()
    {
        m_oFadeImage_BehindUI.enabled = false;
        m_oCurrentImage = m_oFadeImage_Overlay;
    }

    public void setCurrentBehindUI()
    {
        m_oFadeImage_Overlay.enabled = false;
        m_oCurrentImage = m_oFadeImage_BehindUI;
    }

    public void setToBlack(bool _bIsBehindUI)
    {
        if (_bIsBehindUI) { setCurrentBehindUI(); }
        else { setCurrentOverlay(); }

        m_oCurrentImage.enabled = true;
        m_oCurrentAnimator = m_oCurrentImage.GetComponent<Animator>();
        m_oCurrentAnimator.play(m_sBlackAnim);

        if (m_oCurrentImage == m_oFadeImage_BehindUI)
        {
            m_oFadeImage_Overlay.enabled = false;
            m_oFadeImage_Overlay.GetComponent<Animator>().play(m_sClearAnim);
        }
        else
        {
            m_oFadeImage_BehindUI.enabled = false;
            m_oFadeImage_BehindUI.GetComponent<Animator>().play(m_sClearAnim);
        }
    }

    public void skipNextFade() { m_bSkipNextFade = true; }
}
