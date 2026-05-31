[System.Serializable]
public class SavedRandom
{
    public string m_sName;
    public int m_iSeed;
    public int m_iCount = 0;

    public SavedRandom() { }

    public SavedRandom(string _sName, RandomUtils.UpRandom _oRandom)
    {
        m_sName = _sName;
        m_iSeed = _oRandom.m_iSeed;
        m_iCount = _oRandom.m_iCount;
    }
}
