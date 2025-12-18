using System;
using System.Collections.Generic;
using _Scripts.Framework.BulletSystem;
using ECS;
using Lockstep.Math;
using UnityEngine;
using UnityEngine.Pool;
using Component = ECS.Component;

public class BulletSystem : ISystem {
    public int Order => 0;
    private World _world;
    private List<Entity> _bulletToRelease = new();

    #region System
    public void OnInit(World world) {
        // 初始化
        _world = world;
    }

    public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
        var components = ListPool<Component>.Get();
        world.GetComponents(ComponentTypeEnum.BulletComponent,components);
        foreach(var comp in components) {
            if(comp is BulletComponent bullet) {
                bullet.Update(deltaTime.ToLFloat());
            }
        }
        ListPool<Component>.Release(components);
    }

    public void OnFrameLateUpdate(World world,int localFrameCount) {
        var components = ListPool<Component>.Get();
        world.GetComponents(ComponentTypeEnum.BulletComponent,components);
        foreach(var comp in components) {
            if(comp is BulletComponent bullet) {
                bullet.LateUpdate(world);
            }
        }

        foreach(var entity in _bulletToRelease) {
            world.ReleaseEntity(entity);
        }
        ListPool<Component>.Release(components);
        _bulletToRelease.Clear();
    }
    
    public void OnNetworkUpdate(World world, int networkFrameCount){
    
    }

    public void OnDestroy(World world){
        _bulletToRelease.Clear();
        _bulletToRelease = null;
    }
    #endregion
    
    #region API
    public BulletComponent GetNewBullet(
        LVector3 logicalDirection,
        LVector3 decorateDirection, 
        LFloat speed, 
        LFloat lifeTime,
        LFloat radius,
        LayerMask targetLayerMask,
        LVector3 startPosition,
        GameObject bullet,  
        Action<BulletModel> onUpdate,
        Action<BulletModel,Collider[],int> onCollision,
        Action<BulletModel> onDisable) {
        BulletComponent bulletComponent = null;
        var newEntity = _world.GetEntity(bullet,ComponentTypeEnum.BulletComponent.ToMask());
        _world.GetComponentOnEntity(newEntity,ComponentTypeEnum.BulletComponent,out var component);
        bulletComponent = component as BulletComponent;
        bulletComponent.InitModel(logicalDirection,decorateDirection,speed,lifeTime,radius,targetLayerMask,startPosition,bullet,onUpdate,onCollision,onDisable);
        return bulletComponent;
    }

    public void ReleaseBullet(Entity entity) { 
        _bulletToRelease.Add(entity);
    }
    #endregion
}
