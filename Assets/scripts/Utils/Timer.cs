using UnityEngine;

public enum eType { NeverStop, StopWhenPaused, DontTrack }

public class Timer
{
    float m_fStartTime;
    float m_fDuration;
    bool m_bActive;
    eType m_eType;

    //float m_fTime = 0;

    bool m_bCacheActiveStatus;
    // when pausing all timers due to pausing the game, some timers might be running and others dont
    // need to save this information in order to only resume those who were previously active

    public Timer(eType _eType)
    {
        m_fStartTime = float.MaxValue;
        m_fDuration = 0;
        m_bActive = false;
        m_eType = _eType;

        if (m_eType != eType.DontTrack) { TimerManager.Instance.addTimer(this); }
    }

    public Timer(float _fDuration, eType _eType)
    {
        m_fStartTime = float.MaxValue;
        m_fDuration = _fDuration;
        m_bActive = false;
        m_eType = _eType;

        if (m_eType != eType.DontTrack) { TimerManager.Instance.addTimer(this); }
    }

    ~Timer()
    {
        if (TimerManager.isNull()) { return; }

        TimerManager.Instance.removeTimer(this);
    }

    public void start(float _fInitialOffset = 0.0f)
    {
        float fTimeSinceStartUp = m_eType == eType.StopWhenPaused ? TimerManager.Instance.UnpauseTimeSinceStartup : Time.realtimeSinceStartup;

        m_fStartTime = fTimeSinceStartUp + _fInitialOffset;
        m_bActive = true;
    }

    public void start(float _fDuration, float _fInitialOffset)
    {
        //m_fTime = 0;
        m_fDuration = _fDuration;
        start(_fInitialOffset);
    }

    public float getProgress()
    {
        return Mathf.Clamp01(getElapsedTime() / m_fDuration);
    }

    public float getElapsedTime()
    {
        float fTimeSinceStartUp = m_eType == eType.StopWhenPaused ? TimerManager.Instance.UnpauseTimeSinceStartup : Time.realtimeSinceStartup;

        return fTimeSinceStartUp - m_fStartTime;
    }

    public float getDuration()
    {
        return m_fDuration;
    }

    public bool hasPassedTime(float _fTime)
    {
        return getElapsedTime() > _fTime;
    }

    public bool isFinished()
    {
        return getElapsedTime() >= m_fDuration;
    }

    public void deactivate()
    {
        m_bActive = false;
    }

    public void activate() { m_bActive = true; }
    public void pause()
    {
        m_bCacheActiveStatus = m_bActive;
        m_bActive = false;
    }

    public void unpause() { m_bActive = m_bCacheActiveStatus; }

    public bool isActive()
    {
        return m_bActive;
    }

    public eType Type { get { return m_eType; } }
}
