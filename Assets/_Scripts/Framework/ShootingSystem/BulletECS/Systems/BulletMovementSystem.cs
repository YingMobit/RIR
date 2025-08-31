using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial class BulletMovementSystem : SystemBase {
    protected override void OnUpdate() {
        Dependency = Entities.ForEach((Entity entity,ref BulletMoveComponent moveComponent,ref LocalTransform localTransform,in int entityInQueryIndex) => {
            localTransform.Position += moveComponent.velocity * moveComponent.speed * SystemAPI.Time.DeltaTime;
        }).ScheduleParallel(Dependency);
    }
}