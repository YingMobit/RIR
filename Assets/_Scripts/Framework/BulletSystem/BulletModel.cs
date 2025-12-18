using System;
using Lockstep.Math;
using PoolingSystem.ReferencePool;
using UnityEngine;

namespace _Scripts.Framework.BulletSystem {
    public class BulletModel : IReference<BulletModel> {
        public LVector3 MainDirection;
        public LVector3 DecorateDirection;
        public LFloat Speed;
        public LFloat LifeTime;
        public LFloat AwakeTime;
        public LFloat Radius;
        public LayerMask TargetLayerMask;
        public bool IgnoreDisableCausedByCollisionThisFrame;

        public LVector3 CurrentLogicalPosition;
        public LVector3 LastFrameLogicalPosition;

        public GameObject Bullet { get; private set; }

        public LVector3 Direction => MainDirection + DecorateDirection;
        
        public Action<BulletModel> OnUpdate;
        public Action<BulletModel,Collider[],int> OnCollision;//Model,Colliders,Count
        public Action<BulletModel> OnDisable;

        #region IReference
        public uint ReferenceType => ReferenceTypes.BULLETMODEL;
        
        int IReference.IndexInReferencePool { get; set; }

        public void OnRecycle() {
            Reset();
        }

        public void Dispose() {
            Reset();
        }

        public IReference GetNewInstance() {
            return new BulletModel();
        }
        #endregion

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
            Action<BulletModel> onDisable) {
            MainDirection = logicalDirection.normalized;
            DecorateDirection = decorateDirection.normalized;
            Speed = speed;
            LifeTime = lifeTime;
            Radius = radius;
            TargetLayerMask = targetLayerMask;
            CurrentLogicalPosition = startPosition;
            LastFrameLogicalPosition = startPosition;
            Bullet = bullet;
            OnUpdate = onUpdate;
            OnCollision = onCollision;
            OnDisable = onDisable;

            Bullet.transform.position = startPosition.ToVector3();
        }

        public void Reset() {
            MainDirection = LVector3.zero;
            DecorateDirection = LVector3.zero;
            Speed = LFloat.zero;
            LifeTime = LFloat.zero;
            AwakeTime = LFloat.zero;
            Radius = LFloat.zero;
            IgnoreDisableCausedByCollisionThisFrame = false;
            OnUpdate = null;
            OnCollision = null;
            OnDisable = null;
        }
    }
}