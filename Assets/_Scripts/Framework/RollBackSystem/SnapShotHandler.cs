using PoolingSystem.ReferencePool;

namespace RollBackSystem{
    internal class SnapShotHandler : IReference<SnapShotHandler> {
        private IRollBackable rollBackable;
        private ISnapShot snapShot;

        public SnapShotHandler() { }

        public void Bind(IRollBackable rollBackable,ISnapShot snapShot) {
            this.rollBackable = rollBackable;
            this.snapShot = snapShot;
        }

        public void RollBack() {
            rollBackable.Rollback(snapShot);
        }

        #region IReference
        public uint ReferenceType => ReferenceTypes.SNAPSHOTHANDLER;

        int IReference.IndexInRefrencePool { get; set; }

        public void Dispose() {
            rollBackable = null;
            snapShot = null;
        }

        public IReference GetNewInstance() {
            return new SnapShotHandler();
        }

        public void OnRecycle() {
            rollBackable = null;
            snapShot = null;
        }
        #endregion
    }
}