using ECS;
using GAS;
using InputSystemNameSpace;
using UnityEngine.Pool;
using Component = ECS.Component;

public class AbilitySystem : ISystem {
    public const int INPUTID_IN_GLOBALBLACKBORAD = 0;
    public const int ISFALLINGID_IN_GLOBALBLACKBORAD = 1;
    public const int DELTATIMEID_IN_GLOBALBLACKBORAD = 2;


    public int Order => 1;
    public void OnInit(World world) {
        // 初始化
    }

    public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
        // 每帧更新
        var query = world.Query().With(ComponentTypeEnum.AbilityComponent).With(ComponentTypeEnum.InputComponent).With(ComponentTypeEnum.AttributeComponent).Execute();
        for(int i = 0; i < query.Entities.Count; i++) {
            var inputComponent = query.ComponentSets[i].GetComponent<InputComponent>(ComponentTypeEnum.InputComponent);
            var abilityComponentContextHandler = world.GetGameObject(query.Entities[i]).GetComponent<AbilityComponentContextBuilder>();
            abilityComponentContextHandler.Context.GlobalBlacboard.Set(INPUTID_IN_GLOBALBLACKBORAD,inputComponent.CachedInputData);
            abilityComponentContextHandler.Context.GlobalBlacboard.Set(DELTATIMEID_IN_GLOBALBLACKBORAD,deltaTime);
            var abilityComponent = query.ComponentSets[i].GetComponent<AbilityComponent>(ComponentTypeEnum.AbilityComponent);
            if(!abilityComponent.Inited) {
                abilityComponentContextHandler.Context.Bind(query.ComponentSets[i].GetComponent<AttributeComponent>(ComponentTypeEnum.AttributeComponent).AttributeSet);
                var allComponents = ListPool<Component>.Get();
                world.GetAllComponentsOnEntity(query.Entities[i],allComponents);
                foreach(var comp in allComponents) {
                    if(comp is IController) {
                        abilityComponentContextHandler.Context.RegisterController((comp as IController).Type,comp as IController);
                    }
                }
                ListPool<Component>.Release(allComponents);
                abilityComponent.Init(abilityComponentContextHandler.Context);
            }
            abilityComponent.Update(abilityComponentContextHandler.Context);
        }
    }

    public void OnFrameLateUpdate(World world,int localFrameCount) {
        // 帧末更新
        var entities = ListPool<Entity>.Get();
        var abilityComponents = ListPool<Component>.Get();
        world.GetComponents(ComponentTypeEnum.AbilityComponent,abilityComponents,entities);
        for(int i = 0; i < abilityComponents.Count; i++) {
            var componentContext = world.GetGameObject(entities[i]).GetComponent<AbilityComponentContextBuilder>().Context;
            var abilityComponent = abilityComponents[i] as AbilityComponent;
            abilityComponent.LateUpdate(componentContext);
        }
        ListPool<Entity>.Release(entities);
        ListPool<Component>.Release(abilityComponents);
    }

    public void OnNetworkUpdate(World world,int networkFrameCount) {

    }

    public void OnDestroy(World world) {

    }
}
