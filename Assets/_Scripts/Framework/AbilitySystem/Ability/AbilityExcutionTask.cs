using UnityEngine;
using PoolingSystem.ReferencePool;
using RollBackSystem;

namespace GAS {
    public class AbilityExcutionTask : IReference<AbilityExcutionTask> , IRollBackable {
        public AbilityRuntimeContext runtimeContext { get; private set; }
        public Ability Ability => runtimeContext.Ability;
        private AbilityEffect currentEffect => Ability.Effects[runtimeContext.currentEffectIndex];
        public int CurrentInteruptionPriority => currentEffect.InteruptionPriority;

        //驱动事件使用函数传参而不是设置AbilityRuntimeContext是为了避免上下文过期
        public void OnTriggered(AbilityComponentContext abilityComponentContext) {
            runtimeContext.BindComponentContext(abilityComponentContext);
            foreach(var effect in Ability.Effects) { 
                effect.RootBehaviorUnit.OnTriggered(runtimeContext);
            }
        }

        public TaskStatus OnUpdate(AbilityComponentContext abilityComponentContext) {
            runtimeContext.BindComponentContext(abilityComponentContext);
            TaskStatus updateExcutionRes;

            TaskStatus taskStatus = currentEffect.RootBehaviorUnit.OnExcute(runtimeContext);
            if(taskStatus.IsFinished()) {
                if(taskStatus == TaskStatus.Suceeded) {
                    if(runtimeContext.MoveNext()) {
                        return TaskStatus.Running;
                    } else { 
                        return TaskStatus.Suceeded;
                    }
                } else {
                    updateExcutionRes = TaskStatus.Failed;
                }
            } else if(taskStatus == TaskStatus.Running) {
                updateExcutionRes = TaskStatus.Running;
            } else {
                Debug.LogError("Unexpected taskStatus: Unstarted,task didn't start somehow");
                updateExcutionRes = TaskStatus.Failed;
            }

            TaskStatus exitRes;
            for(int i=0;i < runtimeContext.currentEffectIndex;i++) { 
                exitRes = Ability.Effects[i].RootBehaviorUnit.OnExit(runtimeContext,false);
                if(exitRes == TaskStatus.Failed)
                    Debug.LogError($"Effect: {Ability.Effects[i].EffectHeadInfo} exit failed");
            }

            return updateExcutionRes;
        }

        public TaskStatus OnExit(AbilityComponentContext abilityComponentContext) {
            runtimeContext.BindComponentContext(abilityComponentContext);

            if(runtimeContext.currentEffectIndex < Ability.Effects.Count) {
                Debug.LogError("Some effect hasn't finished");
                return TaskStatus.Failed;
            }

            bool allEffectExited = true;
            TaskStatus taskStatus;
            foreach(var effect in Ability.Effects) {
                taskStatus = effect.RootBehaviorUnit.OnExit(runtimeContext,true);
                if(taskStatus.IsFinished()) {
                    if(taskStatus == TaskStatus.Failed) {
                        Debug.LogError($"Effect: {effect.EffectHeadInfo} exit failed");
                        allEffectExited = false;
                    }
                } else { 
                    allEffectExited = false;
                }
            }

            return allEffectExited ? TaskStatus.Suceeded : TaskStatus.Running;
        }

        //暂时不做打断
        public void OnInterrupted(InteruptionContext interuptionContext) {
                
        }

        public void BindRuntimeContext(AbilityRuntimeContext abilityRuntimeContext) {
            runtimeContext = abilityRuntimeContext;
        }

        #region IRefrence
        public uint ReferenceType => ReferenceTypes.ABILITYEXCUTIONTASK;

        int IReference.IndexInRefrencePool { get; set; }

        public void OnRecycle() {
            runtimeContext = null;
        }

        public IReference GetNewInstance() {
            return new AbilityExcutionTask();
        }

        public void Dispose() {
            OnRecycle();
        }
        #endregion

        #region IRollbackable
        internal class AbilityExcutionTaskSnapShot : ISnapShot, IReference<AbilityExcutionTaskSnapShot> {
            internal ISnapShot runtimeContextSnapShot;
            #region Interfaces
            public int LocalizedLogicFrameCount { get; set; }

            public uint ReferenceType => ReferenceTypes.ABILITYEXCUTIONSNAPSHOT;

            int IReference.IndexInRefrencePool { get; set; }

            public void Dispose() {
                OnRecycle();
            }

            public IReference GetNewInstance() {
                return new AbilityExcutionTaskSnapShot();
            }

            public void OnRecycle() {
                if(runtimeContextSnapShot != null) {
                    runtimeContextSnapShot.Release();
                    runtimeContextSnapShot = null;
                }
            }

            public void Release() {
                ReferencePoolingCenter.Instance.ReleaseReference(this);
            }
            #endregion
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            var snapshot = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTaskSnapShot>();
            snapshot.LocalizedLogicFrameCount = localizedLogicFrameCount;
            
            // 快照runtimeContext
            if(runtimeContext != null) {
                snapshot.runtimeContextSnapShot = runtimeContext.SnapShot(localizedLogicFrameCount);
            }
            
            return snapshot;
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var taskSnapShot = snapShot as AbilityExcutionTaskSnapShot;
            if(taskSnapShot == null) {
                Debug.LogError("AbilityExcutionTask RollBack Error: Invalid SnapShot Type");
                return;
            }
            
            // 回滚runtimeContext
            if(taskSnapShot.runtimeContextSnapShot != null) {
                if(runtimeContext == null) {
                    runtimeContext = ReferencePoolingCenter.Instance.GetReference<AbilityRuntimeContext>();
                }
                runtimeContext.RollBack(taskSnapShot.runtimeContextSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
            }
        }
        #endregion
    }
}
