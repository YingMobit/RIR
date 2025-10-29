using Drive.Serialization;
using kcp2k;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Utility;

namespace Drive {
    public class NetworkManager : Singleton<NetworkManager> {
        [Header("Network Config")]
        [SerializeField] string serverIP;
        [SerializeField] ushort serverPort;
        [SerializeField] KcpConfig kcpConfig;
        [Header("Driver")]
        [SerializeField] GameObject driverPrefab;
        public int LocalPlayerID { get; private set; }
        public bool IsHost { get; set; }
        public bool StartGameNextFrame = false;
        protected override bool _isDonDestroyOnLoad => true;
        private KcpClient kcpClient;
        private ClientFSM clientFSM;
        private ClientFSMContext clientFSMContext;
        private List<NetworkMessage> networkMessages;
        private Thread kcpClientUpdateThread;
        private bool isConnected = false;
        private Queue<NetworkMessage> messagesToSend = new();

        protected override void Awake() {
            base.Awake();
            kcpClient = new KcpClient(OnKcpClientConnected,OnKcpClientRecivedData,OnKcpClientDisconnected,OnKcpClientError,kcpConfig);
            networkMessages = new List<NetworkMessage>();
            clientFSMContext = new ClientFSMContext(kcpClient,networkMessages);
            clientFSM = new(ClientEnum.WaitOutofRoom,clientFSMContext);
            GlobalEventCenter.Instance.Listen<IRecivedPlayerIDAllocationEventData>(OnRecivedPlayerIDAllocationEventData);
            kcpClient.Connect(serverIP,serverPort);
            kcpClientUpdateThread = new Thread(new ThreadStart(ClientUpdate));
            kcpClientUpdateThread.Name = "KCP Client Update Thread";
            kcpClientUpdateThread.IsBackground = true;
            kcpClientUpdateThread.Start();
        }

        private void ClientUpdate() {
            try {
                while(true) {
                    // KCP Tick ´¦ÀíÍøÂçI/O
                    // Debug.Log("Client Updating");
                    kcpClient.Tick();

                    if(isConnected) {
                        lock(networkMessages) {
                            clientFSM.Update();
                            networkMessages.Clear();
                        }

                        lock(messagesToSend) {
                            while(messagesToSend.Count > 0) {
                                var message = messagesToSend.Dequeue();
                                kcpClient.Send(ProtobufSerializer.Serialize(message),KcpChannel.Reliable);
                            }
                        }
                    }

                    Thread.Sleep((int)kcpConfig.Interval);
                }
            } catch(ThreadAbortException tae) {
                Debug.LogException(tae);
                Debug.Log("[NetworkManager] KCP client update thread aborted");
            } catch(Exception ex) {
                Debug.LogError($"[NetworkManager] Exception in KCP client update thread: {ex}");
            } finally {
                Debug.Log("[NetworkManager] KCP client update thread stopped");
            }
        }

        private void OnKcpClientConnected() {
            isConnected = true;
        }

        private void OnKcpClientRecivedData(ArraySegment<byte> data,KcpChannel kcpChannel) {
            lock(networkMessages) {
                NetworkMessage networkMessage = ProtobufSerializer.GetNetworkMessage(data);
                networkMessages.Add(networkMessage);
            }
        }

        private void OnKcpClientDisconnected() {
            Debug.Log("Server Disconnected");
            if(!kcpClientUpdateThread.Join(1000)) {
                Debug.LogError("[NetworkManager] KCP client update thread did not stop in time");
#if !UNITY_2021_2_OR_NEWER
                kcpUpdateThread.Abort();          
#endif
            }
        }

        private void OnKcpClientError(ErrorCode errorCode,string message) {
            Debug.LogError($"KcpConnection Error,Error Code: {errorCode},Error Messgae: {message}");
        }

        public void SendNetworkMessage(NetworkMessage message) { 
            lock(messagesToSend) {
                messagesToSend.Enqueue(message);
            }
        }

        private void OnRecivedPlayerIDAllocationEventData(IRecivedPlayerIDAllocationEventData eventData) { 
            LocalPlayerID = eventData.PlayerID;
        }

        public void StartGame() { 
            var go = Instantiate(driverPrefab);
            go.GetComponent<Driver>().StartGame(clientFSMContext.PlayerID_CharactorIDMap);
        }

        private void Update() {
            if(StartGameNextFrame) { 
                StartGameNextFrame = false;
                StartGame();
            }
        }

        private void OnDisable() {
            GlobalEventCenter.Instance.CancelListen<IRecivedPlayerIDAllocationEventData>(OnRecivedPlayerIDAllocationEventData);
        }

        private void OnDestroy() {
            if(!kcpClientUpdateThread.Join(1000)) {
                kcpClientUpdateThread.Abort();
            } 
        }
    } 
}