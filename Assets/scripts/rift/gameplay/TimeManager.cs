using UnityEngine;

public class TimeManager : SceneSingleton<TimeManager>
{
    AnimationCurve m_oCurve;
    float m_fDuration;
    float m_fTime;

    protected override void Awake()
    {
        base.Awake();

        m_oCurve = null;
        m_fTime = -1;
        m_fDuration = 0;
    }

    public void setTime(AnimationCurve _oCurve)
    {
        m_oCurve = _oCurve;
        m_fDuration = _oCurve.keys[^1].time;
        m_fTime = 0;

        Time.timeScale = evaluate(0);
    }

    public void setTime(float _fValue)
    {
        m_oCurve = null;
        m_fDuration = 0;
        m_fTime = -1;

        Time.timeScale = _fValue;
    }

    private void Update()
    {
        if (isRunning())
        {
            if (isFinished())
            {
                Time.timeScale = evaluate(m_fDuration);
                Debug.Log($"[TimeManager] Time scale set to {Time.timeScale}");

                m_fTime = -1;
                m_fDuration = 0;
                m_oCurve = null;
            }
            else
            {
                m_fTime += Time.unscaledDeltaTime;
                Time.timeScale = evaluate();
                Debug.Log($"[TimeManager] Time scale set to {Time.timeScale}");
            }
        }
    }

    private float evaluate()
    {
        float fProgress = m_fTime / m_fDuration;
        float fValue = Mathf.Clamp(m_oCurve.Evaluate(fProgress), 0, 10);

        return fValue;
    }

    private float evaluate(float _fValue)
    {
        float fValue = Mathf.Clamp(m_oCurve.Evaluate(_fValue), 0, 10);

        return fValue;
    }

    private bool isFinished()
    {
        return m_fTime > m_fDuration;
    }

    private bool isRunning()
    {
        return m_fTime != -1;// && !PauseManager.Instance.isGamePaused();
    }
}
