using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// 用于管理配置好的Ability的运行时状态
    /// </summary>
    public class AbilityComponent {
        Dictionary<int,Ability> legalAbilities;//所有当前已经注册的Ability
        Dictionary<int,HashSet<AbilityExcutionTask>> runningTasks;//所有当前正在运行的Ability对应的Task
        HashSet<int> runningAbilities;//所有当前正在运行的Ability

        List<Ability> abilitiesToRegist;//等待注册的Ability
        List<int> abilitiesToRemove;//等待移除的AbilityID
        List<int> abilitiesToCreateTask;//等待创建Task的AbilityID
        List<AbilityExcutionTask> tasksToRemove;//等待移除的Task
        HashSet<AbilityExcutionTask> tasksExiting;//正在执行清理工作的Task
        List<AbilityExcutionTask> tasksToRelease;//清理工作完成可回收的Task
        #region API
        public void RegistAbility(Ability ability) {
            if(legalAbilities.ContainsKey(ability.AbilityHeadInfo.ID)) { 
                Debug.Log($"Ability: {ability.AbilityHeadInfo.Name} has already been registered!");
                return;
            }
            abilitiesToRegist.Add(ability);
        }

        public void RemoveAbility(int abilityID) {
            if(!legalAbilities.ContainsKey(abilityID)) { 
                Debug.Log($"AbilityID: {abilityID} has not been registered!");
                return;
            }
            abilitiesToRemove.Add(abilityID);
        }

        public bool AbilityLegal(int abilityID) { 
            return legalAbilities.ContainsKey(abilityID);
        }

        public bool AbilityRunning(int abilityID) { 
            return AbilityLegal(abilityID) && runningAbilities.Contains(abilityID);
        }

        public bool AbilityExiting(int abilityID,out AbilityExcutionTask abilityExcutionTask) {
            abilityExcutionTask = null;
            foreach(var task in tasksExiting) { 
                if(task.Ability.AbilityHeadInfo.ID == abilityID) {
                    abilityExcutionTask = task;
                    return true;
                }
            }
            return false;
        }

        public void InterruptAbility(IIntreruptionContext intreruptionContext) { 
            
        }
        #endregion

        #region Life Time
        public void Update(AbilityComponentContext abilityComponentContext) {
            //尝试触发所有legalAbilities中的Ability
            foreach(var legalAbility in legalAbilities.Values) {
                if(legalAbility.TriggerUnit.TryTrigger(abilityComponentContext) == TaskStatus.Suceeded
                    // TODO: && CoolDownSystem.CanUse(legalAbility)
                    ) { 
                    runningAbilities.Add(legalAbility.AbilityHeadInfo.ID);
                    abilitiesToCreateTask.Add(legalAbility.AbilityHeadInfo.ID);
                }
            }

            //创建Trigger成功的AbilityExcutionTask
            foreach(var toCreateAbility in abilitiesToCreateTask) { 
                RegistTask(toCreateAbility,abilityComponentContext);
            }
            abilitiesToCreateTask.Clear();

            //更新所有正在运行的AbilityExcutionTask
            TaskStatus taskStatus;
            foreach(var tasks in runningTasks.Values) {
                foreach(var task in tasks) { 
                    taskStatus = task.OnUpdate(abilityComponentContext);
                    if(taskStatus.IsFinished()) { 
                        tasksToRemove.Add(task);
                    }
                }
            }

            //移除所有已经完成的AbilityExcutionTask
            foreach(var task in tasksToRemove) { 
                tasksExiting.Add(task);
                HashSet<AbilityExcutionTask> taskSet = runningTasks[task.Ability.AbilityHeadInfo.ID];
                taskSet.Remove(task);
                if(taskSet.Count == 0) {
                    //只移除runningAbilities中的技能ID，不移除runningTasks中的pair,避免重新分配对象
                    runningAbilities.Remove(task.Ability.AbilityHeadInfo.ID);
                }
            }
            tasksToRemove.Clear();
        }

        public void LateUpdate(AbilityComponentContext abilityComponentContext) {
            //完成本帧的技能注册
            foreach(var ability in abilitiesToRegist) { 
                legalAbilities.Add(ability.AbilityHeadInfo.ID,ability);
            }
            abilitiesToRegist.Clear();

            //完成本帧的技能移除
            foreach(var abilityID in abilitiesToRemove) { 
                legalAbilities.Remove(abilityID);
                if(runningAbilities.Contains(abilityID)) {
                    foreach(var task in runningTasks[abilityID]) { 
                        tasksExiting.Add(task);
                    }
                    runningTasks.Remove(abilityID);
                    runningAbilities.Remove(abilityID);
                }
            }
            abilitiesToRemove.Clear();

            //执行本帧的Task清理
            TaskStatus exitStatus;
            foreach(var task in tasksExiting) { 
                exitStatus = task.OnExit(abilityComponentContext);
                if(exitStatus == TaskStatus.Suceeded) {
                    tasksToRelease.Add(task);
                } else if(exitStatus == TaskStatus.Failed) {
                    Debug.LogError($"Exit task of Ability: {task.Ability.AbilityHeadInfo.Name} failed!");
                    tasksToRelease.Add(task);
                } else {
                    Debug.LogAssertion($"Unexpected exitStatus: {exitStatus} from task of Ablity: {task.Ability.AbilityHeadInfo.Name}");
                }
            }

            //完成本帧的Task回收
            foreach(var deadTask in tasksToRelease) { 
                tasksExiting.Remove(deadTask);
                PoolCenter.Instance.ReleaseInstance(deadTask);
            }
            tasksToRelease.Clear();
        }
        #endregion

        #region Tool Function
        private void RegistTask(int abilityID,AbilityComponentContext abilityComponentContext) {
            var newTask = PoolCenter.Instance.GetInstance<AbilityExcutionTask>(PoolableObjectTypeCollection.AbilityExcutionTask);
            var runtimeContext = PoolCenter.Instance.GetInstance<AbilityRuntimeContext>(PoolableObjectTypeCollection.AbilityRuntimeContext);
            runtimeContext.BindAbility(abilityID);
            runtimeContext.BindComponentContext(abilityComponentContext);
            newTask.BindRuntimeContext(runtimeContext);
            HashSet<AbilityExcutionTask> taskSet;
            if(runningTasks.ContainsKey(abilityID)) {
                taskSet = runningTasks[abilityID];
            } else { 
                taskSet = new ();
                runningTasks.Add(abilityID,taskSet);
            }
            taskSet.Add(newTask);
            newTask.OnTriggered(abilityComponentContext);
        }
        #endregion
    }
}