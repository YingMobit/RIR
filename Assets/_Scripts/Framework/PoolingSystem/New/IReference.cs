using System;

namespace ReferencePoolingSystem { 
    public interface IReference : IDisposable {
        public void OnRecycle();
    }

    public interface IReference<TRefrence> : IReference where TRefrence : IReference , new() {
        
    }
}