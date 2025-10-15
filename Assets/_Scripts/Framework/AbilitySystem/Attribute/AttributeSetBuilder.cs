using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class AttributeSetBuilder : MonoBehaviour {
        [SerializeField] List<string> attributeNames = new();
        [SerializeField] List<AttributeData> attributeDatas = new();

        private AttributeSet _attributeSet;
        public AttributeSet attributeSet { get {
                if(_attributeSet == null) { 
                    _attributeSet = new AttributeSet();
                    if(attributeNames.Count != attributeDatas.Count) {
                        Debug.LogError("Size must match");
                        return null;
                    }
                    for(int i = 0; i < attributeDatas.Count; i++) {
                        attributeSet.AddAttribute(attributeNames[i],new Attribute(attributeDatas[i]));
                    }
                }   
                return _attributeSet;
            } 
        }
    }
}