using Drive.Serialization;
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
        public int LocalPlayerID { get; set; }
        protected override bool _isDonDestroyOnLoad => true;

        private List<NetworkMessage> currentFrameNetworkMessages;

        protected override void Awake() {
            base.Awake();
        }
    }
}