using GAS.Editor.AbilityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace GAS { 
    public class AbilityComponentContextBuilder : MonoBehaviour {
        [SerializeField] List<AbilityGraph> AbilityGraphs;
        [SerializeField] SerializableDictionary<ControllerTypeEnum,IController> ControllerConfig;

        public AbilityComponentContext Context { get; private set; }
        private Dictionary<int,Ability> abilities = new ();
        private BlackBoard globalBlackBoard;
        private AttributeSet attributeSet;

        private void Awake() {
            Ability ability;
            foreach(var config in AbilityGraphs) { 
                ability = config.Build();
                abilities.Add(ability.AbilityHeadInfo.ID, ability);
            }
            globalBlackBoard = PoolCenter.Instance.GetInstance<BlackBoard>(PoolableObjectTypeCollection.BlackBoard);
            attributeSet = GetComponent<AttributeSetBuilder>().attributeSet;
            Context = new(abilities,globalBlackBoard,ControllerConfig.Dictionary,attributeSet);
        }
    }
}