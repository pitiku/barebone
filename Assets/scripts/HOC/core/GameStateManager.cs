using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : StateMachineSingleton<GameStateManager>
{
    public enum eGameState
    {
        Playing,
        Paused
    }

    eGameState m_eGameState = eGameState.Playing;

    [NonSerialized] public Dictionary<int, float> m_aoTimers = new Dictionary<int, float>();
    private List<DelayedEvent> m_aoDelayedEvents = new List<DelayedEvent>(20);

    public const int APPLICATION_TIMER = 0;
    public const int GAMEPLAY_TIMER = 1;

    public State m_oUpdateState;
    public State m_oPauseState;

    State m_oSceneState;

    public override bool initialize(bool _bForce = false)
    {
        if (base.initialize(_bForce))
        {
            m_aoTimers[APPLICATION_TIMER] = 0.0f;
            m_aoTimers[GAMEPLAY_TIMER] = 0.0f;
            return true;
        }
        return false;
    }

    public override void Awake()
    {
        base.Awake();
        LoadingManager.Instance.subscribeOnLoadedScene(linkSceneStates);
    }

    public override void update()
    {
        base.update();

        // update debug stuff
        Deb.Instance.update();

        // update timers
        m_aoTimers[APPLICATION_TIMER] += Time.deltaTime;
        if (isPlaying)
        {
            m_aoTimers[GAMEPLAY_TIMER] += Time.deltaTime;

            // update delayed timed events
            for (int i = 0; i < m_aoDelayedEvents.Count; ++i)
            {
                m_aoDelayedEvents[i].m_fDelay -= Time.deltaTime;
                if (m_aoDelayedEvents[i].m_fDelay <= 0.0f)
                {
                    if (!m_aoDelayedEvents[i].m_oEvent.isNullOrDestroyed())
                    {
                        m_aoDelayedEvents[i].m_oEvent.play();
                    }

                    m_aoDelayedEvents.RemoveAt(i);
                    --i;
                }
            }
        }
    }

    public void toPause()
    {
        m_eGameState = eGameState.Paused;
        m_oPauseState.changeToMe();
    }

    public void toPlaying()
    {
        m_eGameState = eGameState.Playing;
        m_oUpdateState.changeToMe();
    }

    public void checkPause()
    {
        //if (PauseManager.Instance.canPause() && GameUtils.pauseButton(true))
        //{
        //    toPause();
        //}
    }

    public void addDelayedEvent(TimedEvent _oEvent, float _fDelay)
    {
        DelayedEvent oDelayed = new DelayedEvent();
        oDelayed.m_oEvent = _oEvent;
        oDelayed.m_fDelay = _fDelay;
        m_aoDelayedEvents.Add(oDelayed);
    }

    public void removeDelayedEvent(TimedEvent _oEvent)
    {
        DelayedEvent oEventToRemove = null;
        foreach (DelayedEvent oEvent in m_aoDelayedEvents)
        {
            if (oEvent.m_oEvent == _oEvent)
            {
                oEventToRemove = oEvent;
                break;
            }
        }
        if (oEventToRemove != null)
        {
            m_aoDelayedEvents.Remove(oEventToRemove);
        }
    }

    public void updateSceneStates()
    {
        if (!m_oSceneState.isNullOrDestroyed())
        {
            m_oSceneState.update();
        }
    }

    public void linkSceneStates()
    {
        GameObject[] aoContainers = GameObject.FindGameObjectsWithTag("statesContainer");
        if (!aoContainers.isNullOrEmpty())
        {
            m_oSceneState = aoContainers[0].GetComponent<State>();
            m_oSceneState.activate();
        }
    }

    public void unlinkSceneStates()
    {
        m_oSceneState.deactivate();
        m_oSceneState = null;
    }

    #region ON LOADING

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        m_aoTimers[GAMEPLAY_TIMER] = 0.0f;
    }

    #endregion

    #region GAME STATE

    public bool isPaused => m_eGameState == eGameState.Paused;
    public bool isPlaying => m_eGameState == eGameState.Playing;

    #endregion
}
