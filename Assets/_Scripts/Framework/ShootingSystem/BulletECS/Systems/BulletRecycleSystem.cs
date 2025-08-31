using System.Threading;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateAfter(typeof(BulletLifeSystem))]
public partial class BulletRecycleSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
    protected override void OnCreate() {
        base.OnCreate();
        endSimulationEntityCommandBufferSystem = World.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate() {
        var ecbParallel = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        Dependency = Entities.ForEach((Entity entity,int entityInQueryIndex,in BulletRecycleTag tag) => {
            ecbParallel.DestroyEntity(entityInQueryIndex,entity);
        }).ScheduleParallel(Dependency);
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}