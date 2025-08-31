using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(BulletDamageSystem))]
public partial class BulletLifeSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate() {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate() {
        var ecbParallel = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        float deltaTime = SystemAPI.Time.DeltaTime;

        Dependency = Entities.ForEach((Entity entity,int entityInQueryIndex,ref BulletLifeComponent lifeComponent) => {
            lifeComponent.cur_LifeTime += deltaTime;
            if(lifeComponent.cur_LifeTime >= lifeComponent.max_LifeTime) {
                ecbParallel.AddComponent<BulletRecycleTag>(entityInQueryIndex,entity);
            }
        }).ScheduleParallel(Dependency);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

}