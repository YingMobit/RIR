using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility;

public class PoolCenter : Singleton<PoolCenter> {
    struct PoolInfo {
        public GameObject PoolRoot;
        public ObjectPool<GameObject> Pool;
    }

    Dictionary<GameObject,PoolInfo> Prefab_PoolMap = new();
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
        objectFactory ??= new DefaultPoolableObjectFactory(prefab);
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            ()=> { var go = objectFactory.CreateInstance(); if(go.transform.parent == null) { go.transform.SetParent(poolRoot.transform); } return go; } ,
            objectFactory.EnableInstance,
            objectFactory.DisableInstance,
            objectFactory.DestroyInstance,
            objectFactory.CollectionCheck,
            objectFactory.DefualtCapacity,
            objectFactory.MaxCount
        );
        poolRoot.transform.SetParent(transform);
        Prefab_PoolMap.Add(prefab,new PoolInfo {PoolRoot = poolRoot,Pool = pool });
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

    public void ReleaseInstance(GameObject activeInstance) {
        if(!ActiveInstance_PrefabMap.ContainsKey(activeInstance)) { 
            Debug.LogError($"This instance: {activeInstance} doesn't born form pool");
            return;
        }

        GameObject prefab = ActiveInstance_PrefabMap[activeInstance];
        Prefab_PoolMap[prefab].Pool.Release(activeInstance);
        ActiveInstance_PrefabMap.Remove(activeInstance);
    }
}

public class DefaultPoolableObjectFactory : IPoolableObjectFactory {
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

    public DefaultPoolableObjectFactory(GameObject prefab) {
        Prefab = prefab;
    }
}