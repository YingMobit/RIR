using System;
using _Scripts.Framework.BulletSystem;
using ECS;
using Lockstep.Math;
using LockStepLMath;
using PoolingSystem.ReferencePool;
using UnityEngine;
using Component = ECS.Component;

public class BulletComponent : Component {
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.BulletComponent;
    public BulletModel Model { get; private set; }
    public Entity EntityCopy { get; private set; }
    
    private bool _disableBulletThisFrame;
    private AttributeSmoothHandler<Vector3> _bulletPositionSmoothHandler;
    private Collider[] _overlapGameObjects;
 
    private const int BULLETPOSITIONSMOOTHTASKID = 0;

    #region Component
    public override void OnAttach(World world,Entity entity) {
        // 初始化组件
        EntityCopy = entity;
    }

    public override void Reset(World world,Entity entity) {
        Model.Reset();
        _disableBulletThisFrame = false;
    }

    public override void OnDestroy() {
        ReferencePoolingCenter.Instance.ReleaseReference(Model);
    }

    public override Component GetNewInstance() {
        return new BulletComponent() {
            _overlapGameObjects = new Collider[10],
            Model = ReferencePoolingCenter.Instance.GetReference<BulletModel>(),
            _bulletPositionSmoothHandler = new AttributeSmoothHandler<Vector3>(),
            _disableBulletThisFrame = false
        };
    }
    #endregion


    #region LifeTime
    public void InitModel(LVector3 logicalDirection,
        LVector3 decorateDirection, 
        LFloat speed, 
        LFloat lifeTime,
        LFloat radius,
        LayerMask targetLayerMask,
        LVector3 startPosition,
        GameObject bullet,  
        Action<BulletModel> onUpdate,
        Action<BulletModel,Collider[],int> onCollision,
        Action<BulletModel> onDisable)  {
        Model.InitModel(logicalDirection,decorateDirection,speed,lifeTime,radius,targetLayerMask,startPosition,bullet,onUpdate,onCollision,onDisable);
    }

    public void Update(LFloat deltaTime) {
        Model.CurrentLogicalPosition += Model.Speed * deltaTime * Model.Direction;
        int smoothFrameCount = (int)(deltaTime.ToFloat() / Time.deltaTime);
        _bulletPositionSmoothHandler.RegisterTask(
            BULLETPOSITIONSMOOTHTASKID,
            Model.Bullet.transform.position,
            Model.CurrentLogicalPosition.ToVector3(),
            smoothFrameCount,
            ApplyBulletPosition,
            LerpBulletPosition,
            BulletPositionEqual);
        
        Model.OnUpdate?.Invoke(Model);
        
        LFloat halfDistance = Model.Speed * deltaTime / 2f.ToLFloat();
        LVector3 center = Model.LastFrameLogicalPosition + halfDistance * Model.Direction;
        LVector3 halfExtends = new LVector3(halfDistance,Model.Radius,Model.Radius);
        LQuaternion quaternion = LQuaternion.FromToRotation(Model.LastFrameLogicalPosition,center);
        int count = 0; 
        count = Physics.OverlapBoxNonAlloc(center.ToVector3(),halfExtends.ToVector3(),_overlapGameObjects,quaternion,Model.TargetLayerMask,QueryTriggerInteraction.Collide);
        while(count > 0 && count == _overlapGameObjects.Length) {
            ResizeOverlapGameObject();
            count = Physics.OverlapBoxNonAlloc(center.ToVector3(),halfExtends.ToVector3(),_overlapGameObjects,quaternion,Model.TargetLayerMask,QueryTriggerInteraction.Collide);
        }

        if(count > 0) {
            Model.OnCollision?.Invoke(Model,_overlapGameObjects,count);
        }
        Array.Clear(_overlapGameObjects,0,_overlapGameObjects.Length);
        
        if(Time.time.ToLFloat() - Model.AwakeTime > Model.LifeTime) {
            _disableBulletThisFrame = true; 
        }
    }
    
    public void LateUpdate(World world) {
        if(_disableBulletThisFrame) {
            Model.OnDisable?.Invoke(Model);
            world.GetSystemByType<BulletSystem>().ReleaseBullet(EntityCopy);
            _disableBulletThisFrame = false;
        }
    }
    #endregion
   

    #region BulletSmoothCallBack
    private void ApplyBulletPosition(Vector3 position) {
        Model.Bullet.transform.position = position;
    }

    private static Vector3 LerpBulletPosition(Vector3 startPosition,Vector3 targetPosition,float t) {
        return Vector3.Lerp(startPosition,targetPosition,t);
    }

    private static bool BulletPositionEqual(Vector3 a,Vector3 b) {
        return (a - b).magnitude <= 0.01f;
    }
    #endregion
    private void ResizeOverlapGameObject() {
        Array.Clear(_overlapGameObjects, 0, _overlapGameObjects.Length);
        _overlapGameObjects = new Collider[_overlapGameObjects.Length + 5];
    }
}
