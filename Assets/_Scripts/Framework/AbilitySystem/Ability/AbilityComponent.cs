using ECS;
using System.Collections.Generic;
using UnityEngine;
using Component = ECS.Component;

namespace GAS {
    /// <summary>
    /// ���ڹ������úõ�Ability������ʱ״̬
    /// </summary>
    public class AbilityComponent : Component {
        public AttributeSet AttributeSet;

        Dictionary<int,Ability> legalAbilities = new();//���е�ǰ�Ѿ�ע���Ability
        Dictionary<int,HashSet<AbilityExcutionTask>> runningTasks = new();//���е�ǰ�������е�Ability��Ӧ��Task
        HashSet<int> runningAbilities = new();//���е�ǰ�������е�Ability

        List<Ability> abilitiesToRegist = new();//�ȴ�ע���Ability
        List<int> abilitiesToRemove = new();//�ȴ��Ƴ���AbilityID
        List<int> abilitiesToCreateTask = new();//�ȴ�����Task��AbilityID
        List<AbilityExcutionTask> tasksToRemove = new();//�ȴ��Ƴ���Task
        HashSet<AbilityExcutionTask> tasksExiting = new();//����ִ����������Task
        List<AbilityExcutionTask> tasksToRelease = new();//��������ɿɻ��յ�Task
        List<AbilityRuntimeContext> tasksToRercover = new();//�ȴ�֡ĩ�ָ���task

        public bool Inited { get; private set; } = false;

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

        public InteruptionHandler InterruptAbility(InteruptionContext interuptionContext) {
            List<AbilityExcutionTask> interuptedTasks = new();
            foreach(var tasks in runningTasks.Values) {
                foreach(var task in tasks) {
                    if(task.runtimeContext.Interuptable && task.CurrentInteruptionPriority < interuptionContext.InteruptionPriority) {
                        interuptedTasks.Add(task);
                    }
                }
            }

            foreach(var task in interuptedTasks) {
                runningTasks[task.Ability.AbilityHeadInfo.ID].Remove(task);
                task.OnInterrupted(interuptionContext);
                runningAbilities.Remove(task.Ability.AbilityHeadInfo.ID);
            }

            List<AbilityRuntimeContext> pictures = new();
            foreach(var task in interuptedTasks) {
                pictures.Add(task.runtimeContext);
                PoolCenter.Instance.ReleaseInstance(task);
            }

            return new(pictures);
        }

        public InteruptionHandler InterruptAbility(int abilitID,InteruptionContext interuptionContext) {
            if(!AbilityLegal(abilitID) || !runningAbilities.Contains(abilitID))
                return new(new());
            List<AbilityExcutionTask> interruptionTasks = new();
            foreach(var task in runningTasks[abilitID]) {
                if(task.runtimeContext.Interuptable) {
                    interruptionTasks.Add(task);
                }
            }

            List<AbilityRuntimeContext> pictures = new();
            foreach(var task in interruptionTasks) {
                runningTasks[abilitID].Remove(task);
                task.OnInterrupted(interuptionContext);
                pictures.Add(task.runtimeContext);
                PoolCenter.Instance.ReleaseInstance(task);
            }

            runningAbilities.Remove(abilitID);

            return new(pictures);
        }

        public void RecoverTask(List<AbilityRuntimeContext> abilityRuntimeContexts) {
            foreach(var context in abilityRuntimeContexts) {
                RecoverTask(context);
            }
        }

        public void RecoverTask(AbilityRuntimeContext abilityRuntimeContext) {
            tasksToRercover.Add(abilityRuntimeContext);
        }
        #endregion

        #region Life Time
        public void Init(AbilityComponentContext abilityComponentContext) {
            if(Inited)
                return;
            foreach(var ability in abilityComponentContext.Abilities) {
                RegistAbility(ability.Value);
            }
            Inited = true;
        }

        public void Update(AbilityComponentContext abilityComponentContext) {
            //���Դ�������legalAbilities�е�Ability
            foreach(var legalAbility in legalAbilities.Values) {
                if(legalAbility.TriggerUnit.TryTrigger(abilityComponentContext) == TaskStatus.Suceeded &&
                    (!runningAbilities.Contains(legalAbility.AbilityHeadInfo.ID) || legalAbility.Stackable)
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

            //��ɱ�֡���ָܻ�
            foreach(var task in tasksToRercover) {
                RegistTask(task,abilityComponentContext);
            }
            tasksToRercover.Clear();

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
                    Debug.LogError($"Exit newTask of Ability: {task.Ability.AbilityHeadInfo.Name} failed!");
                    tasksToRelease.Add(task);
                } else {
                    Debug.LogAssertion($"Unexpected exitStatus: {exitStatus} from newTask of Ablity: {task.Ability.AbilityHeadInfo.Name}");
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
            runtimeContext.BindComponentContext(abilityComponentContext);
            runtimeContext.BindAbility(abilityID);
            runtimeContext.BindAbilityComponent(this);
            runtimeContext.Init();
            newTask.BindRuntimeContext(runtimeContext);
            HashSet<AbilityExcutionTask> taskSet;
            if(runningTasks.ContainsKey(abilityID)) {
                taskSet = runningTasks[abilityID];
            } else {
                taskSet = new();
                runningTasks.Add(abilityID,taskSet);
            }
            taskSet.Add(newTask);
            newTask.OnTriggered(abilityComponentContext);
        }

        private void RegistTask(AbilityRuntimeContext runtimeContext,AbilityComponentContext componentContext) {
            var newTask = PoolCenter.Instance.GetInstance<AbilityExcutionTask>(PoolableObjectTypeCollection.AbilityExcutionTask);
            runtimeContext.BindComponentContext(componentContext);
            newTask.BindRuntimeContext(runtimeContext);
            HashSet<AbilityExcutionTask> taskSet;
            if(runningTasks.ContainsKey(runtimeContext.AbilityID)) {
                taskSet = runningTasks[runtimeContext.AbilityID];
            } else {
                taskSet = new();
                runningTasks.Add(runtimeContext.AbilityID,taskSet);
            }
            taskSet.Add(newTask);
        }
        #endregion

        #region Component Override
        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.AbilityComponent;
        public override void OnAttach(Entity entity) {
            
        }

        public override void Reset(Entity entity) {
            //��������Ӧ�ñ�����
            throw new System.NotImplementedException();
        }

        public override Component Clone() {
            return new AbilityComponent();
        }

        public override void OnDestroy() {
            legalAbilities.Clear();
            runningTasks.Clear();
            runningAbilities.Clear();
            abilitiesToRegist.Clear();
            abilitiesToRemove.Clear();
            abilitiesToCreateTask.Clear();
            tasksToRemove.Clear();
            tasksExiting.Clear();
            tasksToRelease.Clear();
            tasksToRercover.Clear();

            legalAbilities = null;
            runningTasks = null;
            runningAbilities = null;
            abilitiesToRegist = null;
            abilitiesToRemove = null;
            abilitiesToCreateTask = null;
            tasksToRemove = null;
            tasksExiting = null;
            tasksToRelease = null;
            tasksToRercover = null;
            legalAbilities = null;
        }
        #endregion
    }
}