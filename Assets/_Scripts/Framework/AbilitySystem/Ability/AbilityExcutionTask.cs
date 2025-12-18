using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;

namespace GAS {
    public class AbilityExcutionTask : IReference<AbilityExcutionTask>, IRollBackable {
        public AbilityRuntimeContext RuntimeContext { get; private set; }
        public Ability Ability => RuntimeContext.Ability;
        private AbilityEffect CurrentEffect => Ability.Effects[RuntimeContext.currentEffectIndex];
        public int CurrentInterruptionPriority => CurrentEffect.InteruptionPriority;

        private int _lastExitEffectIndex = 0;
        
        public void OnTriggered(AbilityComponentContext abilityComponentContext) {
            RuntimeContext.BindComponentContext(abilityComponentContext);
            foreach(var effect in Ability.Effects) {
                effect.RootBehaviorUnit.OnTriggered(RuntimeContext);
            }
        }

        public TaskStatus OnUpdate(AbilityComponentContext abilityComponentContext) {
            RuntimeContext.BindComponentContext(abilityComponentContext);
            TaskStatus updateExcutionRes;

            TaskStatus taskStatus = CurrentEffect.RootBehaviorUnit.OnExcute(RuntimeContext);
            if(taskStatus.IsFinished()) {
                if(taskStatus == TaskStatus.Suceeded) {
                    if(RuntimeContext.MoveNext()) {
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

            return updateExcutionRes;
        }

        public TaskStatus OnExit(AbilityComponentContext abilityComponentContext) {
            RuntimeContext.BindComponentContext(abilityComponentContext);
            bool allEffectFinished = RuntimeContext.currentEffectIndex == Ability.Effects.Count;
            Debug.Log($"AbilityExitTask:{Ability.AbilityHeadInfo.Name},index:{_lastExitEffectIndex}");
            var exitRes = Ability.Effects[_lastExitEffectIndex].RootBehaviorUnit.OnExit(RuntimeContext,allEffectFinished);
            if(exitRes.IsFinished()) {
                if(exitRes == TaskStatus.Failed) {
                    Debug.LogError($"AbilityExcutionTask OnExcute Failed," +
                                   $"Ability:{Ability.AbilityHeadInfo.Name}," +
                                   $"Effect:{Ability.Effects[_lastExitEffectIndex]}");
                }
                _lastExitEffectIndex++;
                if(_lastExitEffectIndex > RuntimeContext.currentEffectIndex) {
                    _lastExitEffectIndex = 0;
                    return TaskStatus.Suceeded;
                }
            }
            return TaskStatus.Running;
        }
        
        public void OnInterrupted(InteruptionContext interuptionContext) {

        }

        public void BindRuntimeContext(AbilityRuntimeContext abilityRuntimeContext) {
            RuntimeContext = abilityRuntimeContext;
        }

        #region IRefrence
        public uint ReferenceType => ReferenceTypes.ABILITYEXCUTIONTASK;

        int IReference.IndexInReferencePool { get; set; }

        public void OnRecycle() {
            RuntimeContext = null;
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
            internal int lastExitEffectIndex;
            #region Interfaces
            public int LocalizedLogicFrameCount { get; set; }

            public uint ReferenceType => ReferenceTypes.ABILITYEXCUTIONSNAPSHOT;

            int IReference.IndexInReferencePool { get; set; }

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
            snapshot.lastExitEffectIndex = _lastExitEffectIndex;
            
            if(RuntimeContext != null) {
                snapshot.runtimeContextSnapShot = RuntimeContext.SnapShot(localizedLogicFrameCount);
            }

            return snapshot;
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var taskSnapShot = snapShot as AbilityExcutionTaskSnapShot;
            if(taskSnapShot == null) {
                Debug.LogError("AbilityExcutionTask RollBack Error: Invalid SnapShot Type");
                return;
            }
            
            if(taskSnapShot.runtimeContextSnapShot != null) {
                if(RuntimeContext == null) {
                    RuntimeContext = ReferencePoolingCenter.Instance.GetReference<AbilityRuntimeContext>();
                }
                RuntimeContext.RollBack(taskSnapShot.runtimeContextSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                _lastExitEffectIndex = taskSnapShot.lastExitEffectIndex;
            }
        }
        #endregion
    }
}
