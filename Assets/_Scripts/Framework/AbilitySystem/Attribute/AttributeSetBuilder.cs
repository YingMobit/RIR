using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAS {
    public class AttributeSetBuilder : MonoBehaviour {
        [SerializeField] List<AttributeHeadInfo> attributeHeadInfos = new();
        [SerializeField] List<AttributeData> attributeDatas = new();

        private AttributeSet _attributeSet;
        public AttributeSet attributeSet { get {
                if(_attributeSet == null) { 
                    _attributeSet = new AttributeSet();
                    if(attributeHeadInfos.Count != attributeDatas.Count) {
                        Debug.LogError("Size must match");
                        return null;
                    }
                    for(int i = 0; i < attributeDatas.Count; i++) {
                        attributeSet.AddAttribute(attributeHeadInfos[i].ID,new Attribute(attributeDatas[i]));
                    }
                }   
                return _attributeSet;
            } 
        }
    }

    [Serializable]
    internal struct AttributeHeadInfo {
        public int ID;
        public string Name;
    }
}