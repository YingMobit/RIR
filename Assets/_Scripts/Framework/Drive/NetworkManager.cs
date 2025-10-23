using kcp2k;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Drive { 
    public class NetworkManager : Singleton<NetworkManager>  {
        [Header("Network Config")]
        [SerializeField] string serverIP;
        [SerializeField] ushort serverPort;
        [SerializeField] KcpConfig kcpConfig;
        [Header("Driver")]
        [SerializeField] GameObject driverPrefab;

        public Queue<NetworkPlayerInputsMessage> PlayerInputMessgaes { get; private set; }
        public List<NetworkCharactorChooseMessage> PlayerCharactorChooseInfo { get; private set; }
        public int LocalPlayerID { get; set; }

        protected override bool _isDonDestroyOnLoad => true;
        private NetworkClient networkClient;
        private NetworkMessageTransport networkMessageTransport;
        protected override void Awake() {
            base.Awake();
            networkClient = new NetworkClient();
            networkMessageTransport = new NetworkMessageTransport(this);
            PlayerInputMessgaes = new ();
            PlayerCharactorChooseInfo = new ();
            networkClient.Connect(serverIP,serverPort,kcpConfig,OnClientRecivedData);
            networkClient.StartSendMessage();
        }

        public void SendNetworkMessage(NetworkMessage message) { 
            var data = networkMessageTransport.OnSendNetworkMessage(message);
            networkClient.SendData(data,KcpChannel.Reliable);
        }

        private void OnClientRecivedData(ArraySegment<byte> data,KcpChannel kcpChannel) {
            var messageType = networkMessageTransport.GetNetworkMessgaeType(data,kcpChannel);
            if(messageType == NetworkMessageType.PlayerIDAllocationMessage) {
                var message = networkMessageTransport.GetNetworkdMessgae<NetworkPlayerIDAllocationMessage>(data);
                LocalPlayerID = message.PlayerID;
            } else if(messageType == NetworkMessageType.PlayerInputsMessage) {
                var message = networkMessageTransport.GetNetworkdMessgae<NetworkPlayerInputsMessage>(data);
                PlayerInputMessgaes.Enqueue(message);
            } else if(messageType == NetworkMessageType.CommandMessage) {
                var messgae = networkMessageTransport.GetNetworkdMessgae<NetworkCommandMessgae>(data);
                switch(messgae.CommandID) {
                    case NetworkCommandMessgae.NETWORKCOMMAND_STARTGAME:
                        Instantiate(driverPrefab);
                        break;
                }
            } else if(messageType == NetworkMessageType.CharactorChooseMessage) {
                var message = networkMessageTransport.GetNetworkdMessgae<NetworkCharactorChooseMessage>(data);
                for(int i=0;i < PlayerCharactorChooseInfo.Count; i++) {
                    if(PlayerCharactorChooseInfo[i].PlayerID == message.PlayerID) {
                        PlayerCharactorChooseInfo.RemoveAt(i);
                        break;
                    }
                }
                PlayerCharactorChooseInfo.Add(message);
            } else {
                Debug.LogError($"Unknown Network Message Type: {messageType.MessageType}");
            }
        }
    }
}