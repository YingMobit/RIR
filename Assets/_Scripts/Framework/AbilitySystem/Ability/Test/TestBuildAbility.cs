using UnityEngine;
using UnityEditor;
using AbilitySystem.Editor.AbilityEditor;
using System.Collections.Generic;



namespace AbilitySystem {
    public class TestBuildAbility : MonoBehaviour {
        public AbilityGraph graph;
        AbilityComponentContext abilityComponentContext;
        AbilityComponent abilityComponent;

        public void Awake() {
            if (graph == null) {
                Debug.LogError("graph is null");
                return;
            }
            Ability ability = graph.Build();
            if (ability == null) {
                Debug.LogError("Build returned null ability");
                return;
            }
            Debug.Log($"Ability Built. Effects: {ability.Effects?.Count}");
            Dictionary<int,Ability> abilities = new Dictionary<int, Ability>();
            abilities.Add(ability.AbilityHeadInfo.ID, ability);
            abilityComponentContext = new(abilities,PoolCenter.Instance.GetInstance<BlackBoard>(PoolableObjectTypeCollection.BlackBoard),null);
            abilityComponent = new AbilityComponent();
            abilityComponent.RegistAbility(ability);
        }

        private void Update() {
            abilityComponent.Update(abilityComponentContext);
        }

        private void LateUpdate() {
            abilityComponent.LateUpdate(abilityComponentContext);
        }
    }
}
