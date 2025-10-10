using GAS.Editor.AbilityEditor;
using System.Collections.Generic;
using UnityEngine;

namespace GAS { 
    public class AbilityComponentContextHandler : MonoBehaviour {
        [SerializeField] List<AbilityGraph> AbilityGraphs;
        [SerializeField] SerializableDictionary<ControllerTypeEnum,IController> ControllerConfig;

        public AbilityComponentContext Context { get; private set; }
        private Dictionary<int,Ability> abilities = new ();
        private BlackBoard globalBlackBoard;

        private void Awake() {
            Ability ability;
            foreach(var config in AbilityGraphs) { 
                ability = config.Build();
                abilities.Add(ability.AbilityHeadInfo.ID, ability);
            }
            globalBlackBoard = PoolCenter.Instance.GetInstance<BlackBoard>(PoolableObjectTypeCollection.BlackBoard);
            Context = new(abilities,globalBlackBoard,ControllerConfig.Dictionary);
        }
    }
}