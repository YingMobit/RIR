using ECS;
using TagSystem;

public class TagComponent : Component {
    private TagSet tagSet = new TagSet();
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.TagComponent;
    public override void OnAttach(Entity entity) { }

    public override void Reset(Entity entity) {
        tagSet.ClearAllTag();
    }

    public override void OnDestroy() {
        tagSet.ClearAllTag();
        tagSet = null;
    }

    public override Component Clone() {
        return new TagComponent();
    }

    public void AddTag(Tag tag) {
        tagSet.AddTag(tag);
    }

    public bool RemoveTag(Tag tag) {
        return tagSet.RemoveTag(tag);
    }

    public bool HasTagExactly(Tag tag) {
        return tagSet.HasTagExactly(tag);
    }

    public bool HasTag(Tag tag) {
        return tagSet.HasTag(tag);
    }

    public void ClearAllTag() {
        tagSet.ClearAllTag();
    }
}
