using InputSystemNameSpace;
using System.Collections.Generic;
using Drive;
using Drive.Serialization;
using UnityEngine;

public class ClientInGameState : ClientState {
    List<FrameInputData> playerInputs;
    
    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        playerInputs = new List<FrameInputData>(new FrameInputData[context.PlayerCount]);
        for(int i=0;i < context.PlayerCount; i++) {
            playerInputs.Add(default);
        }
    }

    public override void UpdateState(ClientFSM clientFSM, ClientFSMContext context) {
        foreach(var message in context.networkMessage) {
            if(message.NetworkMessageType == NetworkMessageType.PlayerInputsMessage) { 
                var playerInputsMessage = ProtobufSerializer.Deserialize<NetworkPlayerInputsDownLinkMessage>(message.DataStream);
                GlobalEventCenter.Instance.Invoke<IRecivedNetworkPlayerInputsEventData>(new RecivedNetworkPlayerInputsEventData(playerInputsMessage));   
            }
        }
            
    }

    public override void ExitState(ClientFSM clientFSM, ClientFSMContext context) {
        
    }
}
