
public class Singleton<T> where T : class, new()
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
                    _instance = new T();
                }
                return _instance;
            }
        }
    }

    public static bool isNull() { return _instance == null; }
}