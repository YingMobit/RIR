using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbilitySystem {
    /// <summary>
    /// ���ڹ������úõ�Ability������ʱ״̬
    /// </summary>
    public class AbilityComponent {
        Dictionary<int,Ability> legalAbilities;//���е�ǰ�Ѿ�ע���Ability
        Dictionary<int,HashSet<AbilityExcutionTask>> runningTasks;//���е�ǰ�������е�Ability��Ӧ��Task
        HashSet<int> runningAbilities;//���е�ǰ�������е�Ability

        List<Ability> abilitiesToRegist;//�ȴ�ע���Ability
        List<int> abilitiesToRemove;//�ȴ��Ƴ���AbilityID
        List<int> abilitiesToCreateTask;//�ȴ�����Task��AbilityID
        List<AbilityExcutionTask> tasksToRemove;//�ȴ��Ƴ���Task
        HashSet<AbilityExcutionTask> tasksExiting;//����ִ����������Task
        List<AbilityExcutionTask> tasksToRelease;//��������ɿɻ��յ�Task
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
            //���Դ�������legalAbilities�е�Ability
            foreach(var legalAbility in legalAbilities.Values) {
                if(legalAbility.TriggerUnit.TryTrigger(abilityComponentContext) == TaskStatus.Suceeded
                    // TODO: && CoolDownSystem.CanUse(legalAbility)
                    ) { 
                    runningAbilities.Add(legalAbility.AbilityHeadInfo.ID);
                    abilitiesToCreateTask.Add(legalAbility.AbilityHeadInfo.ID);
                }
            }

            //����Trigger�ɹ���AbilityExcutionTask
            foreach(var toCreateAbility in abilitiesToCreateTask) { 
                RegistTask(toCreateAbility,abilityComponentContext);
            }
            abilitiesToCreateTask.Clear();

            //���������������е�AbilityExcutionTask
            TaskStatus taskStatus;
            foreach(var tasks in runningTasks.Values) {
                foreach(var task in tasks) { 
                    taskStatus = task.OnUpdate(abilityComponentContext);
                    if(taskStatus.IsFinished()) { 
                        tasksToRemove.Add(task);
                    }
                }
            }

            //�Ƴ������Ѿ���ɵ�AbilityExcutionTask
            foreach(var task in tasksToRemove) { 
                tasksExiting.Add(task);
                HashSet<AbilityExcutionTask> taskSet = runningTasks[task.Ability.AbilityHeadInfo.ID];
                taskSet.Remove(task);
                if(taskSet.Count == 0) {
                    //ֻ�Ƴ�runningAbilities�еļ���ID�����Ƴ�runningTasks�е�pair,�������·������
                    runningAbilities.Remove(task.Ability.AbilityHeadInfo.ID);
                }
            }
            tasksToRemove.Clear();
        }

        public void LateUpdate(AbilityComponentContext abilityComponentContext) {
            //��ɱ�֡�ļ���ע��
            foreach(var ability in abilitiesToRegist) { 
                legalAbilities.Add(ability.AbilityHeadInfo.ID,ability);
            }
            abilitiesToRegist.Clear();

            //��ɱ�֡�ļ����Ƴ�
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

            //ִ�б�֡��Task����
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

            //��ɱ�֡��Task����
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