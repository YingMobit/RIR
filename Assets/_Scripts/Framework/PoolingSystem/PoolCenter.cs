using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

public class PoolCenter : Singleton<PoolCenter> {
    struct GameObjectPoolInfo {
        public GameObject PoolRoot;
        public ObjectPool<GameObject> Pool;
    }

    Dictionary<int,IObjectPoolAdapter> PoolableObjectPoolMap = new();

    Dictionary<GameObject,GameObjectPoolInfo> Prefab_PoolMap = new();
    Dictionary<GameObject,GameObject> ActiveInstance_PrefabMap = new();

    protected override void Awake() {
        base.Awake();
        transform.position = Vector3.zero;
    }

    public void RegistPool(GameObject prefab,IPoolableObjectFactory objectFactory = null) {
        if(Prefab_PoolMap.ContainsKey(prefab)) {
            Debug.LogAssertion($"This prefab: {prefab} already has a Pool");
            return;
        }

        var poolRoot = new GameObject($"{prefab.name}_Pool");
        objectFactory ??= new DefaultPoolableGameObjectFactory(prefab);
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            ()=> { var go = objectFactory.CreateInstance(); go.transform.SetParent(poolRoot.transform); return go; } ,
            objectFactory.EnableInstance,
            objectFactory.DisableInstance,
            objectFactory.DestroyInstance,
            objectFactory.CollectionCheck,
            objectFactory.DefualtCapacity,
            objectFactory.MaxCount
        );
        poolRoot.transform.SetParent(transform);
        Prefab_PoolMap.Add(prefab,new GameObjectPoolInfo {PoolRoot = poolRoot,Pool = pool });
    }

    public void RegistPool<PoolableType>(int poolableType,IPoolableObjectFactory<PoolableType> factory = null) where PoolableType : class , IPoolable , new() {
        factory ??= new DefaultPoolableObjectFactory<PoolableType>();
        if(PoolableObjectPoolMap.ContainsKey(poolableType)) {
            Debug.LogAssertion($"This PoolableType: {poolableType} already has a Pool");
            return;
        }
        ObjectPool<PoolableType> pool = new ObjectPool<PoolableType>(
            factory.CreateInstance,
            factory.EnableInstance,
            factory.DisableInstance,
            factory.DestroyInstance,
            factory.CollectionCheck,
            factory.DefualtCapacity,
            factory.MaxCount
        );
        PoolableObjectPoolMap.Add(poolableType,new ObjectPoolAdapter<PoolableType>(pool));
    }

    public GameObject GetInstance(GameObject prefab) {
        if(!Prefab_PoolMap.ContainsKey(prefab)) {
            RegistPool(prefab);
            Debug.LogAssertion($"This prefab: {prefab} hasn't registed pool,use defualt instead");
        }
        var instance = Prefab_PoolMap[prefab].Pool.Get();
        ActiveInstance_PrefabMap.Add(instance,prefab);
        return instance;
    }

    public PoolableType GetInstance<PoolableType>(int poolableObjectType) where PoolableType : IPoolable {
        if(PoolableObjectPoolMap.ContainsKey(poolableObjectType)) { 
            var obj = PoolableObjectPoolMap[poolableObjectType].Get();
            if(obj is PoolableType res) {
                return res;
            } else { 
                Debug.LogError($"This PoolableType: {poolableObjectType} type mismatch, expect:{typeof(PoolableType).Name}, given:{obj.GetType().Name},this probably because of incorrect typeID passing when regist this pool");
                return default;
            }
        }
        Debug.LogError($"This PoolableType: {poolableObjectType} hasn't registed pool");
        return default;
    }


    public void ReleaseInstance(GameObject activeInstance) {
        if(!ActiveInstance_PrefabMap.ContainsKey(activeInstance)) { 
            Debug.LogError($"This instance: {activeInstance} doesn't born form pool");
            return;
        }

        GameObject prefab = ActiveInstance_PrefabMap[activeInstance];
        Prefab_PoolMap[prefab].Pool.Release(activeInstance);
        ActiveInstance_PrefabMap.Remove(activeInstance);
    }

    public void ReleaseInstance(IPoolable poolable) { 
        if(poolable == null) {
            Debug.LogError($"Poolable is null");
            return;
        }
        var type = poolable.PoolableType;   
        if(PoolableObjectPoolMap.ContainsKey(type)) { 
            PoolableObjectPoolMap[type].Release(poolable);
            return;
        }

        Debug.LogError($"This PoolableType: {type} hasn't registed pool");
    }
}

public class DefaultPoolableGameObjectFactory : IPoolableObjectFactory {
    public GameObject Prefab;
    public bool CollectionCheck => true;
    public int DefualtCapacity => 10;
    public int MaxCount => 20;

    public GameObject CreateInstance() {
        return GameObject.Instantiate(Prefab);
    }

    public void DestroyInstance(GameObject obj) {
         GameObject.Destroy(obj);
    }

    public void DisableInstance(GameObject obj) {
        obj.SetActive(false);
    }

    public void EnableInstance(GameObject obj) {
        obj.SetActive(true);
    }

    public DefaultPoolableGameObjectFactory(GameObject prefab) {
        Prefab = prefab;
    }
}

public class DefaultPoolableObjectFactory<T> : IPoolableObjectFactory<T> where T : IPoolable , new() {
    public bool CollectionCheck => true;
    public int DefualtCapacity => 10;
    public int MaxCount => 20;
    public T CreateInstance() {
        return new T();
    }
    public void DestroyInstance(T obj) { obj.Dispose(); }
    public void DisableInstance(T obj) { }
    public void EnableInstance(T obj) { }
}