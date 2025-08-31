using System;
using System.ComponentModel.Design;
using Unity.Entities;
using UnityEngine.UIElements;

/// <summary>
/// 子弹构建器抽象类，用于统一类型
/// </summary>
public abstract class BulletBuilder {
    public abstract void Build(Entity entity);
}

/// <summary>
/// 各类型组件的泛型子弹构建器
/// </summary>
/// <typeparam name="TBulletComponent">具体的组件类型</typeparam>
public class BulletBuilder<TBulletComponent> : BulletBuilder where TBulletComponent : unmanaged, IComponentData {
    TBulletComponent Prototype;
    EntityManager entityManager;
    Func<TBulletComponent,TBulletComponent> OnClone;

    /// <summary>
    /// 设置组件原型
    /// </summary>
    /// <param name="prototype">组件原型</param>
    /// <param name="prototype">构建新组件时的拷贝回调</param>
    public virtual BulletBuilder<TBulletComponent> Set(TBulletComponent prototype,Func<TBulletComponent,TBulletComponent> onClone = null) {
        Prototype = prototype;
        OnClone = onClone ?? ((arche) => { return Prototype; });
        return this;
    }

    /// <summary>
    /// 构建组件
    /// </summary>
    /// <param name="entity"></param>
    public override void Build(Entity entity) {
        if(!entityManager.HasComponent<TBulletComponent>(entity))
            entityManager.AddComponent<TBulletComponent>(entity);
        entityManager.SetComponentData<TBulletComponent>(entity,OnClone.Invoke(Prototype));
    }

    public BulletBuilder() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }
}
