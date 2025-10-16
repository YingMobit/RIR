using Sirenix.Utilities;
using System.Collections.Generic;

namespace GAS {
    public class AttributeSet {
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
    }
}