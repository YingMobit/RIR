using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

public class MachineGun : GunBase {
    void Update() {
        if(Input.GetMouseButton(0)) {
            Shoot();
        }
    }

    public override void Shoot() {
        var bulletEntity = bulletFactory.Produce();
        var moveComponent = entityManager.GetComponentData<BulletMoveComponent>(bulletEntity);
        moveComponent.velocity = CursorAimer.Instance.AimDirection;
        var LocalTransform = entityManager.GetComponentData<LocalTransform>(bulletEntity);
        LocalTransform.Position = _spawnPoint.position;
        entityManager.SetComponentData(bulletEntity,moveComponent);
        entityManager.SetComponentData(bulletEntity,LocalTransform);
    }

    protected override void FactoryInit() {
        bulletFactory = new(_spawnPoint,bulletPrefab);
        bulletFactory.AddBuilder(new BulletBuilder<BulletLifeComponent>().Set(new(bulletConfigData.MaxLifeTime,0)))
                     .AddBuilder(new BulletBuilder<BulletMoveComponent>().Set(new(bulletConfigData.BulletSpeed,CursorAimer.Instance.AimDirection)))
                     .AddBuilder(new BulletBuilder<BulletCollisionComponent>().Set(new(bulletConfigData.HitRadius,bulletConfigData.HitMask)))
                     .AddBuilder(new BulletBuilder<BulletDamageComponent>().Set(new(damageInfo.ToBulletDamageInfo())));
    }
    #region "Component Build Funcs"

    #endregion
}