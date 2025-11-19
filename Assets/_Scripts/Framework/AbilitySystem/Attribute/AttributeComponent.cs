using ECS;
using RollBackSystem;

namespace GAS {
    public class AttributeComponent : Component , IRollBackable {
        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.AttributeComponent;
        public AttributeSet AttributeSet { get; private set; }

        public override Component GetNewInstance() {
            return new AttributeComponent();
        }

        public override void OnAttach(World world,Entity entity) {
            var go = world.GetGameObject(entity);
            AttributeSet = go.GetComponent<AttributeSetBuilder>().attributeSet;
        }

        public override void OnDestroy() {
            AttributeSet = null;
        }

        public override void Reset(World world,Entity entity) {
            AttributeSet = null;            
        }

        #region Rollback
        
        
        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            AttributeSet.RollBack(snapShot, errorStartLocalizedLogicFrameCount, currentLocalizedLogicFrameCount);
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            return AttributeSet.SnapShot(localizedLogicFrameCount);
        }
        #endregion
    }
}