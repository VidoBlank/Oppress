

using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : Component {

    protected static T _Instance;

    public static T Instance {
        get {
            if (_Instance == null) {
                var name = typeof(T).Name;
                var go = new GameObject(name);
                _Instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
            return _Instance;
        }
    }

    public static bool Exists {
        get {
            return _Instance != null;
        }
    }

    protected virtual void Awake() {
        if (_Instance != null) {
            throw new System.Exception($"{nameof(T)} Instance Is Already Exists");
        } else {
            _Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }

    public bool IsDisposed {
        get; private set;
    }

    protected virtual void OnDestroy() {
        IsDisposed = true;
        if (_Instance == this) {
            _Instance = null;
        }
    }
}