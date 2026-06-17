using System.Collections.Generic;
using UnityEngine;
using System;


//Always Create Timer via TimerManager.CreateTimer method
//Then if you store a reference to it in your Class, don't forget to dereference it after using it.
//Normally you don't need to reference it, you can simply listen to his events Elapsed, Stop and Start
//The TimerManager only keeps references to Running Timers. 

//Usage example :
//Timer testTimer = TimerManager.CreateTimer(3f);
//testTimer.ElapsedEvent += OnTimerElapsed;
//testTimer.AutoRestart = false;
//testTimer.Start();
public static class TimerManager
{
    private static List<Timer> m_timers = null;
    private static List<Timer> m_timersAddBuffer = null;
    private static List<Timer> m_timersRemoveBuffer = null;

    public static void ClearReferences()
    {
        m_timers?.Clear();
        m_timersAddBuffer?.Clear();
        m_timersRemoveBuffer?.Clear();
    }

    public static Timer CreateTimer()
    {
        Timer timer = new Timer()
        {
            Duration = 0f
        };
        timer.StartEvent = () => OnEventStart(timer);
        timer.StopEvent = () => OnEventStop(timer);
        return timer;
    }

    public static Timer CreateTimer(float duration)
    {
        Timer timer = new Timer
        {
            Duration = duration
        };
        timer.StartEvent = () => OnEventStart(timer);
        timer.StopEvent = () => OnEventStop(timer);
        return timer;
    }

    public static Timer CreateTimer(float duration, bool autoRestart = false, Action<Timer, object> onElapsed = null)
    {
        Timer timer = TimerManager.CreateTimer(duration);
        timer.AutoRestart = autoRestart;
        timer.ElapsedWithDataEvent += onElapsed;
        return timer;
    }

    public static void Update(float deltaTime)
    {
        if (m_timers == null)
        {
            return;
        }
        foreach (Timer timerToRemove in m_timersRemoveBuffer)
        {
            timerToRemove.IsRunning = false;
            m_timers.Remove(timerToRemove);
        }
        foreach (Timer timerToAdd in m_timersAddBuffer)
        {
            m_timers.Add(timerToAdd);
        }
        m_timersAddBuffer.Clear();
        m_timersRemoveBuffer.Clear();

        foreach (Timer timer in m_timers)
        {
            if (timer == null || !timer.IsRunning)
            {
                continue;
            }

            timer.RunningTime += deltaTime;

            if (timer.RunningTime < timer.Duration) //still running... ... .. .
            {
                continue;
            }

            //... reached the Duration !
            timer.ElapsedWithDataEvent?.Invoke(timer, timer.Data);
            timer.ElapsedEvent?.Invoke();

            if (timer.AutoRestart)
            {
                timer.RunningTime -= timer.Duration;
            }
            else
            {
                timer.Stop();
            }
        }
    }

    private static void OnEventStop(Timer timer)
    {
        RemoveTimer(timer);
    }

    private static void OnEventStart(Timer timer)
    {
        StartTimer(timer);
    }

    private static void StartTimer(Timer timer)
    {
        if (m_timers == null)
        {
            m_timers = new List<Timer>();
            m_timersAddBuffer = new List<Timer>();
            m_timersRemoveBuffer = new List<Timer>();
        }

#if UNITY_EDITOR
        if (timer.ElapsedEvent == null && timer.ElapsedWithDataEvent == null)
        {
            Debug.LogError("Both Timer's ElapsedEvent and ElapsedWithDataEvent are null.");
        }
#endif

        if (timer != null)
        {
            if (m_timers.Contains(timer))
            {
                m_timersRemoveBuffer.Add(timer);
            }

            if (!m_timersAddBuffer.Contains(timer))
            {
                m_timersAddBuffer.Add(timer);
            }
        }
    }

    private static void RemoveTimer(Timer timer)
    {
        if (timer != null)
        {
            if (!m_timersRemoveBuffer.Contains(timer))
            {
                m_timersRemoveBuffer.Add(timer);
            }

            if (m_timersAddBuffer.Contains(timer))
            {
                m_timersAddBuffer.Remove(timer);
            }
        }
    }
}

public class Timer
{
    public float Duration { get; set; }
    public bool AutoRestart { get; set; }
    public float RunningTime { set; get; }
    public Action<Timer, object> ElapsedWithDataEvent { get; set; }
    public Action ElapsedEvent { get; set; }
    public Action StartEvent { get; set; }
    public Action StopEvent { get; set; }

    public object Data;
    public bool IsRunning { get; set; }


    public Timer()
    {
        RunningTime = 0.0f;
        AutoRestart = false;
        IsRunning = false;
    }

    public void Start()
    {
        RunningTime = 0f;
        IsRunning = true;
        StartEvent();
    }

    public void Stop()
    {
        StopEvent();
        IsRunning = false;
    }

    public void Restart()
    {
        RunningTime = 0f;
        IsRunning = true;
    }
}


