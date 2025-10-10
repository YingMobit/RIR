using System;
using UnityEngine;

namespace InputSystemNameSpace {
    public class InputQueue {
        private const int DEFAULT_CAPACITY = 180;

        private FrameInputData[] InputCache;
        private int capacity;
        private int headIndex = 0;
        private int size = 0;

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
    }
}
