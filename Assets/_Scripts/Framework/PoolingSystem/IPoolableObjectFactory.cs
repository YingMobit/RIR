using UnityEngine;

public interface IPoolableObjectFactory<PooledObjectType> : IFactory<PooledObjectType> where PooledObjectType : IPoolable
{
    public void DestroyInstance(IPoolable obj);
    public void EnableInstance(IPoolable obj);
    public void DisableInstance(IPoolable obj);
    public bool CollectionCheck { get; }
    public int DefualtCapacity { get; }
    public int MaxCount { get; }
}
