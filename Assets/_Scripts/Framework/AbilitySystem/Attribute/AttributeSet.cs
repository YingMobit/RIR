using PoolingSystem.ReferencePool;
using RollBackSystem;
using Sirenix.Utilities;
using System.Collections.Generic;
using Unity.Rendering;
using UnityEngine.Pool;

namespace GAS {
    public class AttributeSet : IReference<AttributeSet> , IRollBackable {
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

        int IReference.IndexInReferencePool { get ; set ; }

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
        
        #region IRollBackable
        internal class AttributeSetSnapShot : ISnapShot, IReference<AttributeSetSnapShot> {
            public int LocalizedLogicFrameCount { get; set; }
            public List<int> attributeIDs;
            public List<ISnapShot> attributeSnapShots;
            #region IReference
            public uint ReferenceType => ReferenceTypes.ATTRIBUTESETSNAPSHOT;
            int IReference.IndexInReferencePool { get; set; }

            public void OnRecycle() {
                foreach(var snapshot in attributeSnapShots) {
                    snapshot.Release();
                }
                attributeIDs.Clear();
                attributeSnapShots.Clear();
            }

            public IReference GetNewInstance() {
                return new AttributeSetSnapShot() {
                    attributeIDs = ListPool<int>.Get(),
                    attributeSnapShots = ListPool<ISnapShot>.Get(),
                };
            }
            
            public void Dispose() {
                OnRecycle();
                ListPool<int>.Release(attributeIDs);
                ListPool<ISnapShot>.Release(attributeSnapShots);
                attributeIDs = null;
                attributeSnapShots = null;
            }

            public void Release() {
                ReferencePoolingCenter.Instance.ReleaseReference(this);
            }
            #endregion
        }
        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            var attributeSetSnapShot = snapShot as AttributeSetSnapShot;
            for(int i=0;i < attributeSetSnapShot.attributeSnapShots.Count; i++) {
                map[attributeSetSnapShot.attributeIDs[i]].RollBack(attributeSetSnapShot.attributeSnapShots[i],errorStartLocalizedLogicFrameCount,currentLocalizedLogicFrameCount);
            }
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            AttributeSetSnapShot snapShot = ReferencePoolingCenter.Instance.GetReference<AttributeSetSnapShot>();
            if(snapShot.attributeIDs.Capacity < map.Values.Count) { 
                snapShot.attributeIDs.Capacity = map.Values.Count;
                snapShot.attributeSnapShots.Capacity = map.Values.Count;
            }
            foreach(var pair in map) {
                snapShot.attributeIDs.Add(pair.Key);
                snapShot.attributeSnapShots.Add(pair.Value.SnapShot(localizedLogicFrameCount));
            }
            return snapShot;
        }
        #endregion
    }
}