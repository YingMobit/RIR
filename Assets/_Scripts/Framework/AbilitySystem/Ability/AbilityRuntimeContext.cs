using System.Collections.Generic;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using UnityEngine;
using UnityEngine.Pool;


namespace GAS {
    public class AbilityRuntimeContext : IReference<AbilityRuntimeContext>, IRollBackable {
        public bool Interuptable;
        private Dictionary<int,BlackBoard> LocalBlackBoards = new();
        public int AbilityID { get; private set; }
        public AbilityComponentContext AbilityComponentContext { get; private set; }
        public AbilityComponent AbilityComponent { get; private set; }
        public Ability Ability => AbilityComponentContext.Abilities[AbilityID];
        public short currentEffectIndex { get; private set; } = -1;

        public bool MoveNext() {
            if(currentEffectIndex < Ability.Effects.Count - 1) {
                currentEffectIndex++;
                return true;
            }

            return false;
        }
        
        public BlackBoard GetBlackBoard(int runtimeToken) {
            if(LocalBlackBoards.ContainsKey(runtimeToken)) {
                return LocalBlackBoards[runtimeToken];
            }
            var blackBoard = ReferencePoolingCenter.Instance.GetReference<BlackBoard>();
            LocalBlackBoards.Add(runtimeToken,blackBoard);
            return blackBoard;
        }
        
        public void Init() {
            currentEffectIndex = 0;
            Interuptable = true;
        }
        
        public bool BindAbility(int abilityID) {
            if(AbilityComponentContext.Abilities.ContainsKey(abilityID)) {
                AbilityID = abilityID;
                return true;
            }
            return false;
        }
        public void BindComponentContext(AbilityComponentContext abilityComponentContext) {
            AbilityComponentContext = abilityComponentContext;
        }
        public void BindAbilityComponent(AbilityComponent abilityComponent) {
            AbilityComponent = abilityComponent;
        }

        #region IReference

        uint IReference.ReferenceType => ReferenceTypes.ABILITYRUNTIMECONTEXT;

        int IReference.IndexInReferencePool { get; set; }

        public void OnRecycle() {
            AbilityID = -1;
            foreach(var pair in LocalBlackBoards) {
                ReferencePoolingCenter.Instance.ReleaseReference(pair.Value);
            }
            LocalBlackBoards.Clear();
            AbilityComponentContext = null;
            AbilityComponent = null;
            currentEffectIndex = -1;
        }

        public IReference GetNewInstance() {
            return new AbilityRuntimeContext();
        }

        public void Dispose() {
            OnRecycle();
            LocalBlackBoards = null;
        }
        #endregion

        #region IRollback
        internal class AbilityRuntimeContextSnapShot : ISnapShot, IReference<AbilityRuntimeContextSnapShot> {
            public short CurrentEffectIndex;
            public List<int> LocalBlackBoardKeysCopy;
            public List<ISnapShot> LocalBlackBoardSnapShotsCopy;
            public bool InteruptableCopy;
            public int AbilityIDCopy;

            public int LocalizedLogicFrameCount { get; set; }

            public uint ReferenceType => ReferenceTypes.ABILITYRUNTIMECONTEXTSNAPSHOT;

            int IReference.IndexInReferencePool { get; set; }

            public void Dispose() {
                OnRecycle();
                if(LocalBlackBoardKeysCopy != null) {
                    ListPool<int>.Release(LocalBlackBoardKeysCopy);
                    LocalBlackBoardKeysCopy = null;
                }
                if(LocalBlackBoardSnapShotsCopy != null) {
                    ListPool<ISnapShot>.Release(LocalBlackBoardSnapShotsCopy);
                    LocalBlackBoardSnapShotsCopy = null;
                }
            }

            public IReference GetNewInstance() {
                var res = new AbilityRuntimeContextSnapShot();
                res.LocalBlackBoardKeysCopy = ListPool<int>.Get();
                res.LocalBlackBoardSnapShotsCopy = ListPool<ISnapShot>.Get();
                return res;
            }

            public void OnRecycle() {
                if(LocalBlackBoardKeysCopy != null) {
                    LocalBlackBoardKeysCopy.Clear();
                }
                if(LocalBlackBoardSnapShotsCopy != null) {
                    foreach(var snapShot in LocalBlackBoardSnapShotsCopy) {
                        snapShot?.Release();
                    }
                    LocalBlackBoardSnapShotsCopy.Clear();
                }
                CurrentEffectIndex = -1;
                InteruptableCopy = false;
            }

            public void Release() {
                ReferencePoolingCenter.Instance.ReleaseReference(this);
            }
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            var snapShot = ReferencePoolingCenter.Instance.GetReference<AbilityRuntimeContextSnapShot>();
            snapShot.LocalizedLogicFrameCount = localizedLogicFrameCount;
            snapShot.CurrentEffectIndex = currentEffectIndex;
            snapShot.InteruptableCopy = Interuptable;
            snapShot.AbilityIDCopy = AbilityID;
            
            if(snapShot.LocalBlackBoardKeysCopy.Capacity < LocalBlackBoards.Count) {
                snapShot.LocalBlackBoardKeysCopy.Capacity = LocalBlackBoards.Count;
                snapShot.LocalBlackBoardSnapShotsCopy.Capacity = LocalBlackBoards.Count;
            }
            snapShot.LocalBlackBoardKeysCopy.Clear();
            snapShot.LocalBlackBoardSnapShotsCopy.Clear();
            foreach(var pair in LocalBlackBoards) {
                var blackBoardSnapShot = pair.Value.SnapShot(localizedLogicFrameCount);
                snapShot.LocalBlackBoardKeysCopy.Add(pair.Key);
                snapShot.LocalBlackBoardSnapShotsCopy.Add(blackBoardSnapShot);
            }

            return snapShot;
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var runtimeSnapShot = snapShot as AbilityRuntimeContextSnapShot;
            if(runtimeSnapShot == null) {
                Debug.LogError("AbilityRuntimeContext RollBack Error: Invalid SnapShot Type");
                return;
            }

            AbilityID = runtimeSnapShot.AbilityIDCopy;
            currentEffectIndex = runtimeSnapShot.CurrentEffectIndex;
            Interuptable = runtimeSnapShot.InteruptableCopy;
            
            HashSet<int> snapShotKeys = new HashSet<int>(runtimeSnapShot.LocalBlackBoardKeysCopy);
            
            List<int> keysToRemove = new List<int>();
            foreach(var key in LocalBlackBoards.Keys) {
                if(!snapShotKeys.Contains(key)) {
                    keysToRemove.Add(key);
                }
            }
            foreach(var key in keysToRemove) {
                ReferencePoolingCenter.Instance.ReleaseReference(LocalBlackBoards[key]);
                LocalBlackBoards.Remove(key);
            }
            
            for(int i = 0; i < runtimeSnapShot.LocalBlackBoardKeysCopy.Count; i++) {
                int key = runtimeSnapShot.LocalBlackBoardKeysCopy[i];
                ISnapShot blackBoardSnapShot = runtimeSnapShot.LocalBlackBoardSnapShotsCopy[i];

                if(LocalBlackBoards.ContainsKey(key)) {
                    LocalBlackBoards[key].RollBack(blackBoardSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                } else {
                    var blackBoard = ReferencePoolingCenter.Instance.GetReference<BlackBoard>();
                    blackBoard.RollBack(blackBoardSnapShot,errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
                    LocalBlackBoards.Add(key,blackBoard);
                }
            }
        }
        #endregion
    }
}