using System.Net.Sockets;
using UnityEngine;

public interface IPoolableObjectFactory : IFactory<GameObject>
{
    public void DestroyInstance(GameObject obj);
    public void EnableInstance(GameObject obj);
    public void DisableInstance(GameObject obj);
    public bool CollectionCheck { get; }
    public int DefualtCapacity { get; }
    public int MaxCount { get; }
}

public interface IPoolableObjectFactory<T> : IFactory<T> where T : IPoolable , new() {
    public void DestroyInstance(T obj);
    public void EnableInstance(T obj);
    public void DisableInstance(T obj);
    public bool CollectionCheck { get; }
    public int DefualtCapacity { get; }
    public int MaxCount { get; }
}
