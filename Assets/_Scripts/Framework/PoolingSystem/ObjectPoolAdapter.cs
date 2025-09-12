using UnityEngine.Pool;

public interface IObjectPoolAdapter {
    IPoolable Get();
    void Release(IPoolable obj);
    void Dispose();
    void Clear();

    int CountAll { get; }
    int CountActive { get; }
    int CountInactive { get; }
}

public class ObjectPoolAdapter<PoolableType> : IObjectPoolAdapter where PoolableType : class, IPoolable {
    ObjectPool<PoolableType> _pool;
    public int CountAll => _pool.CountAll;

    public int CountActive => _pool.CountActive;

    public int CountInactive => _pool.CountInactive;

    public void Clear() {
        _pool.Clear();
    }

    public void Dispose() {
        _pool.Dispose();
    }

    public IPoolable Get() {
        return _pool.Get();
    }

    public void Release(IPoolable obj) {
        _pool.Release(obj as PoolableType);
    }

    public ObjectPoolAdapter(ObjectPool<PoolableType> pool) {
        _pool = pool;
    }
}