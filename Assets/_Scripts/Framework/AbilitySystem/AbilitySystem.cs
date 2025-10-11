using ECS;
using GAS;
using InputSystemNameSpace;
using System.Collections.Generic;
using UnityEngine.Pool;

public class AbilitySystem : ISystem {
    public const int INPUTID_IN_GLOBALBLACKBORAD = 0;


    public int Order => 1;
    AbilityComponentContextBuilder abilityComponentContextHandler;
    List<Entity> entities;
    List<Component> abilityComponentWithoutInput;
    InputComponent inputComponent;
    AbilityComponentContext componentContext;
    public void OnInit(World world) {
        // 初始化
        abilityComponentWithoutInput = ListPool<Component>.Get();
        entities = ListPool<Entity>.Get();
    }

    public void OnFrameUpdate(World world,int localFrameCount,float deltaTime) {
        // 每帧更新
        var query = world.Query().With(ComponentTypeEnum.AbilityComponent).With(ComponentTypeEnum.InputComponent).Execute();
        for(int i=0;i < query.Entities.Count; i++) {
            inputComponent = query.ComponentSets[i].GetComponent<InputComponent>(ComponentTypeEnum.InputComponent);
            abilityComponentContextHandler = world.GetGameObject(query.Entities[i]).GetComponent<AbilityComponentContextBuilder>();
            abilityComponentContextHandler.Context.GlobalBlacboard.Set<InputQueue>(INPUTID_IN_GLOBALBLACKBORAD,inputComponent.InputQueue);
            query.ComponentSets[i].GetComponent<AbilityComponent>(ComponentTypeEnum.AbilityComponent).Update(abilityComponentContextHandler.Context);
        }

        world.GetComponents(ComponentTypeEnum.AbilityComponent,abilityComponentWithoutInput,entities);
        for(int i=0 ;i < abilityComponentWithoutInput.Count; i++) {
            if(entities[i].HasComponent(ComponentTypeEnum.InputComponent)) {
                continue; 
            }
            componentContext = world.GetGameObject(entities[i]).GetComponent<AbilityComponentContextBuilder>().Context;
            (abilityComponentWithoutInput[i] as AbilityComponent).Update(componentContext);
        }
    }

    public void OnFrameLateUpdate(World world, int localFrameCount) {
        // 帧末更新
    }
    
    public void OnNetworkUpdate(World world, int networkFrameCount){
    
    }

    public void OnDestroy(World world) {
        abilityComponentWithoutInput.Clear();
        ListPool<Component>.Release(abilityComponentWithoutInput);
        entities.Clear();
        ListPool<Entity>.Release(entities);
    }
}
