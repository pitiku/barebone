using Sirenix.OdinInspector;

#if UNITY_EDITOR
#endif

[System.Serializable]
public class Probability
{
    [HorizontalGroup("1", Width = 100, MarginRight = 5), LabelText("weight"), LabelWidth(50)]
    public float m_fWeight = 1.0f;

    [HorizontalGroup("1", Width = 100), LabelText("prob"), LabelWidth(30)]
    [ReadOnly]
    public float m_fProbability;
}