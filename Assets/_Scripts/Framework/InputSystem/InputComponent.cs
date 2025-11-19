using Drive;
using ECS;
using Lockstep.Math;
using PoolingSystem.ReferencePool;
using RollBackSystem;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Component = ECS.Component;

namespace InputSystemNameSpace {
    public class InputComponent : Component , IRollBackable {
        public int PlayerID { get; private set; }
        public DeQueue<FrameInputData> CachedInputData = new(60);
        private DeQueue<FrameInputData> UnconfirmedInputDataBuffer = new();
        private DeQueue<FrameInputData> InferredInputCache = new();//回滚时使用的根据最新权威输入数据得到的最可能正确的输入缓存
        private static InputComponentSnapShot inputComponentSnapShot = new();//没有数据，只为了节省内存开销

        bool attachUnPredictedInputNeeded = false;
        int unPredictedInput = 0;

        private FrameInputData defaultFrameInputData = new FrameInputData() {
            AimDirection = LVector3.forward,
            KeyCodeinputs = 0
        };

        #region Componenrt Override
        public override ComponentTypeEnum ComponentType => ComponentTypeEnum.InputComponent;
        public override Component GetNewInstance() {
            return new InputComponent() { UnconfirmedInputDataBuffer = this.UnconfirmedInputDataBuffer.Clone() };
        }

        public override void OnAttach(World world,Entity entity) {

        }

        public override void OnDestroy() {
            UnconfirmedInputDataBuffer.Clear();
        }

        public override void Reset(World world,Entity entity) {
            UnconfirmedInputDataBuffer.Clear();
        }
        #endregion

        #region API
        public void LogicUpdate(FrameInputData localPlayerFrameInputData,int authorityLocalLogicFrameCount) {
            FrameInputData predict;
            if(PlayerID == NetworkManager.Instance.LocalPlayerID) {
                predict = localPlayerFrameInputData;
            } else {
                if(CachedInputData.TryPeekBack(out var lastFrameInputData)) {
                    predict = lastFrameInputData.MakePredict(authorityLocalLogicFrameCount);
                } else {
                    //Debug.Log($"[InputComponent]Player: {PlayerID} has no InputData to predict,use defualt instead");
                    defaultFrameInputData.PlayerID = PlayerID;
                    predict = defaultFrameInputData.MakePredict(authorityLocalLogicFrameCount);
                }
            }

            if(attachUnPredictedInputNeeded) {
                predict.KeyCodeinputs |= unPredictedInput;
                attachUnPredictedInputNeeded = false;
            }

            CachedInputData.PushBack(predict);
            UnconfirmedInputDataBuffer.PushBack(predict);

            //Debug.Log($"LocalLogicFrame: {authorityLocalLogicFrameCount},Player: {PlayerID} InputComponent has: {UnconfirmedInputDataBuffer.Count} InputData tobe Conform," +
                //$"Start Frame: {UnconfirmedInputDataBuffer.PeekFront().LocalizedLocalLogicFrameCount} To Frame: {UnconfirmedInputDataBuffer.PeekBack().LocalizedLocalLogicFrameCount}");
        }

        public void BindPlayerID(int playerID) { 
            PlayerID = playerID;
        }

        /// <summary>
        /// 确认预测输入，如有错误将返回false，并返回最早错误帧数，同时重新计算预测帧输入
        /// </summary>
        /// <param name="authoritativeInputData"></param>
        /// <param name="errorStartFrameCount"></param>
        /// <returns></returns>
        public bool IsPredictCorrect(IEnumerable<FrameInputData> authoritiveInputDatas,out int errorStartFrameCount) {
            //Debug.Log($"Player: {PlayerID} Checking PredictState");

            attachUnPredictedInputNeeded = true;
            unPredictedInput = 0;
            foreach(var authoritiveInputData in authoritiveInputDatas) { 
                unPredictedInput |= authoritiveInputData.KeyCodeinputs.GetUnPredictedInput();
            }

            InferredInputCache.Clear();
            int errorFrameCount = -1;
            bool error = false;
            int predictedDataCount = UnconfirmedInputDataBuffer.Count;
            foreach(var authoritiveData in authoritiveInputDatas) {
                if(UnconfirmedInputDataBuffer.TryPeekFront(out var predictData)) {
                    if(predictData.LocalizedLocalLogicFrameCount != authoritiveData.LocalizedLocalLogicFrameCount) { 
                        Debug.LogError($"InputData FrameCount Mismatch Detected,PlayerID: {PlayerID},\nPrediect InputData:{predictData},\nAuthoritive InputData:{authoritiveData}");
                    }
                    if(!predictData.IsRightPredict(authoritiveData)) {
                        Debug.LogWarning($"InCorrect Predict Detected,Prediect InputData:{predictData},Authoritive InputData:{authoritiveData}");
                        if(!error) {
                            errorFrameCount = predictData.LocalizedLocalLogicFrameCount;
                            error = true;
                        }
                    }
                    UnconfirmedInputDataBuffer.PopFront();
                    //这里没有将已确认的输入数据的网络帧号更新为权威网络帧号，因为太几把麻烦了,用到了再说
                    var copy = authoritiveData;
                    copy.KeyCodeinputs |= predictData.KeyCodeinputs.GetUnPredictedInput();//由于权威数据的非预测输入部分会延时模拟，这里需要将之前的权威数据的非预测部份附加到这里，防止回滚导致非预测部分数据丢失
                    InferredInputCache.PushBack(copy);
                } else {
                    Debug.LogError($"There should be inputdata tobe comfirmed,but nothing here,PlayerID: {PlayerID},authoritive input data: {authoritiveData}");
                }
            }

            if(!error) {
                foreach(var localPredictData in UnconfirmedInputDataBuffer) {
                    InferredInputCache.PushBack(localPredictData);
                }
            } else {
                int unConfirmedInputDataCount = UnconfirmedInputDataBuffer.Count;
                UnconfirmedInputDataBuffer.Clear();
                var newestInputData = InferredInputCache.PeekBack();
                for(int i = 0; i < unConfirmedInputDataCount; i++) {
                    var predict = newestInputData.MakePredict(
                        newestInputData.LocalizedLocalLogicFrameCount + i + 1
                    );
                    InferredInputCache.PushBack(predict);
                    UnconfirmedInputDataBuffer.PushBack(predict);
                }
            }
            

            errorStartFrameCount = errorFrameCount;
            if(error)
                Debug.LogWarning($"Player: {PlayerID} Predict State Error Start From Frame: {errorStartFrameCount}");
            return !error;
        }


        public void SimulateInputWhenRollingBackState() {
            var data = InferredInputCache.PopFront();
            // Debug.Log($"[RollingBack,InputSytem]: Use {data} to Rollback");
            CachedInputData.PushBack(data);
        }
        #endregion

        #region IRollBackable
        internal class InputComponentSnapShot : ISnapShot, IReference<InputComponentSnapShot> {
            public int LocalizedLogicFrameCount { get; set; }

            public uint ReferenceType =>ReferenceTypes.INPUTCOMPONENTSNAPSHOT;

            int IReference.IndexInRefrencePool { get ; set ; }

            public void Dispose() {
                
            }

            public IReference GetNewInstance() {
                return new InputComponentSnapShot();
            }

            public void OnRecycle() {
                
            }

            public void Release() {
                
            }
        }

        public ISnapShot SnapShot(int localizedLogicFrameCount) {
            return inputComponentSnapShot;
        }

        public void RollBack(ISnapShot snapShot,int errorStartLocalizedLogicFrameCount,int currentLocalizedLogicFrameCount) {
            // Debug.Log($"Player: {PlayerID} inputComponent Rollback,from: {errorStartLocalizedLogicFrameCount} to: {currentLocalizedLogicFrameCount}");
            int errorFrameCount = currentLocalizedLogicFrameCount - errorStartLocalizedLogicFrameCount + 1;
            while(InferredInputCache.TryPeekFront(out var frameInputData) && frameInputData.LocalizedLocalLogicFrameCount < errorStartLocalizedLogicFrameCount) {
                // Debug.Log($"FrameInputData: {frameInputData} not in error predictRange,Pop");
                InferredInputCache.PopFront();
            }
            CachedInputData.PopBackN(errorFrameCount);
        }
        #endregion
    }
}
