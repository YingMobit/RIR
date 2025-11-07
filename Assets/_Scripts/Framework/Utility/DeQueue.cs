using System;
using System.Collections;
using System.Collections.Generic;

namespace Utility {
    /// <summary>
    /// 双端队列（Deque）实现
    /// 支持从头部和尾部进行高效的入队、出队操作
    /// 使用循环数组实现，时间复杂度 O(1)
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class DeQueue<T> : IEnumerable<T> {
        private const int DEFAULT_CAPACITY = 16;
        private const int MAX_CAPACITY = int.MaxValue / 2;
        
        private T[] buffer;
        private int head;      // 头部索引
        private int tail;      // 尾部索引（指向下一个要插入的位置）
        private int count;     // 当前元素数量
        private int capacity;  // 容量
        
        // ==================== 属性 ====================
        
        /// <summary>
        /// 当前元素数量
        /// </summary>
        public int Count => count;
        
        /// <summary>
        /// 当前容量
        /// </summary>
        public int Capacity => capacity;
        
        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => count == 0;
        
        /// <summary>
        /// 是否已满
        /// </summary>
        public bool IsFull => count == capacity;
        
        // ==================== 构造函数 ====================
        
        /// <summary>
        /// 使用默认容量创建双端队列
        /// </summary>
        public DeQueue() : this(DEFAULT_CAPACITY) { }
        
        /// <summary>
        /// 使用指定容量创建双端队列
        /// </summary>
        public DeQueue(int capacity) {
            if(capacity <= 0) {
                throw new ArgumentOutOfRangeException(nameof(capacity), "容量必须大于 0");
            }
            
            this.capacity = capacity;
            buffer = new T[capacity];
            head = 0;
            tail = 0;
            count = 0;
        }
        
        /// <summary>
        /// 从集合创建双端队列
        /// </summary>
        public DeQueue(IEnumerable<T> collection) {
            if(collection == null) {
                throw new ArgumentNullException(nameof(collection));
            }
            
            var list = new List<T>(collection);
            capacity = Math.Max(DEFAULT_CAPACITY, list.Count);
            buffer = new T[capacity];
            head = 0;
            tail = 0;
            count = 0;
            
            foreach(var item in list) {
                PushBack(item);
            }
        }
        
        // ==================== 头部操作 ====================
        
        /// <summary>
        /// 在头部添加元素
        /// </summary>
        public void PushFront(T item) {
            if(IsFull) {
                Resize(capacity * 2);
            }
            
            // 头部索引向前移动
            head = (head - 1 + capacity) % capacity;
            buffer[head] = item;
            count++;
        }
        
        /// <summary>
        /// 从头部移除并返回元素
        /// </summary>
        public T PopFront() {
            if(IsEmpty) {
                throw new InvalidOperationException("队列为空");
            }
            
            T item = buffer[head];
            buffer[head] = default(T);  // 帮助 GC
            head = (head + 1) % capacity;
            count--;
            
            return item;
        }
        
        /// <summary>
        /// 从头部移除 n 个元素
        /// </summary>
        /// <param name="n">要移除的元素数量</param>
        /// <exception cref="ArgumentOutOfRangeException">当 n 小于 0 时</exception>
        /// <exception cref="InvalidOperationException">当 n 大于当前元素数量时</exception>
        public void PopFrontN(int n) {
            if(n < 0) {
                throw new ArgumentOutOfRangeException(nameof(n), "移除数量不能为负数");
            }
            
            if(n > count) {
                throw new InvalidOperationException($"无法从头部移除 {n} 个元素，当前只有 {count} 个元素");
            }
            
            if(n == 0) {
                return;
            }
            
            // 批量移除
            for(int i = 0; i < n; i++) {
                buffer[head] = default(T);  // 帮助 GC
                head = (head + 1) % capacity;
            }
            
            count -= n;
        }
        
        /// <summary>
        /// 尝试从头部移除元素
        /// </summary>
        public bool TryPopFront(out T item) {
            if(IsEmpty) {
                item = default(T);
                return false;
            }
            
            item = PopFront();
            return true;
        }
        
        /// <summary>
        /// 查看头部元素（不移除）
        /// </summary>
        public T PeekFront() {
            if(IsEmpty) {
                throw new InvalidOperationException("队列为空");
            }
            
            return buffer[head];
        }
        
        /// <summary>
        /// 尝试查看头部元素
        /// </summary>
        public bool TryPeekFront(out T item) {
            if(IsEmpty) {
                item = default(T);
                return false;
            }
            
            item = buffer[head];
            return true;
        }
        
        // ==================== 尾部操作 ====================
        
        /// <summary>
        /// 在尾部添加元素
        /// </summary>
        public void PushBack(T item) {
            if(IsFull) {
                Resize(capacity * 2);
            }
            
            buffer[tail] = item;
            tail = (tail + 1) % capacity;
            count++;
        }
        
        /// <summary>
        /// 从尾部移除并返回元素
        /// </summary>
        public T PopBack() {
            if(IsEmpty) {
                throw new InvalidOperationException("队列为空");
            }
            
            // 尾部索引向后移动
            tail = (tail - 1 + capacity) % capacity;
            T item = buffer[tail];
            buffer[tail] = default(T);  // 帮助 GC
            count--;
            
            return item;
        }
        
        /// <summary>
        /// 从尾部移除 n 个元素
        /// </summary>
        /// <param name="n">要移除的元素数量</param>
        /// <exception cref="ArgumentOutOfRangeException">当 n 小于 0 时</exception>
        /// <exception cref="InvalidOperationException">当 n 大于当前元素数量时</exception>
        public void PopBackN(int n) {
            if(n < 0) {
                throw new ArgumentOutOfRangeException(nameof(n), "移除数量不能为负数");
            }
            
            if(n > count) {
                throw new InvalidOperationException($"无法从尾部移除 {n} 个元素，当前只有 {count} 个元素");
            }
            
            if(n == 0) {
                return;
            }
            
            // 批量移除
            for(int i = 0; i < n; i++) {
                tail = (tail - 1 + capacity) % capacity;
                buffer[tail] = default(T);  // 帮助 GC
            }
            
            count -= n;
        }
        
        /// <summary>
        /// 尝试从尾部移除元素
        /// </summary>
        public bool TryPopBack(out T item) {
            if(IsEmpty) {
                item = default(T);
                return false;
            }
            
            item = PopBack();
            return true;
        }
        
        /// <summary>
        /// 查看尾部元素（不移除）
        /// </summary>
        public T PeekBack() {
            if(IsEmpty) {
                throw new InvalidOperationException("队列为空");
            }
            
            int lastIndex = (tail - 1 + capacity) % capacity;
            return buffer[lastIndex];
        }
        
        /// <summary>
        /// 尝试查看尾部元素
        /// </summary>
        public bool TryPeekBack(out T item) {
            if(IsEmpty) {
                item = default(T);
                return false;
            }
            
            int lastIndex = (tail - 1 + capacity) % capacity;
            item = buffer[lastIndex];
            return true;
        }
        
        // ==================== 索引访问 ====================
        
        /// <summary>
        /// 通过索引访问元素（0 = 头部）
        /// </summary>
        public T this[int index] {
            get {
                if(index < 0 || index >= count) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                
                int actualIndex = (head + index) % capacity;
                return buffer[actualIndex];
            }
            set {
                if(index < 0 || index >= count) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                
                int actualIndex = (head + index) % capacity;
                buffer[actualIndex] = value;
            }
        }
        
        // ==================== 批量操作 ====================
        
        /// <summary>
        /// 从头部查看多个元素
        /// </summary>
        public T[] PeekFront(int count) {
            if(count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            
            int validCount = Math.Min(count, this.count);
            T[] result = new T[count];
            
            for(int i = 0; i < validCount; i++) {
                result[i] = this[i];
            }
            
            // 填充默认值
            for(int i = validCount; i < count; i++) {
                result[i] = default(T);
            }
            
            return result;
        }
        
        /// <summary>
        /// 从尾部查看多个元素
        /// </summary>
        public T[] PeekBack(int count) {
            if(count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            
            int validCount = Math.Min(count, this.count);
            T[] result = new T[count];
            
            // 从尾部往前取
            for(int i = 0; i < validCount; i++) {
                result[count - 1 - i] = this[this.count - 1 - i];
            }
            
            // 填充默认值
            for(int i = validCount; i < count; i++) {
                result[count - 1 - i] = default(T);
            }
            
            return result;
        }
        
        /// <summary>
        /// 转换为数组
        /// </summary>
        public T[] ToArray() {
            T[] result = new T[count];
            for(int i = 0; i < count; i++) {
                result[i] = this[i];
            }
            return result;
        }
        
        // ==================== 工具方法 ====================
        
        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear() {
            if(count > 0) {
                // 清空引用，帮助 GC
                Array.Clear(buffer, 0, buffer.Length);
                head = 0;
                tail = 0;
                count = 0;
            }
        }
        
        /// <summary>
        /// 检查是否包含指定元素
        /// </summary>
        public bool Contains(T item) {
            for(int i = 0; i < count; i++) {
                if(EqualityComparer<T>.Default.Equals(this[i], item)) {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 查找元素索引
        /// </summary>
        public int IndexOf(T item) {
            for(int i = 0; i < count; i++) {
                if(EqualityComparer<T>.Default.Equals(this[i], item)) {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// 手动调整容量
        /// </summary>
        public void Resize(int newCapacity) {
            if(newCapacity < count) {
                throw new ArgumentOutOfRangeException(nameof(newCapacity), "新容量不能小于当前元素数量");
            }
            
            if(newCapacity > MAX_CAPACITY) {
                throw new ArgumentOutOfRangeException(nameof(newCapacity), "容量超出最大限制");
            }
            
            if(newCapacity == capacity) {
                return;
            }
            
            T[] newBuffer = new T[newCapacity];
            
            // 复制元素到新数组
            for(int i = 0; i < count; i++) {
                newBuffer[i] = this[i];
            }
            
            buffer = newBuffer;
            capacity = newCapacity;
            head = 0;
            tail = count;
        }
        
        /// <summary>
        /// 收缩容量到当前元素数量
        /// </summary>
        public void TrimExcess() {
            if(count > 0) {
                int newCapacity = Math.Max(DEFAULT_CAPACITY, count);
                if(newCapacity < capacity) {
                    Resize(newCapacity);
                }
            }
        }
        
        /// <summary>
        /// 克隆队列
        /// </summary>
        public DeQueue<T> Clone() {
            var clone = new DeQueue<T>(capacity);
            for(int i = 0; i < count; i++) {
                clone.PushBack(this[i]);
            }
            return clone;
        }
        
        // ==================== IEnumerable 实现 ====================
        
        public IEnumerator<T> GetEnumerator() {
            for(int i = 0; i < count; i++) {
                yield return this[i];
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        // ==================== 调试支持 ====================
        
        public override string ToString() {
            if(IsEmpty) {
                return "DeQueue<T> [Empty]";
            }
            
            var items = new System.Text.StringBuilder();
            items.Append("DeQueue<T> [");
            
            for(int i = 0; i < Math.Min(count, 5); i++) {
                items.Append(this[i]);
                if(i < count - 1 && i < 4) {
                    items.Append(", ");
                }
            }
            
            if(count > 5) {
                items.Append($", ... ({count - 5} more)");
            }
            
            items.Append("]");
            return items.ToString();
        }
        
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo() {
            return $"DeQueue: Count={count}, Capacity={capacity}, Head={head}, Tail={tail}";
        }
    }
}