using System;
using Unity;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(BulletCollisionSystem))]
public partial class BulletDamageSystem : SystemBase {
    protected override void OnUpdate() {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        //单线程处理
        GameObject hitGO;
        foreach(var (damageComponent, entity) in SystemAPI.Query<RefRO<BulletDamageComponent>>().WithEntityAccess()) {
            hitGO = GameObjectEntityMappingSystem.Instance.FindGameObject(entity);
            if(hitGO) {
                hitGO.GetComponent<IDamageable>()?.TakeDamage(damageComponent.ValueRO.damageInfo.ToDamageInfo());
            }
            ecb.RemoveComponent<BulletHitInfo>(entity);
        }
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}