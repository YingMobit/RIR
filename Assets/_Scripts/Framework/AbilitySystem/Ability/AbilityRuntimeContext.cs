using System.Collections.Generic;
using UnityEngine;


namespace GAS {
    /// <summary>
    /// Ability运行时需要的所有信息，包括AbilityComponentContext和各Unit运行时数据缓存
    /// </summary>
    public class AbilityRuntimeContext : IPoolable {
        public int AbilityID { get; private set; }
        public Ability Ability => AbilityComponentContext.Abilities[AbilityID];
        public AbilityComponentContext AbilityComponentContext { get; private set; }
        public short currentEffectIndex { get; private set; } = -1;
        public bool Interuptable;
        private Dictionary<int,BlackBoard> LocalBlackBoards;

        public bool MoveNext() {
            currentEffectIndex++;
            if(currentEffectIndex == Ability.Effects.Count) {
                return false;
            } else { 
                return true;
            }
        }
        public BlackBoard GetBlackBoard(int runtimeToken) { 
            if(LocalBlackBoards.ContainsKey(runtimeToken)) { 
                return LocalBlackBoards[runtimeToken];
            }
            var blackBoard = PoolCenter.Instance.GetInstance<BlackBoard>(PoolableObjectTypeCollection.BlackBoard);
            LocalBlackBoards.Add(runtimeToken, blackBoard);
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

        #region IPoolable
        public int PoolableType => PoolableObjectTypeCollection.AbilityRuntimeContext;

        public void Dispose() {
            AbilityComponentContext = null;
            LocalBlackBoards = null;
            currentEffectIndex = 0;
            AbilityID = -1;
        }

        public void Reset() {
            LocalBlackBoards = new Dictionary<int, BlackBoard>();
            currentEffectIndex = 0;
            AbilityID = -1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegistePool() {
            PoolCenter.Instance.RegistPool(PoolableObjectTypeCollection.AbilityRuntimeContext,new AbilityRuntimeContextFactory());
        }
        #endregion
    }

    public class AbilityRuntimeContextFactory : IPoolableObjectFactory<AbilityRuntimeContext> {
        public bool CollectionCheck => true;

        public int DefualtCapacity => 10;

        public int MaxCount => 50;

        public AbilityRuntimeContext CreateInstance() {
            return new();
        }

        public void DestroyInstance(AbilityRuntimeContext obj) {
            obj.Dispose();
        }

        public void DisableInstance(AbilityRuntimeContext obj) {
            obj.Reset();
        }

        public void EnableInstance(AbilityRuntimeContext obj) { }
    }
}