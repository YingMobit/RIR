using PoolingSystem.ReferencePool;
using Sirenix.Utilities;
using System.Collections.Generic;

namespace GAS {
    public class AttributeSet : IReference<AttributeSet> {
        private Dictionary<int,Attribute> map = new();

        public Attribute GetAttribute(int attributeID) { 
            if(map.ContainsKey(attributeID)) { 
                return map[attributeID];
            }
            return null;
        }

        public Attribute this[int id] { 
            get { 
                return GetAttribute(id);
            }
        }

        public void AddAttribute(int attributeID,Attribute attribute) { 
            if(!map.ContainsKey(attributeID)) { 
                map.Add(attributeID,attribute);
            }
        }

        public void RemoveAttribute(int attributeID) { 
            if(map.ContainsKey(attributeID)) { 
                map.Remove(attributeID);
            }
        }

        #region IReference
        public uint ReferenceType => ReferenceTypes.ATTRIBUTESET;

        int IReference.IndexInRefrencePool { get ; set ; }

        public void OnRecycle() {
            map.Clear();
        }

        public IReference GetNewInstance() {
            return new AttributeSet();
        }

        public void Dispose() {
            OnRecycle();
            map = null;
        }
        #endregion

    }
}