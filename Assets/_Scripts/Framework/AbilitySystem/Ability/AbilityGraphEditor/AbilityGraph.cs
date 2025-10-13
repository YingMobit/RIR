using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

namespace GAS.Editor.AbilityEditor {

    [CreateAssetMenu(menuName = "GAS/Ability Graph",fileName = "NewAbilityGraph")]
    [RequireNode(typeof(AbilityNode))]
    public class AbilityGraph : NodeGraph {
        private AbilityNode _root;
        public AbilityNode Root => _root ??= GetRoot();
        public AbilityNode GetRoot() {
            // 缓存失效或未赋值时，从 nodes 中查找
            if(_root == null) {
                _root = nodes?.OfType<AbilityNode>().FirstOrDefault();
            }
            return _root;
        }

#if UNITY_EDITOR
        // 资源变动/反序列化后确保缓存刷新
        private void OnValidate() {
            if(_root == null)
                _root = nodes?.OfType<AbilityNode>().FirstOrDefault();
        }
        private void OnEnable() {
            if(_root == null)
                _root = nodes?.OfType<AbilityNode>().FirstOrDefault();
        }
#endif
        public Ability Build() {
            Ability res = Root.Build();
            return res;
        }
    }
}
