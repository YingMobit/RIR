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
