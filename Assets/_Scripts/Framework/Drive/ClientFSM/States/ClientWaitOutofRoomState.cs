using Drive;
using Drive.Serialization;
using UnityEngine;

public class ClientWaitOutofRoomState : ClientState {
    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        
    }

    public override void UpdateState(ClientFSM clientFSM, ClientFSMContext context) {
        foreach (var message in context.networkMessage) {
            if(message.NetworkMessageType == NetworkMessageType.CommandMessage) {
                var command = ProtobufSerializer.Deserialize<NetworkCommandMessgae>(message.DataStream);
                if(command.CommandID ==  NetworkCommandMessgae.NETWORKCOMMAND_STARTGAME) {
                    Debug.Log("GameStarted Choosing Charactor");
                    clientFSM.SwitchState(ClientEnum.CharactorChoose);
                }
            } else {
                Debug.LogError($"Unsupported message type in this serverstate: {message.NetworkMessageType},current state: {ClientEnum.WaitOutofRoom}");
            }
        }
    }

    public override void ExitState(ClientFSM clientFSM, ClientFSMContext context) {

    }
}

