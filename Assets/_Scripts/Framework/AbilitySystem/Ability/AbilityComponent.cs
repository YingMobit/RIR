using System.Collections.Generic;
using ECS;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;
using UnityEngine.Pool;
using Component = ECS.Component;

namespace GAS {
    public class AbilityComponent : Component, IRollBackable {
        Dictionary<int,Ability> legalAbilities = new();
        Dictionary<int,AbilityExcutionTask> runningTasks = new();
        HashSet<int> runningAbilities = new();

        List<Ability> abilitiesToRegist = new();
        List<int> abilitiesToRemove = new();
        List<int> abilitiesToCreateTask = new();
        List<AbilityExcutionTask> tasksToRemove = new();
        HashSet<AbilityExcutionTask> tasksExiting = new();
        List<AbilityExcutionTask> tasksToRelease = new();
        List<AbilityRuntimeContext> tasksToRecover = new();

        public bool Inited { get; private set; } = false;

        #region API
        public void RegisterAbility(Ability ability) {
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
            foreach(var task in runningTasks.Values) {
                if(task.RuntimeContext.Interuptable && task.CurrentInterruptionPriority < interuptionContext.InteruptionPriority) {
                    interuptedTasks.Add(task);
                }
            }

            foreach(var task in interuptedTasks) {
                runningTasks.Remove(task.Ability.AbilityHeadInfo.ID);
                task.OnInterrupted(interuptionContext);
                runningAbilities.Remove(task.Ability.AbilityHeadInfo.ID);
            }

            List<AbilityRuntimeContext> pictures = new();
            foreach(var task in interuptedTasks) {
                pictures.Add(task.RuntimeContext);
                ReferencePoolingCenter.Instance.ReleaseReference(task);
            }

            return new(pictures);
        }

        public InteruptionHandler InterruptAbility(int abilitID,InteruptionContext interuptionContext) {
            if(!AbilityLegal(abilitID) || !runningAbilities.Contains(abilitID))
                return new(new());
            List<AbilityExcutionTask> interruptionTasks = new();
            var task = runningTasks[abilitID];

            List<AbilityRuntimeContext> pictures = new();
            runningTasks.Remove(abilitID);
            task.OnInterrupted(interuptionContext);
            pictures.Add(task.RuntimeContext);
            ReferencePoolingCenter.Instance.ReleaseReference(task);

            runningAbilities.Remove(abilitID);

            return new(pictures);
        }

        public void RecoverTask(List<AbilityRuntimeContext> abilityRuntimeContexts) {
            foreach(var context in abilityRuntimeContexts) {
                RecoverTask(context);
            }
        }

        public void RecoverTask(AbilityRuntimeContext abilityRuntimeContext) {
            tasksToRecover.Add(abilityRuntimeContext);
        }
        #endregion

        #region Life Time
        public void Init(AbilityComponentContext abilityComponentContext) {
            if(Inited)
                return;
            foreach(var ability in abilityComponentContext.Abilities) {
                RegisterAbility(ability.Value);
            }
            Inited = true;
        }

        public void Update(AbilityComponentContext abilityComponentContext) {
            foreach(var legalAbility in legalAbilities.Values) {
                if(legalAbility.TriggerUnit.TryTrigger(abilityComponentContext) == TaskStatus.Suceeded &&
                    (!runningAbilities.Contains(legalAbility.AbilityHeadInfo.ID))
                    // TODO: && CoolDownSystem.CanUse(legalAbility)
                    ) {
                    Debug.Log($"Ability:{legalAbility.AbilityHeadInfo.Name} Triggered");
                    runningAbilities.Add(legalAbility.AbilityHeadInfo.ID);
                    abilitiesToCreateTask.Add(legalAbility.AbilityHeadInfo.ID);
                }
            }
            
            foreach(var toCreateAbility in abilitiesToCreateTask) {
                RegistTask(toCreateAbility,abilityComponentContext);
            }
            abilitiesToCreateTask.Clear();
            
            TaskStatus taskStatus;
            foreach(var task in runningTasks.Values) {
                taskStatus = task.OnUpdate(abilityComponentContext);
                if(taskStatus.IsFinished()) {
                    Debug.Log($"task:{task.Ability.AbilityHeadInfo.Name} finished with status:{taskStatus}");
                    tasksToRemove.Add(task);
                }
            }
            
            foreach(var task in tasksToRemove) {
                tasksExiting.Add(task);
                runningAbilities.Remove(task.Ability.AbilityHeadInfo.ID);
                runningTasks.Remove(task.Ability.AbilityHeadInfo.ID);
            }
            tasksToRemove.Clear();
        }

        public void LateUpdate(AbilityComponentContext abilityComponentContext) {
            foreach(var ability in abilitiesToRegist) {
                legalAbilities.Add(ability.AbilityHeadInfo.ID,ability);
            }
            abilitiesToRegist.Clear();
            
            foreach(var task in tasksToRecover) {
                RegistTask(task,abilityComponentContext);
            }
            tasksToRecover.Clear();
            
            foreach(var abilityID in abilitiesToRemove) {
                legalAbilities.Remove(abilityID);
                if(runningAbilities.Contains(abilityID)) {
                    var task = runningTasks[abilityID];
                    tasksExiting.Add(task);
                    runningTasks.Remove(abilityID);
                    runningAbilities.Remove(abilityID);
                }
            }
            abilitiesToRemove.Clear();
            
            TaskStatus exitStatus;
            foreach(var task in tasksExiting) {
                exitStatus = task.OnExit(abilityComponentContext);
                if(exitStatus.IsFinished()) {
                    tasksToRelease.Add(task);
                    if(exitStatus == TaskStatus.Failed)
                        Debug.LogError($"Exit newTask of Ability: {task.Ability.AbilityHeadInfo.Name} failed!");
                }
            }

            foreach(var deadTask in tasksToRelease) {
                Debug.Log($"task:{deadTask.Ability.AbilityHeadInfo.Name} dead!");
                tasksExiting.Remove(deadTask);
                ReferencePoolingCenter.Instance.ReleaseReference(deadTask.RuntimeContext);
                ReferencePoolingCenter.Instance.ReleaseReference(deadTask);
            }
            tasksToRelease.Clear();
        }
        #endregion

        #region Tool Function
        private void RegistTask(int abilityID,AbilityComponentContext abilityComponentContext) {
            var newTask = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTask>();
            var runtimeContext = ReferencePoolingCenter.Instance.GetReference<AbilityRuntimeContext>();
            runtimeContext.BindComponentContext(abilityComponentContext);
            runtimeContext.BindAbility(abilityID);
            runtimeContext.BindAbilityComponent(this);
            runtimeContext.Init();
            newTask.BindRuntimeContext(runtimeContext);
            runningTasks[abilityID] = newTask;
            newTask.OnTriggered(abilityComponentContext);
        }

        private void RegistTask(AbilityRuntimeContext runtimeContext,AbilityComponentContext componentContext) {
            var newTask = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTask>();
            runtimeContext.BindComponentContext(componentContext);
            newTask.BindRuntimeContext(runtimeContext);
            runningTasks[runtimeContext.AbilityID] = newTask;
        }
        #endregion

        #region Component Override
        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.AbilityComponent;
        public override void OnAttach(World world,Entity entity) {

        }

        public override void Reset(World world,Entity entity) {
            //��������Ӧ�ñ�����
            throw new System.NotImplementedException();
        }

        public override Component GetNewInstance() {
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
            tasksToRecover.Clear();

            legalAbilities = null;
            runningTasks = null;
            runningAbilities = null;
            abilitiesToRegist = null;
            abilitiesToRemove = null;
            abilitiesToCreateTask = null;
            tasksToRemove = null;
            tasksExiting = null;
            tasksToRelease = null;
            tasksToRecover = null;
            legalAbilities = null;
        }
        #endregion

        #region Rollback
        internal class AbilityComponentSnapShot : ISnapShot, IReference<AbilityComponentSnapShot> {
            public int LocalizedLogicFrameCount { get; set; }
            public bool InitedCopy;

            internal List<int> legalAbilitiesKeysCopy;
            internal List<Ability> legalAbilitiesValuesCopy;

            internal List<int> runningTasksKeysCopy;
            internal List<ISnapShot> runningTasksSnapShotsCopy;

            internal List<int> runningAbilitiesCopy;

            internal List<Ability> abilitiesToRegistCopy;
            internal List<int> abilitiesToRemoveCopy;
            internal List<int> abilitiesToCreateTaskCopy;
            internal List<ISnapShot> tasksToRemoveSnapShotsCopy;
            internal List<ISnapShot> tasksExitingSnapShotsCopy;
            internal List<ISnapShot> tasksToReleaseSnapShotsCopy;
            internal List<ISnapShot> tasksToRecoverSnapShotsCopy;

            #region IReference
            public uint ReferenceType => ReferenceTypes.ABILITYCOMPONENTSNAPSHOT;
            int IReference.IndexInReferencePool { get; set; }

            public void Dispose() {
                OnRecycle();

                // 2. ���б������黹������ز��ÿ�
                if(legalAbilitiesKeysCopy != null) {
                    ListPool<int>.Release(legalAbilitiesKeysCopy);
                    legalAbilitiesKeysCopy = null;
                }
                if(legalAbilitiesValuesCopy != null) {
                    ListPool<Ability>.Release(legalAbilitiesValuesCopy);
                    legalAbilitiesValuesCopy = null;
                }
                if(runningTasksKeysCopy != null) {
                    ListPool<int>.Release(runningTasksKeysCopy);
                    runningTasksKeysCopy = null;
                }
                if(runningTasksSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(runningTasksSnapShotsCopy);
                    runningTasksSnapShotsCopy = null;
                }
                if(runningAbilitiesCopy != null) {
                    ListPool<int>.Release(runningAbilitiesCopy);
                    runningAbilitiesCopy = null;
                }
                if(abilitiesToRegistCopy != null) {
                    ListPool<Ability>.Release(abilitiesToRegistCopy);
                    abilitiesToRegistCopy = null;
                }
                if(abilitiesToRemoveCopy != null) {
                    ListPool<int>.Release(abilitiesToRemoveCopy);
                    abilitiesToRemoveCopy = null;
                }
                if(abilitiesToCreateTaskCopy != null) {
                    ListPool<int>.Release(abilitiesToCreateTaskCopy);
                    abilitiesToCreateTaskCopy = null;
                }
                if(tasksToRemoveSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(tasksToRemoveSnapShotsCopy);
                    tasksToRemoveSnapShotsCopy = null;
                }
                if(tasksExitingSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(tasksExitingSnapShotsCopy);
                    tasksExitingSnapShotsCopy = null;
                }
                if(tasksToReleaseSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(tasksToReleaseSnapShotsCopy);
                    tasksToReleaseSnapShotsCopy = null;
                }
                if(tasksToRecoverSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(tasksToRecoverSnapShotsCopy);
                    tasksToRecoverSnapShotsCopy = null;
                }
            }

            public IReference GetNewInstance() {
                var res = new AbilityComponentSnapShot();
                res.legalAbilitiesKeysCopy = ListPool<int>.Get();
                res.legalAbilitiesValuesCopy = ListPool<Ability>.Get();
                res.runningTasksKeysCopy = ListPool<int>.Get();
                res.runningTasksSnapShotsCopy = ListPool<ISnapShot>.Get();
                res.runningAbilitiesCopy = ListPool<int>.Get();
                res.abilitiesToRegistCopy = ListPool<Ability>.Get();
                res.abilitiesToRemoveCopy = ListPool<int>.Get();
                res.abilitiesToCreateTaskCopy = ListPool<int>.Get();
                res.tasksToRemoveSnapShotsCopy = ListPool<ISnapShot>.Get();
                res.tasksExitingSnapShotsCopy = ListPool<ISnapShot>.Get();
                res.tasksToReleaseSnapShotsCopy = ListPool<ISnapShot>.Get();
                res.tasksToRecoverSnapShotsCopy = ListPool<ISnapShot>.Get();
                return res;
            }

            public void OnRecycle() {
                legalAbilitiesKeysCopy.Clear();
                legalAbilitiesValuesCopy.Clear();
                runningTasksKeysCopy.Clear();

                // �ͷ��б��еĿ���Ԫ��
                foreach(var snapShot in runningTasksSnapShotsCopy) {
                    snapShot?.Release();
                }
                runningTasksSnapShotsCopy.Clear();

                runningAbilitiesCopy.Clear();
                abilitiesToRegistCopy.Clear();
                abilitiesToRemoveCopy.Clear();
                abilitiesToCreateTaskCopy.Clear();

                foreach(var snapShot in tasksToRemoveSnapShotsCopy) {
                    snapShot?.Release();
                }
                tasksToRemoveSnapShotsCopy.Clear();

                foreach(var snapShot in tasksExitingSnapShotsCopy) {
                    snapShot?.Release();
                }
                tasksExitingSnapShotsCopy.Clear();

                foreach(var snapShot in tasksToReleaseSnapShotsCopy) {
                    snapShot?.Release();
                }
                tasksToReleaseSnapShotsCopy.Clear();

                foreach(var snapShot in tasksToRecoverSnapShotsCopy) {
                    snapShot?.Release();
                }
                tasksToRecoverSnapShotsCopy.Clear();

                InitedCopy = false;
            }

            public void Release() {
                ReferencePoolingCenter.Instance.ReleaseReference(this);
            }
            #endregion
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            var snapShot = ReferencePoolingCenter.Instance.GetReference<AbilityComponentSnapShot>();
            snapShot.LocalizedLogicFrameCount = localizedLogicFrameCount;
            snapShot.InitedCopy = Inited;

            // ���� legalAbilities
            if(snapShot.legalAbilitiesKeysCopy.Capacity < legalAbilities.Count) {
                snapShot.legalAbilitiesKeysCopy.Capacity = legalAbilities.Count;
                snapShot.legalAbilitiesValuesCopy.Capacity = legalAbilities.Count;
            }
            snapShot.legalAbilitiesKeysCopy.Clear();
            snapShot.legalAbilitiesValuesCopy.Clear();
            foreach(var pair in legalAbilities) {
                snapShot.legalAbilitiesKeysCopy.Add(pair.Key);
                snapShot.legalAbilitiesValuesCopy.Add(pair.Value);
            }

            if(snapShot.runningTasksKeysCopy.Capacity < runningTasks.Count) {
                snapShot.runningTasksKeysCopy.Capacity = runningTasks.Count;
                snapShot.runningTasksSnapShotsCopy.Capacity = runningTasks.Count;
            }

            snapShot.runningTasksKeysCopy.Clear();
            snapShot.runningTasksSnapShotsCopy.Clear();
            foreach(var pair in runningTasks) {
                if(pair.Value != null) {
                    snapShot.runningTasksKeysCopy.Add(pair.Key);
                    var taskSnapShot = pair.Value.SnapShot(localizedLogicFrameCount);
                    snapShot.runningTasksSnapShotsCopy.Add(taskSnapShot);
                }
            }

            // ���� runningAbilities
            if(snapShot.runningAbilitiesCopy.Capacity < runningAbilities.Count) {
                snapShot.runningAbilitiesCopy.Capacity = runningAbilities.Count;
            }
            snapShot.runningAbilitiesCopy.Clear();
            snapShot.runningAbilitiesCopy.AddRange(runningAbilities);

            // ���մ������б�
            if(snapShot.abilitiesToRegistCopy.Capacity < abilitiesToRegist.Count) {
                snapShot.abilitiesToRegistCopy.Capacity = abilitiesToRegist.Count;
            }
            snapShot.abilitiesToRegistCopy.Clear();
            snapShot.abilitiesToRegistCopy.AddRange(abilitiesToRegist);

            if(snapShot.abilitiesToRemoveCopy.Capacity < abilitiesToRemove.Count) {
                snapShot.abilitiesToRemoveCopy.Capacity = abilitiesToRemove.Count;
            }
            snapShot.abilitiesToRemoveCopy.Clear();
            snapShot.abilitiesToRemoveCopy.AddRange(abilitiesToRemove);

            if(snapShot.abilitiesToCreateTaskCopy.Capacity < abilitiesToCreateTask.Count) {
                snapShot.abilitiesToCreateTaskCopy.Capacity = abilitiesToCreateTask.Count;
            }
            snapShot.abilitiesToCreateTaskCopy.Clear();
            snapShot.abilitiesToCreateTaskCopy.AddRange(abilitiesToCreateTask);

            // ���� tasksToRemove
            if(snapShot.tasksToRemoveSnapShotsCopy.Capacity < tasksToRemove.Count) {
                snapShot.tasksToRemoveSnapShotsCopy.Capacity = tasksToRemove.Count;
            }
            snapShot.tasksToRemoveSnapShotsCopy.Clear();
            foreach(var task in tasksToRemove) {
                if(task != null) {
                    snapShot.tasksToRemoveSnapShotsCopy.Add(task.SnapShot(localizedLogicFrameCount));
                }
            }

            // ���� tasksExiting
            if(snapShot.tasksExitingSnapShotsCopy.Capacity < tasksExiting.Count) {
                snapShot.tasksExitingSnapShotsCopy.Capacity = tasksExiting.Count;
            }
            snapShot.tasksExitingSnapShotsCopy.Clear();
            foreach(var task in tasksExiting) {
                if(task != null) {
                    snapShot.tasksExitingSnapShotsCopy.Add(task.SnapShot(localizedLogicFrameCount));
                }
            }

            // ���� tasksToRelease
            if(snapShot.tasksToReleaseSnapShotsCopy.Capacity < tasksToRelease.Count) {
                snapShot.tasksToReleaseSnapShotsCopy.Capacity = tasksToRelease.Count;
            }
            snapShot.tasksToReleaseSnapShotsCopy.Clear();
            foreach(var task in tasksToRelease) {
                if(task != null) {
                    snapShot.tasksToReleaseSnapShotsCopy.Add(task.SnapShot(localizedLogicFrameCount));
                }
            }

            // ���� tasksToRecover - RuntimeContext�б�
            if(snapShot.tasksToRecoverSnapShotsCopy.Capacity < tasksToRecover.Count) {
                snapShot.tasksToRecoverSnapShotsCopy.Capacity = tasksToRecover.Count;
            }
            snapShot.tasksToRecoverSnapShotsCopy.Clear();
            foreach(var context in tasksToRecover) {
                if(context != null) {
                    snapShot.tasksToRecoverSnapShotsCopy.Add(context.SnapShot(localizedLogicFrameCount));
                }
            }

            return snapShot;
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var componentSnapShot = snapShot as AbilityComponentSnapShot;
            if(componentSnapShot == null) {
                Debug.LogError("AbilityComponent RollBack Error: Invalid SnapShot Type");
                return;
            }

            Inited = componentSnapShot.InitedCopy;

            // �ع� legalAbilities
            legalAbilities.Clear();
            for(int i = 0; i < componentSnapShot.legalAbilitiesKeysCopy.Count; i++) {
                legalAbilities.Add(componentSnapShot.legalAbilitiesKeysCopy[i],componentSnapShot.legalAbilitiesValuesCopy[i]);
            }

            // �ع� runningTasks����ʵ���ܹ��򻯰棩
            // �ռ���ǰ����Task���ڸ���
            var currentTasksByAbilityID = new Dictionary<int,AbilityExcutionTask>();
            foreach(var pair in runningTasks) {
                if(pair.Value != null) {
                    currentTasksByAbilityID[pair.Key] = pair.Value;
                }
            }

            // ��ղ��ؽ� runningTasks
            runningTasks.Clear();
            for(int i = 0; i < componentSnapShot.runningTasksKeysCopy.Count; i++) {
                int abilityID = componentSnapShot.runningTasksKeysCopy[i];
                ISnapShot taskSnapShot = componentSnapShot.runningTasksSnapShotsCopy[i];

                // ���Ը�������Task
                if(currentTasksByAbilityID.TryGetValue(abilityID,out var existingTask)) {
                    existingTask.RollBack(taskSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                    runningTasks[abilityID] = existingTask;
                    currentTasksByAbilityID.Remove(abilityID); // ���Ϊ��ʹ��
                } else {
                    // ������Task
                    var newTask = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTask>();
                    newTask.RollBack(taskSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                    runningTasks[abilityID] = newTask;
                }
            }

            // �ͷ�δʹ�õ�Task
            foreach(var unusedTask in currentTasksByAbilityID.Values) {
                if(unusedTask.RuntimeContext != null) {
                    ReferencePoolingCenter.Instance.ReleaseReference(unusedTask.RuntimeContext);
                }
                ReferencePoolingCenter.Instance.ReleaseReference(unusedTask);
            }

            // �ع� runningAbilities
            runningAbilities.Clear();
            foreach(var abilityID in componentSnapShot.runningAbilitiesCopy) {
                runningAbilities.Add(abilityID);
            }

            // �ع��������б�
            abilitiesToRegist.Clear();
            abilitiesToRegist.AddRange(componentSnapShot.abilitiesToRegistCopy);

            abilitiesToRemove.Clear();
            abilitiesToRemove.AddRange(componentSnapShot.abilitiesToRemoveCopy);

            abilitiesToCreateTask.Clear();
            abilitiesToCreateTask.AddRange(componentSnapShot.abilitiesToCreateTaskCopy);

            // �ع� tasksToRemove
            RollBackTaskList(ref tasksToRemove,componentSnapShot.tasksToRemoveSnapShotsCopy,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);

            // �ع� tasksExiting
            RollBackTaskSet(ref tasksExiting,componentSnapShot.tasksExitingSnapShotsCopy,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);

            // �ع� tasksToRelease
            RollBackTaskList(ref tasksToRelease,componentSnapShot.tasksToReleaseSnapShotsCopy,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);

            // �ع� tasksToRecover
            RollBackRuntimeContextList(ref tasksToRecover,componentSnapShot.tasksToRecoverSnapShotsCopy,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
        }

        // �����������ع�Task�б�
        private void RollBackTaskList(ref List<AbilityExcutionTask> taskList,List<ISnapShot> snapShots,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            // �ռ�����Task���ڸ���
            var availableTasks = new Queue<AbilityExcutionTask>(taskList);

            taskList.Clear();
            foreach(var snapShot in snapShots) {
                AbilityExcutionTask task;
                if(availableTasks.Count > 0) {
                    // ��������Task
                    task = availableTasks.Dequeue();
                    task.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                } else {
                    // ������Task
                    task = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTask>();
                    task.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                }
                taskList.Add(task);
            }

            // �ͷŶ����Task
            while(availableTasks.Count > 0) {
                var unusedTask = availableTasks.Dequeue();
                if(unusedTask.RuntimeContext != null) {
                    ReferencePoolingCenter.Instance.ReleaseReference(unusedTask.RuntimeContext);
                }
                ReferencePoolingCenter.Instance.ReleaseReference(unusedTask);
            }
        }

        // �����������ع�Task����
        private void RollBackTaskSet(ref HashSet<AbilityExcutionTask> taskSet,List<ISnapShot> snapShots,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var availableTasks = new Queue<AbilityExcutionTask>(taskSet);

            taskSet.Clear();
            foreach(var snapShot in snapShots) {
                AbilityExcutionTask task;
                if(availableTasks.Count > 0) {
                    task = availableTasks.Dequeue();
                    task.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                } else {
                    task = ReferencePoolingCenter.Instance.GetReference<AbilityExcutionTask>();
                    task.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                }
                taskSet.Add(task);
            }

            while(availableTasks.Count > 0) {
                var unusedTask = availableTasks.Dequeue();
                if(unusedTask.RuntimeContext != null) {
                    ReferencePoolingCenter.Instance.ReleaseReference(unusedTask.RuntimeContext);
                }
                ReferencePoolingCenter.Instance.ReleaseReference(unusedTask);
            }
        }

        // �����������ع�RuntimeContext�б�
        private void RollBackRuntimeContextList(ref List<AbilityRuntimeContext> contextList,List<ISnapShot> snapShots,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var availableContexts = new Queue<AbilityRuntimeContext>(contextList);

            contextList.Clear();
            foreach(var snapShot in snapShots) {
                AbilityRuntimeContext context;
                if(availableContexts.Count > 0) {
                    context = availableContexts.Dequeue();
                    context.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                } else {
                    context = ReferencePoolingCenter.Instance.GetReference<AbilityRuntimeContext>();
                    context.RollBack(snapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                }
                contextList.Add(context);
            }

            while(availableContexts.Count > 0) {
                var unusedContext = availableContexts.Dequeue();
                ReferencePoolingCenter.Instance.ReleaseReference(unusedContext);
            }
        }
        #endregion
    }
}