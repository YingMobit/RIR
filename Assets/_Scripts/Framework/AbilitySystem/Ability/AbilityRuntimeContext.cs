using PoolingSystem.ReferencePool;
using System.Collections.Generic;
using UnityEngine;


namespace GAS {
    /// <summary>
    /// Ability运行时需要的所有信息，包括AbilityComponentContext和各Unit运行时数据缓存
    /// </summary>
    public class AbilityRuntimeContext : IReference<AbilityRuntimeContext> {
        public bool Interuptable;
        private Dictionary<int,BlackBoard> LocalBlackBoards = new();
        public int AbilityID { get; private set; }
        public AbilityComponentContext AbilityComponentContext { get; private set; }
        public AbilityComponent AbilityComponent { get; private set; }
        public Ability Ability => AbilityComponentContext.Abilities[AbilityID];
        public short currentEffectIndex { get; private set; } = -1;

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
            var blackBoard = ReferencePoolingCenter.Instance.GetReference<BlackBoard>();
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
        public void BindAbilityComponent(AbilityComponent abilityComponent) {
            AbilityComponent = abilityComponent;
        }

        #region IPoolable

        uint IReference.ReferenceType => ReferenceTypes.ABILITYRUNTIMECONTEXT;

        int IReference.IndexInRefrencePool { get; set; }

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
    }
}