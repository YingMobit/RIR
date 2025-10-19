using System;
using UnityEngine;

namespace TagSystem {
    [Serializable]
    public class Tag {
        [SerializeField, InspectorName("Tag")] internal string _tagString;
        internal ushort _tagToken;

        public Tag() {
 
        }
    }
}