using PoolingSystem.ReferencePool;
using RollBackSystem;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GAS {
    [Serializable]
    public class Attribute : IReference<Attribute>, IRollBackable {
        AttributeData attributeData;
        public float BaseValue => attributeData.BaseValue;
        public float MaxValue => attributeData.MaxValue;
        public float MinValue => attributeData.MinValue;

        public event Action<float,float> OnValueChanged;
        public event Action<float,float> OnMaxValueChanged;
        public event Action<float,float> OnMinValueChanged;
        public Attribute() { }
        public Attribute(AttributeData data) {
            attributeData = data;
        }

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


        #region IReference
        public uint ReferenceType => ReferenceTypes.ATTRIBUTE;
        int IReference.IndexInReferencePool { get; set; }

        public void OnRecycle() {
            attributeData.BaseValue = 0;
            attributeData.MaxValue = 0;
            attributeData.MinValue = 0;
            OnValueChanged = null;
            OnMaxValueChanged = null;
            OnMinValueChanged = null;
        }

        public IReference GetNewInstance() {
            return new Attribute(new AttributeData());
        }

        public void Dispose() {
            OnRecycle();
        }
        #endregion

        #region IRollBackable
        internal class AttributeSnapShot : ISnapShot, IReference<AttributeSnapShot> {
            public int LocalizedLogicFrameCount { get; set; }

            public uint ReferenceType => ReferenceTypes.ATTRIBUTESNAPSHOT;

            int IReference.IndexInReferencePool { get ; set ; }

            public AttributeData attributeData;
            public void OnRecycle() {
                
            }

            public IReference GetNewInstance() {
                return new AttributeSnapShot();
            }

            public void Dispose() {
                OnRecycle();
            }

            public void Release() {
                ReferencePoolingCenter.Instance.ReleaseReference(this);
            }
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            attributeData = (snapShot as AttributeSnapShot).attributeData;
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            AttributeSnapShot attributeSnapShot = ReferencePoolingCenter.Instance.GetReference<AttributeSnapShot>();
            attributeSnapShot.attributeData = attributeData;
            return attributeSnapShot;
        }
        #endregion

    }


    [Serializable]
    public struct AttributeData { 
        public float BaseValue;
        public float MaxValue;
        public float MinValue;
    }
}