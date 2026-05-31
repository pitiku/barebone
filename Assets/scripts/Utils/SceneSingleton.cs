using UnityEngine;

public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (isNull())
                {
                    _instance = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        string s = typeof(T).ToString().toLower();
        T[] gos = FindObjectsByType<T>(FindObjectsSortMode.None);
        if (gos.Length > 1)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else if (gos.Length == 1 && isNull())
        {
            _instance = gos[0]; 
        }
    }

    public virtual void update() { }

    public static bool isNull() { return _instance == null; }
}