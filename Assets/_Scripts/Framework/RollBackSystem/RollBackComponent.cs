using ECS;
using PoolingSystem.ReferencePool;
using System.Collections.Generic;
using UnityEngine.Pool;
using Utility;
using Component = ECS.Component;

namespace RollBackSystem {
    public class RollBackComponent : Component {
        const int SNAPSHOTCACHESIZE = 60;
        DeQueue<List<SnapShotHandler>> cachedSnapShots = new(SNAPSHOTCACHESIZE);
        Entity currentEntity;

        #region Component Overrides
        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.RollBackComponent;

        public override void OnAttach(World world,Entity entity) {
            currentEntity = entity;
            // SnapShot(0);
        }

        public override void Reset(World world,Entity entity) {

        }

        public override void OnDestroy() {
        }

        public override Component GetNewInstance() {
            return new RollBackComponent();
        }
        #endregion

        #region API
        public void SnapShot(World world,int logicFrameCount) {
            var compList = ListPool<Component>.Get();
            world.GetAllComponentsOnEntity(currentEntity,compList);
            var handlers = ListPool<SnapShotHandler>.Get();
            if(handlers.Capacity < compList.Capacity) handlers.Capacity = compList.Capacity;
            foreach(var comp in compList) { 
                if(comp is IRollBackable rollback) {
                    var handler = ReferencePoolingCenter.Instance.GetReference<SnapShotHandler>();
                    handler.Bind(rollback,rollback.SnapShot(logicFrameCount));
                    handlers.Add(handler);
                }
            }

            if(cachedSnapShots.Count == SNAPSHOTCACHESIZE) {
                var outTime = cachedSnapShots.PopFront();
                FreeSnapShotHandler(outTime);
            }

            cachedSnapShots.PushBack(handlers);
            ListPool<Component>.Release(compList);
        }

        public void RollBackState(int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            int count = currentLocalizedLogicFrameCount - errorStartLocalizedLogicFrameCount + 1;
            for(int i=0;i < count; i++) { 
                var outTime = cachedSnapShots.PopBack();
                FreeSnapShotHandler(outTime);
            }
            var lastCorrectSnapData = cachedSnapShots.PeekBack();
            foreach(var handler in lastCorrectSnapData) {
                handler.RollBack();
            }
        }

        private void FreeSnapShotHandler(List<SnapShotHandler> outTime) {
            foreach(var handler in outTime) {
                ReferencePoolingCenter.Instance.ReleaseReference(handler);
            }
            outTime.Clear();
            ListPool<SnapShotHandler>.Release(outTime);
        }
        #endregion
    }
}

