using GAS.Editor.AbilityEditor;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    [RequireComponent(typeof(AttributeSetBuilder))]
    public class AbilityComponentContextBuilder : MonoBehaviour {
        [SerializeField] List<AbilityGraph> AbilityGraphs;
        [SerializeField,OdinSerialize] SerializableDictionary<ControllerTypeEnum,Component> ControllerConfig;

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
            var ControllerDict = new Dictionary<ControllerTypeEnum,IController>();
            foreach(var pair in ControllerConfig.Dictionary) { 
                if(pair.Value is IController controller) { 
                    ControllerDict.Add(pair.Key,controller);
                } else { 
                    Debug.LogError($"Controller Config Error: {pair.Key} is not a IController");
                }
            }
            Context = new(abilities,globalBlackBoard,ControllerDict,attributeSet);
        }
    }
}