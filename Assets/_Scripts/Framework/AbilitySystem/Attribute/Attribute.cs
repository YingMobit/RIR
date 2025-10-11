using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GAS {
    [Serializable]
    public class Attribute {
        AttributeData attributeData;
        public float BaseValue => attributeData.BaseValue;
        public float MaxValue => attributeData.MaxValue;
        public float MinValue => attributeData.MinValue;

        public event Action<float,float> OnValueChanged;
        public event Action<float,float> OnMaxValueChanged;
        public event Action<float,float> OnMinValueChanged;

        public int Int() { 
            return (int)BaseValue;
        }

        public float Float() { 
            return (float)BaseValue;
        }

        public bool Bool() {
            return BaseValue != 0;
        }

        public void SetBaseValue(float newValue,bool invokeEvent = true) { 
            newValue = Math.Clamp(newValue,MinValue,MaxValue);
            if(newValue != BaseValue) { 
                float oldValue = BaseValue;
                attributeData.BaseValue = newValue;
                if(invokeEvent) { 
                    OnValueChanged?.Invoke(oldValue,BaseValue);
                }
            }
        }

        public void SetMaxValue(float newValue,bool invokeEvent = true) {
            if(newValue < MinValue) {
                Debug.LogError("MaxValue must bigger than MinValue");
                return;
            }
            
            if(newValue != MaxValue) {
                float oldvalue = MaxValue;
                attributeData.MaxValue = newValue;
                if(BaseValue > MaxValue) {
                    attributeData.BaseValue = MaxValue;
                    if(invokeEvent) {
                        OnValueChanged?.Invoke(oldvalue,MaxValue);
                    }
                }
            
                if(invokeEvent) {
                    OnMaxValueChanged?.Invoke(MaxValue,newValue);
                }
            }
        }

        public void SetMinValue(float newValue,bool invokeEvent = true) {
            if(newValue > MaxValue) {
                Debug.LogError("MinValue must smaller than MaxValue");
            }

            if(newValue != MinValue) { 
                float oldValue = MinValue;
                attributeData.MinValue = newValue;
                if(BaseValue < MinValue) { 
                    attributeData.BaseValue = MinValue;
                    if(invokeEvent) { 
                        OnValueChanged?.Invoke(BaseValue,MinValue);
                    }
                }

                if(invokeEvent) { 
                    OnMinValueChanged?.Invoke(MinValue,newValue);
                }
            }
        }

        public Attribute(AttributeData data) { 
            attributeData = data;
        }
    }

    [Serializable]
    public struct AttributeData { 
        public float BaseValue;
        public float MaxValue;
        public float MinValue;
    }
}