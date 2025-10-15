using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class AbilityExcutionTask : IPoolable {
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

        #region IPoolable
        public int PoolableType => PoolableObjectTypeCollection.AbilityExcutionTask;
        
        public void Dispose() {
            runtimeContext = null;
        }
        
        /// <summary>
        /// 暴露给工厂的重置接口
        /// </summary>
        public void Reset() {
            runtimeContext = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegistePool() {
            PoolCenter.Instance.RegistPool(PoolableObjectTypeCollection.AbilityExcutionTask,new AbilityExcutionTaskFactory());
        }
        #endregion
    }

    public class AbilityExcutionTaskFactory : IPoolableObjectFactory<AbilityExcutionTask> {
        public bool CollectionCheck => true;

        public int DefualtCapacity => 5;

        public int MaxCount => 50;

        public AbilityExcutionTask CreateInstance() { return new(); }

        public void DestroyInstance(AbilityExcutionTask obj) { obj.Dispose(); }

        public void DisableInstance(AbilityExcutionTask obj) { obj.Reset(); }

        public void EnableInstance(AbilityExcutionTask obj) { }
    }
}
