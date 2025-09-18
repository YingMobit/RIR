using UnityEngine;
using UnityEditor;
using AbilitySystem.Editor.AbilityEditor;



namespace AbilitySystem {
    public class TestBuildAbility : MonoBehaviour {
        public AbilityGraph graph;

        [ContextMenu("Build Ability From Graph")]
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
        }
    }
}
