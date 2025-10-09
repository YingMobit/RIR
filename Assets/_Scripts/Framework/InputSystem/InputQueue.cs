using UnityEngine;

namespace InputSystemNameSpace {
    public class InputQueue {
        private FrameInputModel[] InputCache;
        private int capacity;
        private int headIndex = 0;
        private int size = 0;

        public void EnQueue(FrameInputModel frameInputModel) {
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

        public FrameInputModel DeQueue() {
            if(size == 0)
                return FrameInputModel.Null;
            int index = headIndex;
            headIndex++;
            headIndex %= capacity;
            size--;
            return InputCache[index];
        }

        public FrameInputModel PeekHead() {
            if(size == 0)
                return FrameInputModel.Null;
            return InputCache[headIndex];
        }

        public FrameInputModel[] PeekHead(int count) {
            FrameInputModel[] res = new FrameInputModel[count];
            int validCount = count < this.size ? count : this.size;
            for(int i = 0; i < validCount; i++) {
                res[i] = InputCache[(headIndex + i) % capacity];
            }

            for(int i = 0; i < count - validCount; i++) {
                res[validCount + i] = FrameInputModel.Null;
            }
            return res;
        }

        public FrameInputModel PeekTail() {
            if(size == 0)
                return FrameInputModel.Null;
            return InputCache[(headIndex + size - 1) % capacity];
        }

        public FrameInputModel[] PeekTail(int count) {
            FrameInputModel[] res = new FrameInputModel[count];

            int index = 0;
            while(count > size) {
                res[index++] = FrameInputModel.Null;
                count--;
            }

            int startIdnex = (headIndex + size - count) % capacity;
            for(int i = 0; i < count; i++) {
                res[index++] = InputCache[(startIdnex + i) % capacity];
            }
            return res;
        }

        public InputQueue(int capacity) {
            InputCache = new FrameInputModel[capacity];
            this.capacity = capacity;
        }
    }
}
