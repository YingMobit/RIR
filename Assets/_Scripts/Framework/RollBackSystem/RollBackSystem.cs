using ECS;
using PoolingSystem.ReferencePool;
using UnityEngine.Pool;

namespace RollBackSystem {
    public class RollBackSystem : ISystem {
        public int Order => 2;

        public void OnInit(World world) {

        }

        public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
            // 每帧更新
            var list = ListPool<Component>.Get();
            world.GetComponents(ComponentTypeEnum.RollBackComponent,list);
            foreach(var comp in list) {
                if(comp is RollBackComponent rollback) {
                    rollback.SnapShot(world,localFrameCount);
                }
            }
            ListPool<Component>.Release(list);
        }

        public void OnFrameLateUpdate(World world,int localFrameCount) {
            // 帧末更新
        }

        public void OnNetworkUpdate(World world,int networkFrameCount) {

        }

        public void OnDestroy(World world) {

        }

        public void RollBack(World world,int errorStartLocalizedLogicFrameCount,int correctLocalizedLogicFrameCount) {
            var list = ListPool<Component>.Get();
            world.GetComponents(ComponentTypeEnum.RollBackComponent,list);
            foreach(var comp in list) {
                if(comp is RollBackComponent rollback) {
                    rollback.RollBackState(errorStartLocalizedLogicFrameCount,
                                            correctLocalizedLogicFrameCount);
                }
            }
            ListPool<Component>.Release(list);
        }
    }
}

