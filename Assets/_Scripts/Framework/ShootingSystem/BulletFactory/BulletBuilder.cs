using System;
using System.ComponentModel.Design;
using Unity.Entities;
using UnityEngine.UIElements;

/// <summary>
/// �ӵ������������࣬����ͳһ����
/// </summary>
public abstract class BulletBuilder {
    public abstract void Build(Entity entity);
}

/// <summary>
/// ����������ķ����ӵ�������
/// </summary>
/// <typeparam name="TBulletComponent">������������</typeparam>
public class BulletBuilder<TBulletComponent> : BulletBuilder where TBulletComponent : unmanaged, IComponentData {
    TBulletComponent Prototype;
    EntityManager entityManager;
    Func<TBulletComponent,TBulletComponent> OnClone;

    /// <summary>
    /// �������ԭ��
    /// </summary>
    /// <param name="prototype">���ԭ��</param>
    /// <param name="prototype">���������ʱ�Ŀ����ص�</param>
    public virtual BulletBuilder<TBulletComponent> Set(TBulletComponent prototype,Func<TBulletComponent,TBulletComponent> onClone = null) {
        Prototype = prototype;
        OnClone = onClone ?? ((arche) => { return Prototype; });
        return this;
    }

    /// <summary>
    /// �������
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
