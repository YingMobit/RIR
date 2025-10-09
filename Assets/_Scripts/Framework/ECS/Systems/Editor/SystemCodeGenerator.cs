using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace ECS {
    public class SystemCodeGenerator {
        [MenuItem("Assets/Create/ECS/System",false,1)]
        public static void GenerateSystemScript() {
            ECSSystemNameInputWindow.ShowWindow((systemName) => {
                if(string.IsNullOrEmpty(systemName))
                    return;

                string className = systemName;
                string folderPath = "Assets/Scripts/Systems";
                if(Selection.activeObject != null) {
                    string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if(AssetDatabase.IsValidFolder(selectedPath)) {
                        folderPath = selectedPath;
                    } else {
                        folderPath = Path.GetDirectoryName(selectedPath).Replace("\\","/");
                    }
                }

                // 创建文件
                string scriptPath = $"{folderPath}/{className}.cs";
                if(!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string script = $@"using ECS;
using UnityEngine;

public class {className} : ISystem {{
    public void OnInit(World world) {{
        // 初始化
    }}

    public void OnFrameUpdate(World world,float deltaTime) {{
        // 每帧更新
    }}

    public void OnFrameLateUpdate(World world) {{
        // 帧末更新
    }}
    
    public void OnNetworkUpdate(World world, int networkFrameCount){{
    
    }}

    public void OnDestroy(World world){{
    }}
}}
";
                File.WriteAllText(scriptPath,script);

                // 更新 SystemTypeCollection
                UpdateSystemCollection(className);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            });
        }

        static void UpdateSystemCollection(string className) {
            string collectionPath = "Assets/_Scripts/Framework/ECS/Systems/SystemTypeCollection.cs";
            if(!File.Exists(collectionPath)) {
                Debug.LogError("SystemTypeCollection.cs not found at path: " + collectionPath);
                return;
            }

            string content = File.ReadAllText(collectionPath);

            // 已存在则直接返回
            if(content.Contains($"typeof({className})"))
                return;

            // 找到 SystemTypes 初始化块的 '{'
            int listPos = content.IndexOf("SystemTypes = new()");
            if(listPos < 0) {
                Debug.LogError("Cannot locate SystemTypes initializer in SystemTypeCollection.cs");
                return;
            }
            int bracePos = content.IndexOf('{',listPos);
            if(bracePos < 0) {
                Debug.LogError("Cannot locate '{' for SystemTypes initializer");
                return;
            }

            // 在大括号后插入 typeof 行，保持缩进格式一致
            string insertLine = $"\n            typeof({className}),";
            content = content.Insert(bracePos + 1,insertLine);

            File.WriteAllText(collectionPath,content);
        }
    }

    public class ECSSystemNameInputWindow : EditorWindow {
        private string systemName = "NewSystem";
        private System.Action<string> onConfirm;

        public static void ShowWindow(System.Action<string> onConfirm) {
            var window = ScriptableObject.CreateInstance<ECSSystemNameInputWindow>();
            window.titleContent = new GUIContent("输入 System 名称");
            window.position = new Rect(Screen.width / 2,Screen.height / 2,360,100);
            window.onConfirm = onConfirm;
            window.ShowUtility();
        }

        void OnGUI() {
            GUILayout.Label("请输入 System 类名（不带后缀）",EditorStyles.boldLabel);
            systemName = EditorGUILayout.TextField("System 名称",systemName);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("确定")) {
                onConfirm?.Invoke(systemName);
                Close();
            }
            if(GUILayout.Button("取消")) {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}