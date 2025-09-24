using UnityEditor;
using UnityEngine;
using System.IO;

namespace ECS {
    public class ComponentCodeGenerator {
        [MenuItem("Assets/Create/ECS/Component",false,0)]
        public static void GenerateComponentScript() {
            ECSComponentNameInputWindow.ShowWindow((componentName) => {
                if(string.IsNullOrEmpty(componentName))
                    return;

                string className = componentName;
                string folderPath = "Assets/Scripts/Components";
                if(Selection.activeObject != null) {
                    string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if(AssetDatabase.IsValidFolder(selectedPath)) {
                        folderPath = selectedPath;
                    } else {
                        folderPath = Path.GetDirectoryName(selectedPath).Replace("\\","/");
                    }
                }
                string scriptPath = $"{folderPath}/{className}.cs";

                string script = $@"using ECS;
using UnityEngine;

public class {className} : ECS.Component
{{
    public override ComponentTypeEnum ComponentType => ComponentTypeEnum.{className};
    // 其他字段和方法
}}
";

                File.WriteAllText(scriptPath,script);

                UpdateComponentEnum(className);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            });
        }

        static void UpdateComponentEnum(string className) {
            string enumPath = "Assets/_Scripts/Framework/ECS/Components/ComponentTypeEnum.cs";
            string enumContent = File.ReadAllText(enumPath);

            // 检查是否已存在该枚举成员
            if(enumContent.Contains($"{className} ="))
                return;

            // 找到枚举左括号的位置
            int insertIndex = enumContent.IndexOf('{',enumContent.IndexOf("enum")) + 1;
            string newMember = $"\n        {className} = 1 << {GetNextEnumIndex(enumContent)},";

            // 插入新成员到左括号后
            enumContent = enumContent.Insert(insertIndex,newMember);
            File.WriteAllText(enumPath,enumContent);
        }

        static int GetNextEnumIndex(string enumContent) {
            int maxIndex = -1;
            var lines = enumContent.Split('\n');
            foreach(var line in lines) {
                var trimmed = line.Trim();
                if(trimmed.EndsWith(",")) {
                    var parts = trimmed.Split('=');
                    if(parts.Length == 2) {
                        var valuePart = parts[1].Trim();
                        if(valuePart.StartsWith("1 <<")) {
                            var indexStr = valuePart.Substring(4).TrimEnd(',');
                            if(int.TryParse(indexStr,out int idx)) {
                                if(idx > maxIndex)
                                    maxIndex = idx;
                            }
                        }
                    }
                }
            }
            return maxIndex + 1;
        }
    }

    public class ECSComponentNameInputWindow : EditorWindow {
        private string componentName = "NewComponent";
        private System.Action<string> onConfirm;

        public static void ShowWindow(System.Action<string> onConfirm) {
            var window = ScriptableObject.CreateInstance<ECSComponentNameInputWindow>();
            window.titleContent = new GUIContent("输入组件名");
            window.position = new Rect(Screen.width / 2,Screen.height / 2,300,80);
            window.onConfirm = onConfirm;
            window.ShowUtility();
        }

        void OnGUI() {
            GUILayout.Label("请输入组件名（不带后缀）",EditorStyles.boldLabel);
            componentName = EditorGUILayout.TextField("组件名",componentName);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("确定")) {
                onConfirm?.Invoke(componentName);
                Close();
            }
            if(GUILayout.Button("取消")) {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}
