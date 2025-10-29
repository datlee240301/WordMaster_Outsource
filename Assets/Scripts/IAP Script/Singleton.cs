using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(T)) as T;

                if (_instance == null)
                {
                    _instance = new GameObject().AddComponent<T>();
                    _instance.gameObject.name = _instance.GetType().Name;
                }
            }
            return _instance;
        }
    }

    public void Reset()
    {
        _instance = null;
    }

    public static bool Exists()
    {
        return (_instance != null);
    }
}