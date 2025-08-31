using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

public class PoolCenter : Singleton<PoolCenter> {
    private const string DefualtSubTypeName = "DefualtSubTypeName";
    Dictionary<(Type, string),ObjectPool<IPoolable>> PoolMap = new();
    Dictionary<(Type, string),GameObject> PoolHolderMap = new();

    public bool RegistPool<PoolableObjectType>(IPoolableObjectFactory<PoolableObjectType> _factory,string subType = DefualtSubTypeName) where PoolableObjectType : IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey((_type, subType))) {
            Debug.LogError($"Pool of {_type.Name} has been registed");
            return false;
        } else {
            PoolMap.Add((_type, subType),new ObjectPool<IPoolable>(
                () => { var go = _factory.CreateInstance(); go.Entity?.transform.SetParent(PoolHolderMap[(_type, subType)].transform); return go; },
                _factory.EnableInstance,
                _factory.DisableInstance,
                _factory.DestroyInstance,
                _factory.CollectionCheck,
                _factory.DefualtCapacity,
                _factory.MaxCount
                ));

            GameObject poolHolder = new GameObject(_type.Name + "Pool");
            poolHolder.transform.position = Vector3.zero;
            poolHolder.transform.SetParent(transform);
            PoolHolderMap.Add((_type, subType),poolHolder);
            return true;
        }
    }

    public IPoolable GetInstance<PoolableObjectType>(string subType = DefualtSubTypeName) where PoolableObjectType : IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey((_type, DefualtSubTypeName))) {
            return PoolMap[(_type, DefualtSubTypeName)].Get();
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return null;
        }
    }

    public bool ReleaseInstance(IPoolable obj,string subType = DefualtSubTypeName) {
        Type _type = obj.Type;
        if(PoolMap.ContainsKey((_type, subType))) {
            PoolMap[(_type, subType)].Release(obj);
            return true;
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return false;
        }
    }

    public bool DisposePool<PoolableObjectType>(string subType = DefualtSubTypeName) where PoolableObjectType : IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey((_type, subType))) {
            PoolMap[(_type, subType)].Dispose();
            PoolMap.Remove((_type, subType));
            Destroy(PoolHolderMap[(_type, subType)]);
            PoolHolderMap.Remove((_type, subType));
            return true;
        } else {
            Debug.LogError($"Pool of {_type.Name} has not been Registed");
            return false;
        }
    }

    public bool ClearPool<PoolableObjectType>(string subType = DefualtSubTypeName) where PoolableObjectType : IPoolable {
        Type _type = typeof(PoolableObjectType);
        if(PoolMap.ContainsKey((_type, subType))) {
            PoolMap[(_type, subType)].Clear();
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
