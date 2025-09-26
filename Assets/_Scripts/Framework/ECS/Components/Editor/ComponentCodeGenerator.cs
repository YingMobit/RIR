using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

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
    public override void OnAttach(Entity entity) {{
        // 初始化组件
    }}
    public override void Reset(Entity entity) {{
        // 重置组件状态
    }}
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
            if(!File.Exists(enumPath)) {
                Debug.LogError("ComponentTypeEnum.cs not found at path: " + enumPath);
                return;
            }
            string content = File.ReadAllText(enumPath);

            // 如果已经有该成员直接返回
            if(content.Contains($" {className} =") || content.Contains($"\n{className} ="))
                return;

            // 1. 解析现有枚举项 (Name = 1 << n,)
            var entryRegex = new Regex(@"^\s*(\w+)\s*=\s*1\s*<<\s*(\d+)\s*,", RegexOptions.Multiline);
            var matches = entryRegex.Matches(content);
            var entries = new List<(string name,int index)>();
            foreach(Match m in matches) {
                string name = m.Groups[1].Value;
                if(name == "None") continue; // 忽略 None 或其它保留
                int idx = int.Parse(m.Groups[2].Value);
                entries.Add((name,idx));
            }

            int nextIndex = entries.Count == 0 ? 0 : entries.Max(e => e.index) + 1;
            entries.Add((className,nextIndex));
            entries = entries.OrderBy(e => e.index).ToList();

            // 2. 重新生成枚举块（仅替换 AUTO 区域或插入到 enum 花括号后）
            // 找到 enum ComponentTypeEnum 的 '{'
            int enumDecl = content.IndexOf("enum ComponentTypeEnum");
            if(enumDecl < 0) { Debug.LogError("Cannot locate enum ComponentTypeEnum"); return; }
            int enumOpen = content.IndexOf('{', enumDecl);
            if(enumOpen < 0) { Debug.LogError("Cannot locate '{' for ComponentTypeEnum"); return; }
            int enumClose = content.IndexOf('}', enumOpen+1); // 只到第一个关闭，后面是 extension class
            if(enumClose < 0) { Debug.LogError("Cannot locate '}' for ComponentTypeEnum"); return; }

            // 枚举体原始文本
            string enumBody = content.Substring(enumOpen+1, enumClose - enumOpen -1);
            // 移除旧的 1 << n 行
            enumBody = entryRegex.Replace(enumBody, string.Empty);
            // 清理多余空行
            enumBody = Regex.Replace(enumBody, "\n{2,}", "\n");

            // 新条目文本
            string indent = "        ";
            var enumLines = entries.Select(e => $"{indent}{e.name} = 1 << {e.index},");
            string newEnumBody = "\n" + string.Join("\n", enumLines) + "\n";
            content = content.Remove(enumOpen+1, enumClose - enumOpen -1)
                             .Insert(enumOpen+1, newEnumBody);

            // 3. 更新 COMPONENT_TYPE_COUNT 行
            int newCount = entries.Count; // 不含 None
            var countRegex = new Regex(@"public const int COMPONENT_TYPE_COUNT = \d+;");
            if(countRegex.IsMatch(content)) {
                content = countRegex.Replace(content, $"public const int COMPONENT_TYPE_COUNT = {newCount};");
            }

            // 4. 更新 COMPONENT_TYPE_MAPPING 数组内容
            var mapRegex = new Regex(@"public static readonly Type\[] COMPONENT_TYPE_MAPPING = new Type\[.*?\]\s*{(.*?)};", RegexOptions.Singleline);
            content = mapRegex.Replace(content, m => {
                string mappingLines = string.Join("\n", entries.Select(e => $"            typeof({e.name}), // index {e.index}"));
                return $"public static readonly Type[] COMPONENT_TYPE_MAPPING = new Type[COMPONENT_TYPE_COUNT]{{\n{mappingLines}\n        }};";
            });

            File.WriteAllText(enumPath, content);
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
