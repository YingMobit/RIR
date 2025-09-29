using System;

namespace ReferencePoolingSystem {
    public interface IReference : IDisposable {
        public uint ReferenceType { get; }
        internal int IndexInRefrencePool { get; set; }
        public void OnRecycle();
        public IReference Clone();
    }

    public interface IReference<TRefrence> : IReference where TRefrence : IReference, new() {
    }
}