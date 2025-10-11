using Sirenix.Utilities;
using System.Collections.Generic;

namespace GAS {
    public class AttributeSet {
        private Dictionary<string,Attribute> map = new();

        public Attribute GetAttribute(string attributeName) { 
            if(map.ContainsKey(attributeName)) { 
                return map[attributeName];
            }
            return null;
        }

        public Attribute this[string name] { 
            get { 
                return GetAttribute(name);
            }
        }

        public void AddAttribute(string attributeName,Attribute attribute) { 
            if(!map.ContainsKey(attributeName)) { 
                map.Add(attributeName,attribute);
            }
        }

        public void RemoveAttribute(string attributeName) { 
            if(map.ContainsKey(attributeName)) { 
                map.Remove(attributeName);
            }
        }
    }
}