using InputSystemNameSpace;
using System.Collections.Generic;
using Drive;
using Drive.Serialization;
using UnityEngine;

public class ClientInGameState : ClientState {
    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        // Debug.Log($"Client: {NetworkManager.Instance.LocalPlayerID} In Game!");
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
