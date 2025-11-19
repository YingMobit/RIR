using NUnit.Framework.Internal;
using PoolingSystem.ReferencePool;
using UnityEngine.UIElements;

namespace RollBackSystem {
    public interface IRollBackable {
        public ISnapShot SnapShot(int localizedLogicFrameCount);

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount);
    }
}