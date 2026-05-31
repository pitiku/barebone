using System.Collections.Generic;
using UnityEngine;

public class TimerManager : SceneSingleton<TimerManager>
{
    List<Timer> m_aoTimers = new List<Timer>();

    float m_fUnpauseTimeSinceStartup = 0;
    bool m_bIsGamepaused = false;

    public void Update()
    {
        if (m_bIsGamepaused) { return; }

        m_fUnpauseTimeSinceStartup += Time.deltaTime;
    }

    public void pause()
    {
        m_bIsGamepaused = true;
        pauseTimers(eType.StopWhenPaused);
    }
    public void unpause()
    {
        m_bIsGamepaused = false;
        unpauseTimers(eType.StopWhenPaused);
    }

    public void pauseTimers(eType _eType)
    {
        for (int i = 0; i < m_aoTimers.Count; i++)
        {
            Timer oTimer = m_aoTimers[i];

            if (oTimer.Type == _eType) { oTimer.pause(); }
        }
    }

    public void unpauseTimers(eType _eType)
    {
        for (int i = 0; i < m_aoTimers.Count; i++)
        {
            Timer oTimer = m_aoTimers[i];

            if (oTimer.Type == _eType) { oTimer.unpause(); }
        }
    }


    public void addTimer(Timer _oTimer)
    {
        m_aoTimers.addIfNotContains(_oTimer);
    }
    public void removeTimer(Timer _oTimer)
    {
        m_aoTimers.remove(_oTimer);
    }

    public float UnpauseTimeSinceStartup { get { return m_fUnpauseTimeSinceStartup; } }
}
