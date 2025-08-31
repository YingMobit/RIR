using System;
using Unity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(BulletMovementSystem))]
public partial class BulletCollisionSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate() {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var ecbParallel = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Dependency = Entities.WithReadOnly(physicsWorld)
            .ForEach((Entity entity,
                    int entityInQueryIndex,
                    in LocalTransform localTransform,
                    in BulletCollisionComponent collision,
                    in BulletDamageComponent bulletDamageComponent) => {

                        var filter = new CollisionFilter {
                            BelongsTo = ~0u,
                            CollidesWith = (uint)collision.collisionLayer.value,
                            GroupIndex = 0
                        };

                        // 使用 CheckSphere 来简单检测是否有碰撞
                        bool hasCollision = physicsWorld.CheckSphere(
                            localTransform.Position,
                            collision.radius,
                            filter,
                            QueryInteraction.Default
                        );

                        if(hasCollision) {
                            // 获取具体的碰撞Entity
                            var hits = new NativeList<DistanceHit>(Allocator.Temp);

                            if(physicsWorld.OverlapSphere(
                                localTransform.Position,
                                collision.radius,
                                ref hits,
                                filter,
                                QueryInteraction.Default
                            )) {
                                // 遍历所有碰撞的Entity
                                for(int i = 0; i < hits.Length; i++) {
                                    var hit = hits[i];
                                    var hitEntity = hit.Entity;

                                    // 给被碰撞的Entity添加BulletHitTag
                                    ecbParallel.AddComponent<BulletHitInfo>(entityInQueryIndex,hitEntity,new() { from = entity,damageInfo = bulletDamageComponent.damageInfo });
                                }
                            }

                            hits.Dispose();

                            // 可选：同时给子弹添加销毁标记
                            // TODO:考虑子弹穿透，但是只增减穿透层数，子弹生命周期不在这里处理
                        }

                    }).ScheduleParallel(Dependency);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}