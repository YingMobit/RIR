using System;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// 子弹生命周期管理
/// </summary>
public struct BulletLifeComponent : IComponentData {
    public float max_LifeTime;
    public float cur_LifeTime;

    public BulletLifeComponent(float _max_LifeTime,float _cur_LifeTime) {
        max_LifeTime = _max_LifeTime;
        cur_LifeTime = _cur_LifeTime;
    }
}

public struct BulletMoveComponent : IComponentData {
    public float speed;
    public float3 velocity;

    public BulletMoveComponent(float _speed,float3 _velocity) {
        speed = _speed;
        velocity = _velocity;
    }
}

public struct BulletCollisionComponent : IComponentData {
    public float radius;
    public LayerMask collisionLayer;

    public BulletCollisionComponent(float _radius,LayerMask _collisionLayer) {
        radius = _radius;
        collisionLayer = _collisionLayer;
    }
}

/// <summary>
/// 子弹伤害信息
/// </summary>
public struct BulletDamageComponent : IComponentData {
    public BulletDamageInfo damageInfo;
    public BulletDamageComponent(BulletDamageInfo _damageInfo) {
        damageInfo = _damageInfo;
    }
}
//Entity被子弹击中之后的Info
public struct BulletHitInfo : IComponentData {
    public Entity from;
    public BulletDamageInfo damageInfo;
}
public struct BulletRecycleTag : IComponentData { }
