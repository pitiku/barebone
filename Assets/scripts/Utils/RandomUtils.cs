using System.Collections.Generic;
using UnityEngine;

public static class RandomUtils
{
    #region UpRandom

    [System.Serializable]
    public class UpRandom
    {
        public int m_iSeed;
        public int m_iCount = 0;
        public string m_sSeed = "";

        System.Random m_oRandom = null;
        bool m_bInitializing = false;

        public UpRandom() { }

        public UpRandom(int _iSeed, string _sSeed, int _iCount = 0)
        {
            m_iSeed = _iSeed;
            m_sSeed = _sSeed;
            m_iCount = _iCount;
            initialize();
        }

        public void initialize()
        {
            m_bInitializing = true;
            m_oRandom = new System.Random(m_iSeed);
            for (int iIndex = 0; iIndex < m_iCount; ++iIndex)
            {
                next("Initializing seed");
            }
            m_bInitializing = false;
        }

        public int next(string _sContext)
        {
            if (m_oRandom == null)
            {
                initialize();
            }

            if (!m_bInitializing)
            {
                m_iCount++;
            }

            int iValue = m_oRandom.Next();

#if RANDOM_DEBUG
            if (!_sContext.isNullOrEmpty() && _sContext != "Initializing seed")
            {
                string sText = $"[Random] [Seed:{m_sSeed}:{m_iSeed}] [Count:{m_iCount}] [Value:{iValue}] {_sContext}";

                if (RandomDebugger.Instance != null) { RandomDebugger.Instance.Log(sText); }
            }
#endif

            return iValue;
        }

        public int next(int _iMin, int _iMax, string _sSeed, string _sContext)
        {
            if (m_oRandom == null)
            {
                initialize();
            }

            if (!m_bInitializing)
            {
                m_iCount++;
            }

            int iValue = m_oRandom.Next(_iMin, _iMax);

#if RANDOM_DEBUG
        if (!_sContext.isNullOrEmpty())
        {
            string sText = $"[Random] [Seed:{m_sSeed}:{m_iSeed}] [Count:{m_iCount}] [Value:{iValue}] {_sContext}";
            RandomDebugger.Instance.Log(sText);
        }
#endif
            return iValue;
        }

        public double nextDouble(string _sContext)
        {
            if (m_oRandom == null)
            {
                initialize();
            }

            if (!m_bInitializing)
            {
                m_iCount++;
            }

            double fValue = m_oRandom.NextDouble();

#if RANDOM_DEBUG
            if (!_sContext.isNullOrEmpty())
            {
                string sText = $"[Random] [Seed:{m_sSeed}:{m_iSeed}] [Count:{m_iCount}] [Value:{fValue}] {_sContext}";
                RandomDebugger.Instance.Log(sText);
            }
#endif

            return fValue;
        }
    }

    #endregion

    public const string SEED_MASTER = "MASTER";

    static Dictionary<string, UpRandom> m_dGenerators = new Dictionary<string, UpRandom>();

    [RuntimeInitializeOnLoadMethod]
    static void onLoad()
    {
        m_dGenerators.Clear();
    }

    #region LOAD/SAVE

    public static void setRandoms(List<SavedRandom> _aoRandoms)
    {
        m_dGenerators.Clear();

        foreach (SavedRandom oRandom in _aoRandoms)
        {
            m_dGenerators[oRandom.m_sName] = new UpRandom(oRandom.m_iSeed, oRandom.m_sName, oRandom.m_iCount);
            Deb.log($"{oRandom.m_sName} → Name: {oRandom.m_sName} | Seed: {oRandom.m_iSeed} | Count: {oRandom.m_iCount}", eLogFlags.RANDOM);
        }
    }

    public static void loadRandom(SavedRandom _oRandom)
    {
        //Deb.log($"[Random] Loading → Name: {_oRandom.m_sName} | Seed: {_oRandom.m_iSeed} | Count: {_oRandom.m_iCount}", eLogFlags.RANDOM);
        initializeSeed(_oRandom.m_sName, _oRandom.m_iSeed);

        for (int i = 0; i < _oRandom.m_iCount; i++)
        {
            m_dGenerators[_oRandom.m_sName].nextDouble("");
        }
    }

    public static void setRandom(SavedRandom _oRandoms)
    {
        m_dGenerators[_oRandoms.m_sName] = new UpRandom(_oRandoms.m_iSeed, _oRandoms.m_sName, _oRandoms.m_iCount);
    }

    public static Dictionary<string, UpRandom> getRandoms()
    {
        return m_dGenerators;
    }

    public static UpRandom getUPRandom(string _sSeed)
    {
        if (m_dGenerators.ContainsKey(_sSeed))
        {
            return m_dGenerators[_sSeed];
        }
        return null;
    }

    #endregion

    #region INITIALIZE

    static UpRandom getGenerator(string _s)
    {
        if (_s == null)
        {
            _s = SEED_MASTER;
        }
        if (!existsSeed(_s))
        {
            Deb.log("[Random] The random seed with the key " + _s + " hasnt been initialized. One has just been initialized but this could lead to errors in consistency.", eLogFlags.RANDOM);

            // if initializing master seed, we need to initialize it from a non seeded random number
            if (_s == SEED_MASTER)
            {
                initializeSeed(_s);
            }
            else
            {
                initializeSeed(_s, getInt(SEED_MASTER, "Get new seed"));
            }
        }

        return m_dGenerators[_s];
    }

    public static bool existsSeed(string _sSeed)
    {
        return m_dGenerators.ContainsKey(_sSeed);
    }

    public static void initializeSeed(string _sSeed, int _iSeed = -1)
    {
        Deb.log($"[Random] Initialize seed: {_sSeed}", eLogFlags.RANDOM);

        if (_iSeed == -1)
        {
            _iSeed = Random.Range(0, int.MaxValue);
        }

        m_dGenerators[_sSeed] = new UpRandom(_iSeed, _sSeed);

#if RANDOM_DEBUG
        string sText = $"[Random] [Seed:{_sSeed}:{_iSeed}] Initializing.";
        RandomDebugger.Instance.Log(sText);
#endif
    }

    #endregion

    #region GETTERS

    public static int getInt(this string _sSeed, string _sContext)
    {
        return getGenerator(_sSeed).next(_sContext);
    }

    public static int getInt(this string _sSeed, int _iMin, int _iMax, string _sContext)
    {
        UpRandom oRandom = getGenerator(_sSeed);

        int i = oRandom.next(_iMin, _iMax, _sSeed, _sContext);
        return i;
    }

    public static float value(string _sContext)
    {
        return (float)getGenerator(null).nextDouble(_sContext);
    }

    public static float value(this string _sSeed, string _sContext)
    {
        float fValue = (float)getGenerator(_sSeed).nextDouble(_sContext);
        return fValue;
    }

    public static float range(this string _sSeed, float _fMin, float _fMax, string _sContext)
    {
        return _fMin + (value(_sSeed, _sContext) * (_fMax - _fMin));
    }

    public static float range(this Vector2 _v, string _sSeed, string _sContext)
    {
        return range(_sSeed, _v.x, _v.y, _sContext);
    }

    public static float range(this Vector2 _v)
    {
        return range(null, _v.x, _v.y, "");
    }

    public static int rangeInt(this Vector2 _v, string _sSeed, string _sContext)
    {
        if(_v.x == _v.y) { return (int)_v.x; }
        return getInt(_sSeed, (int)_v.x, (int)(_v.y + 1), _sContext);
    }

    static List<ProbabilityPair<float>> m_aiTempValuesPair = new List<ProbabilityPair<float>>();

    public static int rangeInt(this Vector2 _v, string _sSeed, string _sContext, float _fAddedProb = 0)
    {
        m_aiTempValuesPair.Clear();
        for (int i = 0; i < _v.y - _v.x + 1; i++)
        {
            ProbabilityPair<float> iValue = new ProbabilityPair<float>(_v.y - i, 1);
            m_aiTempValuesPair.Add(iValue);
        }
        m_aiTempValuesPair.normalizeProbabilities();
        return (int)m_aiTempValuesPair.getRandomWithLuck(_sSeed, _sContext, _fAddedProb);
    }

    public static int rangeInt(this string _sSeed, int _iMin, int _iMax, string _sContext)
    {
        return getInt(_sSeed, _iMin, _iMax + 1, _sContext);
    }
    public static T getRandomElement<T>(this List<T> _ao, string _sSeed, string _sContext)
    {
        if(_ao.Count == 0)
        {
            Debug.LogWarning("Trying to get a random element from an empty list. Context: " + _sContext);
            return default(T);
        }

        T oItem = _ao[getInt(_sSeed, 0, _ao.Count, _sContext)];
#if RANDOM_DEBUG
        if (_sContext != "")
        {
            string sText = $"[Random] [Seed:{_sSeed} | Item: {oItem}] {_sContext}";
            RandomDebugger.Instance.Log(sText);
        }
#endif
        return oItem;
    }

    public static int getRandomIndex<T>(this List<T> _ao, string _sSeed, string _sContext)
    {
        return getInt(_sSeed, 0, _ao.Count, _sContext);
    }

    public static int getRandomIndex<T>(this T[] _ao, string _sSeed, string _sContext)
    {
        return getInt(_sSeed, 0, _ao.Length, _sContext);
    }

    public static T getRandomElement<T>(this T[] _ao, string _sSeed, string _sContext)
    {
        T oItem = _ao[getInt(_sSeed, 0, _ao.Length, _sContext)];
#if RANDOM_DEBUG
        if (_sContext != "")
        {
            string sText = $"[Random] [Seed:{_sSeed} | Item: {oItem}] {_sContext}";
            RandomDebugger.Instance.Log(sText);
        }
#endif
        return oItem;
    }

#endregion

    #region RUN HELPERS



    #endregion

    #region SPECIALS

    const int MAX_ITERATIONS = 100;
    public static T getRandomWithoutRepeating<T>(this List<T> _ao, List<T> _aoCensored, string _sSeed, string _sContext)
    {
        int iMaxIterations = MAX_ITERATIONS;

        T oResult = default(T);
        do
        {
            oResult = _ao.getRandomElement(_sSeed, _sContext);
            --iMaxIterations;
        }
        while (iMaxIterations > 0 && _aoCensored != null && _aoCensored.Contains(oResult));
#if RANDOM_DEBUG
        if (_sContext != "")
        {
            string sText = $"[Random] [Seed:{_sSeed} | Item: {oResult}] {_sContext}";
            RandomDebugger.Instance.Log(sText);
        }
#endif
        return oResult;
    }

    public static T getRandomWithoutRepeating<T>(this T[] _ao, T[] _aoCensored, string _sSeed, string _sContext)
    {
        int iMaxIterations = MAX_ITERATIONS;

        T oResult = default(T);
        bool bIsCensored = false;
        do
        {
            oResult = _ao.getRandomElement(_sSeed, _sContext);
            --iMaxIterations;

            for (int iCensoredListIndex = 0; iCensoredListIndex < _aoCensored.Length; iCensoredListIndex++)
            {
                if (oResult.Equals(_aoCensored[iCensoredListIndex]))
                {
                    bIsCensored = true;
                    break;
                }
            }

        }
        while (iMaxIterations > 0 && _aoCensored != null && bIsCensored);

#if RANDOM_DEBUG
        if (_sContext != "")
        {
            string sText = $"[Random] [Seed:{_sSeed} | Item: {oResult}] {_sContext}";
            RandomDebugger.Instance.Log(sText);
        }
#endif
        return oResult;
    }

    public static int getRandomIndexWithoutRepeating<T>(this List<T> _ao, List<int> _aiCensored, string _sSeed, string _sContext)
    {
        int iMaxIterations = MAX_ITERATIONS;

        int iIndex = -1;
        do
        {
            iIndex = _ao.getRandomIndex(_sSeed, _sContext);
            --iMaxIterations;
        }
        while (iMaxIterations > 0 && _aiCensored.Contains(iIndex));

        return iIndex;
    }

    public static Vector3 insideUnitSphere(this string _sSeed, string _sContext)
    {
        float u1 = range(_sSeed, -1.0f, 1.0f, _sContext);
        float u2 = range(_sSeed, 0.0f, 1.0f, _sContext);
        float r = Mathf.Sqrt(1.0f - (u1 * u1));
        float theta = 2.0f * Mathf.PI * u2;


        return new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), u1);
    }

    public static Vector3 insideUnitSphere()
    {
        return insideUnitSphere(null, "");
    }

    public static Vector2 insideUnitCircleUniformly(this string _sSeed)
    {
        float angle = value(_sSeed, "") * Mathf.PI * 2.0f;
        float radius = Mathf.Sqrt(value(_sSeed, ""));
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        return new Vector2(x, y);
    }

    /// <summary>returns a boolean array of _iTotalPositions with _iToMark random positions marked as true</summary>
    public static bool[] getRandomPositions(this string _sSeed, int _iTotalPositions, int _iToMark)
    {
        bool[] ao = new bool[_iTotalPositions];
        int iTries = MAX_ITERATIONS;
        int iAvailablePositions = _iTotalPositions;
        while (iTries > 0)
        {
            for (int i = 0; i < _iTotalPositions; ++i)
            {
                float fProbability = (float)_iToMark / (float)iAvailablePositions;
                if (!ao[i])
                {
                    if (value(_sSeed, "") < fProbability)
                    {
                        ao[i] = true;
                        --_iToMark;
                        --iAvailablePositions;
                        if (_iToMark <= 0)
                        {
                            return ao;
                        }
                    }
                }
            }
        }
        return ao;
    }

    public static void shuffle<T>(this IList<T> list, string _sSeed, string _sContext)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = getInt(_sSeed, 0, n, _sContext);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

#endregion
}