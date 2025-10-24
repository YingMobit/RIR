using Drive;
using Drive.Serialization;
using UnityEngine;
public class ClientCharactorShooseState : ClientState {
    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        
    }

    public override void UpdateState(ClientFSM clientFSM, ClientFSMContext context) {
        foreach(var message in context.networkMessage) {
            if(message.NetworkMessageType == NetworkMessageType.CommandMessage) {
                var command = ProtobufSerializer.Deserialize<NetworkCommandMessgae>(message.DataStream);
                if(command.CommandID == NetworkCommandMessgae.NETWORKCOMMAND_ALLPLAYERREADY) {
                    clientFSM.SwitchState(ClientEnum.InGame);
                }
            } else if(message.NetworkMessageType == NetworkMessageType.CharactorChooseMessage) {
                //转发角色选择消息,这里先不做处理，因为角色选择功能还没做完
                var charactorChooseMessage = ProtobufSerializer.Deserialize<NetworkCharactorChooseMessage>(message.DataStream);
                Debug.Log($"Recived CharactorChooseMessage, PlayerID: {charactorChooseMessage.PlayerID}, ChoosedCharactorID: {charactorChooseMessage.CharactorID}");
            } else {
                Debug.LogError($"Unsupported message type in this serverstate: {message.NetworkMessageType},current state: {ClientEnum.WaitOutofRoom}");
            }
        }
    }

    public override void ExitState(ClientFSM clientFSM, ClientFSMContext context) {

    }

}

