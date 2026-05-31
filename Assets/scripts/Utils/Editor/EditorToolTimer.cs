using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public sealed class EditorToolTimer
{
    public sealed class SegmentData
    {
        public string m_label;
        public TimeSpan m_duration;
        public TimeSpan m_elapsedFromStart;
    }

    public bool m_isRunning => m_stopwatch.IsRunning;
    public TimeSpan m_totalElapsed => m_accumulatedPausedTime + m_stopwatch.Elapsed;
    public IReadOnlyList<SegmentData> m_segments => m_segmentsInternal;

    private readonly Stopwatch m_stopwatch = new();
    private readonly List<SegmentData> m_segmentsInternal = new();

    private TimeSpan m_accumulatedPausedTime = TimeSpan.Zero;
    private TimeSpan m_lastSegmentTimestamp = TimeSpan.Zero;
    private bool m_hasOpenSegment = false;
    private string m_currentSegmentLabel = string.Empty;

    public void Start()
    {
        if (m_stopwatch.IsRunning)
        {
            return;
        }

        m_stopwatch.Start();
    }

    public void Pause()
    {
        if (!m_stopwatch.IsRunning)
        {
            return;
        }

        m_stopwatch.Stop();
        m_accumulatedPausedTime += m_stopwatch.Elapsed;
        m_stopwatch.Reset();
    }

    public void Resume()
    {
        if (m_stopwatch.IsRunning)
        {
            return;
        }

        m_stopwatch.Start();
    }

    public void Reset()
    {
        m_stopwatch.Reset();
        m_segmentsInternal.Clear();
        m_accumulatedPausedTime = TimeSpan.Zero;
        m_lastSegmentTimestamp = TimeSpan.Zero;
        m_hasOpenSegment = false;
        m_currentSegmentLabel = string.Empty;
    }

    public void Restart()
    {
        Reset();
        Start();
    }

    public void BeginSegment(string _label)
    {
        if (string.IsNullOrWhiteSpace(_label))
        {
            _label = "Unnamed Segment";
        }

        if (m_hasOpenSegment)
        {
            EndSegment();
        }

        m_currentSegmentLabel = _label;
        m_hasOpenSegment = true;
        m_lastSegmentTimestamp = m_totalElapsed;
    }

    public void EndSegment()
    {
        if (!m_hasOpenSegment)
        {
            return;
        }

        TimeSpan currentElapsed = m_totalElapsed;
        TimeSpan segmentDuration = currentElapsed - m_lastSegmentTimestamp;

        if (segmentDuration < TimeSpan.Zero)
        {
            segmentDuration = TimeSpan.Zero;
        }

        m_segmentsInternal.Add(new SegmentData
        {
            m_label = m_currentSegmentLabel,
            m_duration = segmentDuration,
            m_elapsedFromStart = currentElapsed
        });

        m_currentSegmentLabel = string.Empty;
        m_hasOpenSegment = false;
    }

    public void Split(string _nextLabel)
    {
        EndSegment();
        BeginSegment(_nextLabel);
    }

    public void DebugTotal(string _prefix = "Tool Timer")
    {
        UnityEngine.Debug.Log($"{_prefix} | Total elapsed: {formatTime(m_totalElapsed)}");
    }

    public void DebugSegments(string _prefix = "Tool Timer", bool _closeOpenSegment = true)
    {
        if (_closeOpenSegment && m_hasOpenSegment)
        {
            EndSegment();
        }

        StringBuilder builder = new();
        builder.AppendLine($"{_prefix} | Total elapsed: {formatTime(m_totalElapsed)}");

        if (m_segmentsInternal.Count == 0)
        {
            builder.AppendLine("No segments recorded.");
            UnityEngine.Debug.Log(builder.ToString());
            return;
        }

        for (int i = 0; i < m_segmentsInternal.Count; i++)
        {
            SegmentData segment = m_segmentsInternal[i];
            builder.AppendLine(
                $"[{i + 1}] {segment.m_label} | Segment: {formatTime(segment.m_duration)} | Accumulated: {formatTime(segment.m_elapsedFromStart)}");
        }

        UnityEngine.Debug.Log(builder.ToString());
    }

    public T Measure<T>(Func<T> _func, string _label = null)
    {
        if (_func == null)
        {
            throw new ArgumentNullException(nameof(_func));
        }

        string label = string.IsNullOrWhiteSpace(_label) ? "Measured Block" : _label;

        BeginSegment(label);
        T result = _func.Invoke();
        EndSegment();

        return result;
    }

    public void Measure(Action _action, string _label = null)
    {
        if (_action == null)
        {
            throw new ArgumentNullException(nameof(_action));
        }

        string label = string.IsNullOrWhiteSpace(_label) ? "Measured Block" : _label;

        BeginSegment(label);
        _action.Invoke();
        EndSegment();
    }

    private static string formatTime(TimeSpan _time)
    {
        return $"{_time.TotalSeconds:0.000}s ({_time.TotalMilliseconds:0.###} ms)";
    }
}