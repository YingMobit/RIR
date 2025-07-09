using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 可序列化的字典实现
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey,TValue>> {
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    private Dictionary<TKey,TValue> dictionary = new Dictionary<TKey,TValue>();

    public Dictionary<TKey,TValue> Dictionary => dictionary;

    public void OnBeforeSerialize() {
        // 不要清空，而是保持Inspector中的修改
        // 只在dictionary有更多元素时才同步
    }

    public void OnAfterDeserialize() {
        dictionary.Clear();
        for(int i = 0; i < Mathf.Min(keys.Count,values.Count); i++) {
            if(keys[i] != null) {
                dictionary[keys[i]] = values[i];
            }
        }
    }

    // 字典操作方法
    public void Add(TKey key,TValue value) => dictionary.Add(key,value);
    public bool Remove(TKey key) => dictionary.Remove(key);
    public bool TryGetValue(TKey key,out TValue value) => dictionary.TryGetValue(key,out value);
    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);
    public TValue this[TKey key] {
        get => dictionary[key];
        set => dictionary[key] = value;
    }
    public int Count => dictionary.Count;
    public Dictionary<TKey,TValue>.KeyCollection Keys => dictionary.Keys;
    public Dictionary<TKey,TValue>.ValueCollection Values => dictionary.Values;

    // 从列表同步到字典（用于Inspector修改后的同步）
    public void SyncFromLists() {
        dictionary.Clear();
        for(int i = 0; i < Mathf.Min(keys.Count,values.Count); i++) {
            if(keys[i] != null && !dictionary.ContainsKey(keys[i])) {
                dictionary[keys[i]] = values[i];
            }
        }
    }

    // 实现 IEnumerable<KeyValuePair<TKey, TValue>>
    public IEnumerator<KeyValuePair<TKey,TValue>> GetEnumerator() {
        return dictionary.GetEnumerator();
    }

    // 实现 IEnumerable
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
public class SerializableDictionaryDrawer : PropertyDrawer {
    // 修改为 static readonly 或者使用属性
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight;
    private static readonly float Spacing = EditorGUIUtility.standardVerticalSpacing;

    // 或者使用属性的方式
    private static float GetLineHeight => EditorGUIUtility.singleLineHeight;
    private static float GetSpacing => EditorGUIUtility.standardVerticalSpacing;

    public override void OnGUI(Rect position,SerializedProperty property,GUIContent label) {
        EditorGUI.BeginProperty(position,label,property);

        // 获取keys和values属性
        var keysProperty = property.FindPropertyRelative("keys");
        var valuesProperty = property.FindPropertyRelative("values");

        // 绘制标题
        var headerRect = new Rect(position.x,position.y,position.width,LineHeight);
        property.isExpanded = EditorGUI.Foldout(headerRect,property.isExpanded,label,true);

        if(property.isExpanded) {
            EditorGUI.indentLevel++;

            // 绘制大小控制
            var sizeRect = new Rect(position.x,position.y + LineHeight + Spacing,position.width,LineHeight);
            int newSize = EditorGUI.IntField(sizeRect,"Size",keysProperty.arraySize);

            if(newSize != keysProperty.arraySize) {
                keysProperty.arraySize = newSize;
                valuesProperty.arraySize = newSize;
            }

            // 记录要删除的索引
            int indexToDelete = -1;

            // 绘制字典元素
            for(int i = 0; i < keysProperty.arraySize; i++) {
                var elementRect = new Rect(
                    position.x,
                    position.y + (i + 2) * (LineHeight + Spacing),
                    position.width,
                    LineHeight
                );

                if(DrawDictionaryElement(elementRect,keysProperty.GetArrayElementAtIndex(i),
                                    valuesProperty.GetArrayElementAtIndex(i),i)) {
                    indexToDelete = i;
                }
            }

            // 在循环外删除元素
            if(indexToDelete >= 0) {
                keysProperty.DeleteArrayElementAtIndex(indexToDelete);
                valuesProperty.DeleteArrayElementAtIndex(indexToDelete);
            }

            // 添加/删除按钮
            var buttonRect = new Rect(
                position.x,
                position.y + (keysProperty.arraySize + 2) * (LineHeight + Spacing),
                position.width,
                LineHeight
            );
            DrawButtons(buttonRect,keysProperty,valuesProperty);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private bool DrawDictionaryElement(Rect rect,SerializedProperty keyProperty,SerializedProperty valueProperty,int index) {
        // 分割矩形：删除按钮 + Key + Value
        var deleteButtonWidth = 20f;
        var keyWidth = (rect.width - deleteButtonWidth) * 0.4f;
        var valueWidth = (rect.width - deleteButtonWidth) * 0.6f;

        var deleteRect = new Rect(rect.x,rect.y,deleteButtonWidth,rect.height);
        var keyRect = new Rect(rect.x + deleteButtonWidth,rect.y,keyWidth,rect.height);
        var valueRect = new Rect(rect.x + deleteButtonWidth + keyWidth,rect.y,valueWidth,rect.height);

        // 删除按钮
        if(GUI.Button(deleteRect,"-")) {
            return true; // 返回true表示需要删除这个元素
        }

        // Key字段
        EditorGUI.PropertyField(keyRect,keyProperty,GUIContent.none);

        // Value字段
        EditorGUI.PropertyField(valueRect,valueProperty,GUIContent.none);

        return false; // 返回false表示不需要删除
    }

    private void DrawButtons(Rect rect,SerializedProperty keysProperty,SerializedProperty valuesProperty) {
        var buttonWidth = rect.width / 2f;
        var addRect = new Rect(rect.x,rect.y,buttonWidth - 2f,rect.height);
        var clearRect = new Rect(rect.x + buttonWidth + 2f,rect.y,buttonWidth - 2f,rect.height);

        if(GUI.Button(addRect,"Add Element")) {
            keysProperty.arraySize++;
            valuesProperty.arraySize++;
        }

        if(GUI.Button(clearRect,"Clear")) {
            keysProperty.ClearArray();
            valuesProperty.ClearArray();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property,GUIContent label) {
        if(!property.isExpanded)
            return LineHeight;

        var keysProperty = property.FindPropertyRelative("keys");
        return (keysProperty.arraySize + 3) * (LineHeight + Spacing) + Spacing;
    }
}
#endif