using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class RandomDebugSession
{
    public string Id;
    public DateTime Start;
}

public class RandomDebugger : SceneSingleton<RandomDebugger>
{
#if UNITY_EDITOR
    private const string PATH = "Assets/debug_errors";

    [SerializeField] bool m_bAutoEnable = false;
    [SerializeField] bool m_bEnabled = false;
    [SerializeField] bool m_bDebugToConsole = false;

    private const int MAX_ENTRIES = 250_000_000;
    private readonly string[] m_entries = new string[MAX_ENTRIES];
    [SerializeField, ReadOnly] private int m_iIndex = 0;
    private bool m_bUseTimestamps = false;

    [SerializeField, ValueDropdown("GetSeedOptions")] private string[] m_asIncludeSeeds = null;
    [SerializeField, ValueDropdown("GetSeedOptions")] private string[] m_asExcludeSeeds = null;

    [SerializeField, ReadOnly] private RandomDebugSession m_oCurrentSession;

    private static string[] GetSeedOptions()
    {
        return new[]
        {
        "RUN",
        "MAP",
        "NODE",
        "SCENE"
        };
    }

    private void OnDestroy()
    {
        if (m_bEnabled)
        {
            m_bEnabled = false;
            EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
        }
    }

    private void OnDisable()
    {
        if (m_bEnabled)
        {
            m_bEnabled = false;
            EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
        }
    }

    private void onPlayModeStateChanged(PlayModeStateChange _state)
    {
        if (_state == PlayModeStateChange.ExitingPlayMode)
        {
            if (m_bEnabled && m_iIndex > 0)
            {
                Deb.log("[RandomDebugger] Auto dump on ExitingPlayMode", eLogFlags.RANDOM);

                //DumpToFile();
            }
        }
    }

    private void Start()
    {
        if (m_bAutoEnable)
        {
            Enable();
        }

        Deb.m_bShowTimeFrame = false;
    }

    public void Enable(bool _useTimestamps = false)
    {
        if(m_bEnabled)
        {
            return;
        }

        EditorApplication.playModeStateChanged += onPlayModeStateChanged;

        m_bEnabled = true;
        m_bUseTimestamps = _useTimestamps;

        m_oCurrentSession = new RandomDebugSession()
        {
            Start = DateTime.Now,
            Id = $"Session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{RandomUtils.getInt("", "")}"
        };

        Clear();
    }

    public void Disable()
    {
        m_bEnabled = false;
    }

    public void Clear()
    {
        Array.Clear(m_entries, 0, m_entries.Length);
        m_iIndex = 0;
    }

    public void Log(string _msg)
    {
        if(m_bEnabled && m_iIndex < MAX_ENTRIES && EntryPassesFilters(_msg))
        {
            if (m_bUseTimestamps)
            {
                m_entries[m_iIndex++] = $"{Time.frameCount} | {_msg}";
            }
            else
            {
                m_entries[m_iIndex++] = _msg;
            }

            if (m_bDebugToConsole)
            {
                Deb.log(_msg, eLogFlags.RANDOM);
            }
        }
    }

    public void IncludeStringSeeds(params string[] _seeds)
    {
        m_asIncludeSeeds =
            _seeds?.Where(s => !string.IsNullOrWhiteSpace(s))
                   .Select(s => s.Trim())
                   .ToArray();
    }

    public void ExcludeStringSeeds(params string[] _seeds)
    {
        m_asExcludeSeeds =
            _seeds?.Where(s => !string.IsNullOrWhiteSpace(s))
                   .Select(s => s.Trim())
                   .ToArray();
    }

    private bool EntryPassesFilters(string _entry)
    {
        // Extract:
        // [Seed:<stringSeed>:<intSeed>]

        int seedStart = _entry.IndexOf("[Seed:");
        if (seedStart < 0) return true; 

        seedStart += 6; // move after "[Seed:"

        // discard bad formatted strings
        int mid = _entry.IndexOf(":", seedStart);
        if (mid < 0)
        {
            return false;
        }

        int end = _entry.IndexOf("]", mid);
        if (end < 0)
        {
            return false;
        }

        string sSeed = _entry.Substring(seedStart, mid - seedStart);

        if (m_asIncludeSeeds != null)
        {
            return m_asIncludeSeeds.Contains(sSeed);
        }

        if (m_asExcludeSeeds != null)
        {
            return !m_asExcludeSeeds.Contains(sSeed);
        }

        return true;
    }

    [Button("Reset")]
    private void reset()
    {
        m_oCurrentSession = null;
        m_bEnabled = false;

        Enable();
    }

    [Button("Dump to file")]
    public void DumpToFile()
    {
        string sDirectory = PATH;
        try
        {
            if (!Directory.Exists(sDirectory))
            {
                Directory.CreateDirectory(sDirectory);
            }

            string sSessionID = m_oCurrentSession != null
                ? m_oCurrentSession.Id
                : "UnknownSession";

            string sFileName = $"RandomLog_{sSessionID}.txt";
            string sFullPath = Path.Combine(sDirectory, sFileName);

            File.WriteAllLines(sFullPath, m_entries.Take(m_iIndex));

            Deb.log($"[RandomDebugger] Dumped {m_iIndex} entries to {sFullPath}", eLogFlags.RANDOM);
        }
        catch (Exception e)
        {
            Deb.logError($"[RandomDebugger] Error dumping log: {e}");
        }
    }

    public int GetEntryCount()
    {
        return m_iIndex;
    }

    #region Files comparison

    [ValueDropdown("GetLogFiles")]
    [SerializeField, FoldoutGroup("Files comparison")] private string m_sFileA;

    [ValueDropdown("GetLogFiles")]
    [SerializeField, FoldoutGroup("Files comparison")] private string m_sFileB;

    private IEnumerable<string> GetLogFiles()
    {
        if (!Directory.Exists(PATH))
            yield break;

        foreach (var file in Directory.GetFiles(PATH, "*.txt"))
            yield return file;
    }

    [SerializeField, ReadOnly, FoldoutGroup("Files comparison")] int m_iDiffLine;
    [SerializeField, ReadOnly, FoldoutGroup("Files comparison")] public string m_sLineA;
    [SerializeField, ReadOnly, FoldoutGroup("Files comparison")] public string m_sLineB;

    [Button(ButtonSizes.Medium), FoldoutGroup("Files comparison")]
    public void Compare()
    {
        if (string.IsNullOrEmpty(m_sFileA) || string.IsNullOrEmpty(m_sFileB))
        {
            Debug.LogError("Both files must be selected.");
            return;
        }

        if (FindFirstDifference(
            m_sFileA, m_sFileB,
            out m_iDiffLine,
            out m_sLineA,
            out m_sLineB))
        {
            Deb.log($"Difference found at line {m_iDiffLine}", eLogFlags.RANDOM);
        }
        else
        {
            Deb.log("Both files are equal.", eLogFlags.RANDOM);
            m_iDiffLine = 0;
            m_sLineA = "";
            m_sLineB = "";
        }
    }
    public static bool FindFirstDifference(
    string _fileA,
    string _fileB,
    out int _lineIndex,
    out string _lineA,
    out string _lineB)
    {
        _lineIndex = 0;
        _lineA = null;
        _lineB = null;

        using (var readerA = new StreamReader(_fileA))
        using (var readerB = new StreamReader(_fileB))
        {
            int index = 1;

            while (true)
            {
                string a = readerA.ReadLine();
                string b = readerB.ReadLine();

                // Both ended → files are equal
                if (a == null && b == null)
                    return false; // no difference

                // If one ends earlier → difference
                if (a == null || b == null)
                {
                    _lineIndex = index;
                    _lineA = a;
                    _lineB = b;
                    return true;
                }

                // Normal difference
                if (a != b)
                {
                    _lineIndex = index;
                    _lineA = a;
                    _lineB = b;
                    return true;
                }

                index++;
            }
        }
    }
    #endregion Files comparison

#endif
}