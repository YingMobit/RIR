using kcp2k;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Drive {
    public class NetworkClient {
        string IP;
        ushort Port;
        KcpConfig KcpConfig;
        KcpClient kcpClient;
        CancellationTokenSource cts;
        Action<ArraySegment<byte>,KcpChannel> OnClientReceiveData;
        Queue<ArraySegment<byte>> messagesToSend;
        Thread kcpClientKickerThread;

        public void Connect(string ip,ushort port,KcpConfig kcpConfig,Action<ArraySegment<byte>,KcpChannel> onRecivedData) { 
            IP = ip;
            Port = port;
            KcpConfig = kcpConfig;
            OnClientReceiveData = onRecivedData;
            cts = new();
            messagesToSend = new();
            kcpClient = new KcpClient(OnClientConnected,OnClientReceiveData,OnClientDisconnected,OnClientError,KcpConfig);
            kcpClient.Connect(IP, Port);
        }

        public void SendData(ArraySegment<byte> data,KcpChannel kcpChannel) { 
            messagesToSend.Enqueue(data);
        }

        public void StartSendMessage() {
            kcpClientKickerThread = new Thread(() => KcpClientKicker(cts.Token)) {
                IsBackground = true,
                Name = "KcpClientKickerThread"
            };
            kcpClientKickerThread.Start();
        }


        void OnClientConnected() {
            Debug.Log($"Client has connected to server:{{IP: {IP}, Port: {Port}}}");
        }

        void OnClientDisconnected() {
            lock(KcpConfig) {
                StopKcpClientKickThread();
                kcpClient = null;
            }
        }

        void OnClientError(ErrorCode errorCode,string message) {
            Debug.LogError($"Network Error:{{ErrorCode: {errorCode},Message: {message}}}");
        }

        void KcpClientKicker(CancellationToken ct) {
            uint interval = KcpConfig.Interval;
            try {
                while(!ct.IsCancellationRequested) {
                    kcpClient.Tick();

                    while(messagesToSend.TryDequeue(out var message)) {
                        kcpClient.Send(message,KcpChannel.Reliable);
                    }
                    ct.WaitHandle.WaitOne((int)interval);
                }
            } catch(Exception e) {
                Debug.LogException(e);
            } finally { 
                
            }
        }

        void StopKcpClientKickThread() {
            try {
                cts.Cancel();
            } catch(Exception e) {
                Debug.LogException(e);
            }

            try {
                if(kcpClientKickerThread != null && kcpClientKickerThread.IsAlive) {
                    if(!kcpClientKickerThread.Join(200)) {
                        try { kcpClientKickerThread.Interrupt(); } catch { }
                    }
                }
            } finally {
                kcpClientKickerThread = null;
                cts.Dispose();
                cts = null;
            }
        }
    }
}