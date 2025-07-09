using Utility;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Pool;

public class PoolCenter : Singleton<PoolCenter>
{
    Dictionary<Type, ObjectPool<IPoolable>> PoolMap = new();
    Dictionary<Type, GameObject> PoolHolderMap = new();

    public bool RegistPool<PoolableObjectType>(IPoolableObjectFactory<PoolableObjectType> _factory) where PoolableObjectType : IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey(_type)){
            Debug.LogError($"Pool of {_type.Name} has been registed");
            return false;
        } else {
            PoolMap.Add(_type,new ObjectPool<IPoolable>(
                ()=>{ var go = _factory.CreateInstance(); go.Entity?.transform.SetParent(PoolHolderMap[_type].transform); return go; } ,
                _factory.EnableInstance,
                _factory.DisableInstance,
                _factory.DestroyInstance,
                _factory.CollectionCheck,
                _factory.DefualtCapacity,
                _factory.MaxCount
                ));

            GameObject poolHolder = new GameObject(_type.Name+"Pool");
            poolHolder.transform.position = Vector3.zero;
            poolHolder.transform.SetParent(transform);
            PoolHolderMap.Add(_type,poolHolder);
            return true;
        }
    }

    public IPoolable GetInstance<PoolableObjectType>() where PoolableObjectType : IPoolable{
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey(_type)){
            return PoolMap[_type].Get();
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return null;
        }
    }

    public bool ReleaseInstance(IPoolable obj) {
        Type _type = obj.Type;
        if(PoolMap.ContainsKey(_type)) {
            PoolMap[_type].Release(obj);
            return true;
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return false;
        }
    }

    public bool DisposePool<PoolableObjectType>() where PoolableObjectType: IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey(_type)){
            PoolMap[_type].Dispose();
            PoolMap.Remove(_type);
            Destroy(PoolHolderMap[_type]);
            PoolHolderMap.Remove(_type);
            return true;
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return false;
        }
    }

    public bool ClearPool<PoolableObjectType>() where PoolableObjectType: IPoolable{
        Type _type = typeof(PoolableObjectType);
        if (PoolMap.ContainsKey(_type)) {
            PoolMap[_type].Clear();
            return true;
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return false;
        }
    }

    protected override void Awake() {
        base.Awake();
        transform.position = Vector3.zero;
    }
}
