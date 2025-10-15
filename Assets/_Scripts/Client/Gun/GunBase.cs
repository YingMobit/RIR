using System.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public abstract class GunBase : MonoBehaviour, IShootable {
    [Header("Config")]
    [SerializeField] protected Transform _spawnPoint;
    [SerializeField] protected BulletConfigData bulletConfigData;

    [Header("Interface")]
    public ShootableType shootableType => 0;
    public Transform SpawnPoint => _spawnPoint;

    [Header("RunTimeData")]
    protected BulletFactory bulletFactory;
    protected EntityManager entityManager;
    protected Entity bulletPrefab;
    protected bool entityPrefabReady;
    [Header("BulletComponentPrototype")]
    [SerializeField] protected DamageInfo damageInfo;
    protected virtual void Start() {
        StartCoroutine(GetECSSystems());
        StartCoroutine(EntityPrefabInit());
        StartCoroutine(StartFactoryInit());
    }
    private IEnumerator StartFactoryInit() {
        yield return new WaitUntil(() => entityPrefabReady);
        FactoryInit();
    }
    protected virtual IEnumerator EntityPrefabInit() {
        yield return new WaitUntil(() => EntityBaker.Instance.Ready);
        bulletPrefab = EntityBaker.Instance.GetEntityPrototype(bulletConfigData.Prefab);
        entityPrefabReady = true;
    }
    protected abstract void FactoryInit();
    protected virtual IEnumerator GetECSSystems() {
        yield return new WaitWhile(() => World.DefaultGameObjectInjectionWorld == null);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
    public abstract void Shoot();
}
