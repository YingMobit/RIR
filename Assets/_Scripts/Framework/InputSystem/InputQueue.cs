using System;
using UnityEngine;

namespace InputSystemNameSpace {
    public class InputQueue {
        private const int DEFAULT_CAPACITY = 180;

        private FrameInputData[] InputCache;
        private int capacity;
        private int headIndex = 0;
        private int size = 0;

        // 添加公共访问器以支持序列化
        public int Capacity => capacity;
        public int Size => size;
        public int HeadIndex => headIndex;

        public void EnQueue(FrameInputData frameInputModel) {
            int index = -1;
            if(size < capacity) {
                index = (headIndex + size) % capacity;
                size++;
            } else if(size == capacity) {
                index = headIndex;
                headIndex++;
                headIndex %= capacity;
            } else {
                Debug.LogError("Out of capacity");
                return;
            }

            InputCache[index] = frameInputModel;
        }

        public FrameInputData DeQueue() {
            if(size == 0)
                return FrameInputData.Null;
            int index = headIndex;
            headIndex++;
            headIndex %= capacity;
            size--;
            return InputCache[index];
        }

        public FrameInputData PeekHead() {
            if(size == 0)
                return FrameInputData.Null;
            return InputCache[headIndex];
        }

        public FrameInputData[] PeekHead(int count) {
            FrameInputData[] res = new FrameInputData[count];
            int validCount = count < this.size ? count : this.size;
            for(int i = 0; i < validCount; i++) {
                res[i] = InputCache[(headIndex + i) % capacity];
            }

            for(int i = 0; i < count - validCount; i++) {
                res[validCount + i] = FrameInputData.Null;
            }
            return res;
        }

        public FrameInputData PeekTail() {
            if(size == 0)
                return FrameInputData.Null;
            return InputCache[(headIndex + size - 1) % capacity];
        }

        public FrameInputData[] PeekTail(int count) {
            FrameInputData[] res = new FrameInputData[count];

            int index = 0;
            while(count > size) {
                res[index++] = FrameInputData.Null;
                count--;
            }

            int startIdnex = (headIndex + size - count) % capacity;
            for(int i = 0; i < count; i++) {
                res[index++] = InputCache[(startIdnex + i) % capacity];
            }
            return res;
        }

        public InputQueue(int capacity = DEFAULT_CAPACITY) {
            InputCache = new FrameInputData[capacity];
            Array.Fill(InputCache,FrameInputData.Null);
            this.capacity = capacity;
        }

        public InputQueue Clone() { 
            return new InputQueue(this.capacity) { 
                headIndex = this.headIndex,
                size = this.size,
                capacity = this.capacity,
                InputCache = (FrameInputData[])this.InputCache.Clone()
            };
        }

        public void Reset() {
            headIndex = 0;
            size = 0;
        }

        public void OnDestroy() { 
            Array.Clear(InputCache,0,InputCache.Length);
            InputCache = null;
        }

        /// <summary>
        /// 获取所有有效的输入数据（用于序列化）
        /// </summary>
        public FrameInputData[] GetAllValidInputs() {
            FrameInputData[] result = new FrameInputData[size];
            for (int i = 0; i < size; i++) {
                result[i] = InputCache[(headIndex + i) % capacity];
            }
            return result;
        }

        /// <summary>
        /// 从数组批量加载输入（用于反序列化）
        /// </summary>
        public void LoadFromArray(FrameInputData[] inputs) {
            Reset();
            if (inputs == null) return;
            
            foreach (var input in inputs) {
                if (input.NetworkFrameCount >= 0) { // 过滤无效数据
                    EnQueue(input);
                }
            }
        }
    }
}
