using ECS;
using PoolingSystem.ReferencePool;
using System.Collections.Generic;
using UnityEngine;
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
            SnapShot(world,0);
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
            // Debug.Log($"[RollBackComponent] SnapShot at frame {logicFrameCount} for Entity {currentEntity.EntityID}");
            var compList = ListPool<Component>.Get();
            world.GetAllComponentsOnEntity(currentEntity,compList);
            // Debug.Log($"[RollBackComponent] Entity {currentEntity.EntityID} has {compList.Count} components to SnapShot");
            var handlers = ListPool<SnapShotHandler>.Get();
            if(handlers.Capacity < compList.Capacity) handlers.Capacity = compList.Capacity;
            foreach(var comp in compList) {
                if(comp is IRollBackable rollback) {
                    // Debug.Log($"[RollBackComponent] SnapShot {rollback.GetType().Name} at frame {logicFrameCount}");
                    var handler = ReferencePoolingCenter.Instance.GetReference<SnapShotHandler>();
                    handler.Bind(rollback,rollback.SnapShot(logicFrameCount));
                    handlers.Add(handler);
                } else { 
                    // Debug.LogWarning($"[RollBackComponent] Component {comp.GetType().Name} does not implement IRollBackable, skipped SnapShot");
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
                // Debug.Log($"[RollBackComponent] RollBack {handler.rollBackable.GetType().Name} from frame {currentLocalizedLogicFrameCount} to frame {errorStartLocalizedLogicFrameCount}");
                handler.RollBack(errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
            }
        }

        private void FreeSnapShotHandler(List<SnapShotHandler> outTime) {
            foreach(var handler in outTime) {
                handler.snapShot.Release();
                ReferencePoolingCenter.Instance.ReleaseReference(handler);
            }
            outTime.Clear();
            ListPool<SnapShotHandler>.Release(outTime);
        }
        #endregion
    }
}

