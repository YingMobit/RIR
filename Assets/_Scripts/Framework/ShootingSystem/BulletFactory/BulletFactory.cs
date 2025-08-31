using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// 工厂类，用于容纳Bilders并返回子弹实体
/// </summary>
public class BulletFactory {
    private struct BuilderInfo {
        public BulletBuilder builder;
        public Type buildType;

        public BuilderInfo(BulletBuilder _builder,Type _type) {
            builder = _builder;
            buildType = _type;
        }
    }

    private List<BuilderInfo> Builders = new();
    private EntityManager entityManager;
    private Transform spawnPoint;
    private Entity prefab;

    public BulletFactory AddBuilder<TBuildComponent>(BulletBuilder<TBuildComponent> builder) where TBuildComponent : unmanaged, IComponentData {
        Builders.Add(new(builder,typeof(TBuildComponent)));
        return this;
    }

    public Entity Produce() {
        Entity bullet = entityManager.Instantiate(prefab);
        entityManager.SetName(bullet,"NormalBullet");
        foreach(var builderInfo in Builders) {
            builderInfo.builder.Build(bullet);
        }
        return bullet;
    }

    public BulletFactory(Transform _spawnPoint,Entity _prefab) {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        spawnPoint = _spawnPoint;
        prefab = _prefab;
    }
}
