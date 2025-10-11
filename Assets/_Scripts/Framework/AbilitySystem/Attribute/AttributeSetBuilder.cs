using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class AttributeSetBuilder : MonoBehaviour {
        [SerializeField] List<string> attributeNames = new();
        [SerializeField] List<AttributeData> attributeDatas = new();

        public AttributeSet attributeSet { get; private set; }

        private void Awake() {
            if(attributeNames.Count != attributeDatas.Count) {
                Debug.LogError("Size must match");
                return;
            }

            attributeSet = new AttributeSet();
            for(int i = 0; i < attributeDatas.Count; i++) {
                attributeSet.AddAttribute(attributeNames[i],new Attribute(attributeDatas[i]));
            }
        }
    }
}