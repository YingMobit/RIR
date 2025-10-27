using Drive;
using Drive.Serialization;
using System.Collections.Generic;
using UnityEngine;
public class ClientCharactorShooseState : ClientState {

    public override void EnterState(ClientFSM clientFSM, ClientFSMContext context) {
        //模拟发送角色选择消息给服务器，先随便选一个角色ID 1
        NetworkCharactorChooseMessage charactorChooseMessage = new() { CharactorID  = 0,PlayerID = NetworkManager.Instance.LocalPlayerID };
        NetworkManager.Instance.SendNetworkMessage(new NetworkMessage() { NetworkMessageType = NetworkMessageType.CharactorChooseMessage,
                                                                                     DataStream = ProtobufSerializer.Serialize(charactorChooseMessage) });
        NetworkPlayerReadyMessage playerReadyMessage = new() { PlayerID = NetworkManager.Instance.LocalPlayerID };
        NetworkManager.Instance.SendNetworkMessage(new NetworkMessage() { NetworkMessageType = NetworkMessageType.PlayerReadyMessage,
                                                                                     DataStream = ProtobufSerializer.Serialize(playerReadyMessage) });
    }

    public override void UpdateState(ClientFSM clientFSM, ClientFSMContext context) {
        foreach(var message in context.networkMessage) {
            if(message.NetworkMessageType == NetworkMessageType.CommandMessage) {
                var command = ProtobufSerializer.Deserialize<NetworkCommandMessgae>(message.DataStream);
                if(command.CommandID == NetworkCommandMessgae.NETWORKCOMMAND_ALLPLAYERREADY) {
                    clientFSM.SwitchState(ClientEnum.InGame);
                    //通知主线程开始游戏
                    NetworkManager.Instance.StartGameNextFrame = true;
                }
            } else if(message.NetworkMessageType == NetworkMessageType.CharactorChooseMessage) {
                var charactorChooseMessage = ProtobufSerializer.Deserialize<NetworkCharactorChooseMessage>(message.DataStream);
                context.PlayerID_CharactorIDMap[charactorChooseMessage.PlayerID] = charactorChooseMessage.CharactorID;
                // Debug.Log($"Recived CharactorChooseMessage, PlayerID: {charactorChooseMessage.PlayerID}, ChoosedCharactorID: {charactorChooseMessage.CharactorID}");
            } else {
                Debug.LogError($"Unsupported message type in this serverstate: {message.NetworkMessageType},current state: {ClientEnum.WaitOutofRoom}");
            }
        }
    }

    public override void ExitState(ClientFSM clientFSM, ClientFSMContext context) {

    }

}

