using GAS.Editor.AbilityEditor;
using PoolingSystem.ReferencePool;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    [RequireComponent(typeof(AttributeSetBuilder))]
    public class AbilityComponentContextBuilder : MonoBehaviour {
        [SerializeField] List<AbilityGraph> AbilityGraphs;

        public AbilityComponentContext Context { get; private set; }
        private Dictionary<int,Ability> abilities = new ();

        private void Awake() {
            Ability ability;
            foreach(var config in AbilityGraphs) { 
                ability = config.Build();
                abilities.Add(ability.AbilityHeadInfo.ID, ability);
            }
            Context = ReferencePoolingCenter.Instance.GetReference<AbilityComponentContext>();
            Context.LoadAbilityConfig(abilities);
        }
    }
}