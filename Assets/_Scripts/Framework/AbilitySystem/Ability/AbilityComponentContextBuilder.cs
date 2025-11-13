using GAS.Editor.AbilityEditor;
using PoolingSystem.ReferencePool;
using Sirenix.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    [RequireComponent(typeof(AttributeSetBuilder))]
    public class AbilityComponentContextBuilder : MonoBehaviour {
        [SerializeField] List<AbilityGraph> AbilityGraphs;

        public AbilityComponentContext Context { get; private set; }
        private Dictionary<int,Ability> abilities = new ();
        private Dictionary<ControllerTypeEnum,IController> controllers = new();
        private BlackBoard globalBlackBoard;
        private AttributeSet attributeSet;

        private void Awake() {
            Ability ability;
            foreach(var config in AbilityGraphs) { 
                ability = config.Build();
                abilities.Add(ability.AbilityHeadInfo.ID, ability);
            }
            globalBlackBoard = ReferencePoolingCenter.Instance.GetReference<BlackBoard>();
            attributeSet = GetComponent<AttributeSetBuilder>().attributeSet;
            Context = ReferencePoolingCenter.Instance.GetReference<AbilityComponentContext>();
            Context.BindContext(abilities,globalBlackBoard,controllers,attributeSet);
        }

        public void RegistController(ControllerTypeEnum controllerTypeEnum,IController controller) {
            controllers[controllerTypeEnum] = controller;
        }
    }
}