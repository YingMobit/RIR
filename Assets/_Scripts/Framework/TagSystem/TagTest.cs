using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TagSystem {
    // 简单运行时测试组件，用于放到场景中或添加到 GameObject 上运行
    public class TagTest : MonoBehaviour {
        [SerializeField] Tag t1;
        [SerializeField] Tag t2;
        [SerializeField] Tag t3;
        [SerializeField] Tag t4;
        [SerializeField] Tag t5;

        TagSet set=new();
        void Start() {
            Debug.Log("TagTest started");

            Debug.Log($"t1 token: {t1.tagToken}");
            Debug.Log($"t2 token: {t2.tagToken}");
            Debug.Log($"t3 token: {t3.tagToken}");
            Debug.Log($"t4 token: {t4.tagToken}");
            Debug.Log($"t5 token: {t5.tagToken}");

            Debug.Log($"t3 DerivedFrom t2: {t3.DerivedFrom(t2)} (expect true)");
            Debug.Log($"t3 DerivedFrom t1: {t3.DerivedFrom(t1)} (expect true)");
            Debug.Log($"t2 DerivedFrom t3: {t2.DerivedFrom(t3)} (expect false)");

            set.AddTag(t3); // add Melee
            set.AddTag(t3); // add Melee
            Debug.Log($"HasTagExactly Melee: {  set.HasTagExactly(t3)}  (expect true)");
            Debug.Log($"HasTag Cooldown: {      set.HasTag(t2)}         (expect true)");
            Debug.Log($"HasTag AbilitySystem: { set.HasTag(t1)}         (expect true)");

            set.RemoveTag(t3);
            Debug.Log($"After remove, HasTagExactly Melee: {set.HasTagExactly(t3)   } (expect false)");
            Debug.Log($"After remove, HasTag Cooldown: {    set.HasTag(t2)          } (expect false)");
            set.ClearAllTag();
        }

        private void Update() {
            t3.DerivedFrom(t2);
            t3.DerivedFrom(t1);
            t2.DerivedFrom(t3);
            var set = new TagSet();
            set.AddTag(t3);
            set.HasTagExactly(t3);
            set.HasTag(t2);
            set.HasTag(t1);
            set.RemoveTag(t3);
            set.HasTagExactly(t3);
            set.HasTag(t2);
            set.ClearAllTag();
        }
    }
}
