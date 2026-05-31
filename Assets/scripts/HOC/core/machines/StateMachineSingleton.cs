
public class StateMachineSingleton<T> : StateMachine where T : StateMachine
{
    private static T _instance;

    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<T>(UnityEngine.FindObjectsInactive.Include);
                }
                return _instance;
            }
        }
    }

    public override void Awake()
    {
        base.Awake();

        T[] gos = FindObjectsByType<T>(0);
        if (gos.Length > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public static bool isNull() { return _instance == null; }
}