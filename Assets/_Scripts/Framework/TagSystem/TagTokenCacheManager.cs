using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TagSystem {
    internal static class TagTokenCacheManager {
        // 文件路径常量 -> Resources 下
        private const string FileName = "tag_tokens.json";
        private static readonly string FilePath = Path.Combine(Application.dataPath, "Resources", FileName);
        private static readonly string ResourcesLoadKey = Path.GetFileNameWithoutExtension(FileName);

        private static readonly object _lock = new object();
        private static Dictionary<string, uint> _cache = new Dictionary<string, uint>(StringComparer.Ordinal);
        private static uint _nextId = 1; // token 从 1 开始，0 为无效

        [Serializable]
        private class Model {
            public uint nextId;
            public List<string> keys = new List<string>();
            public List<uint> values = new List<uint>();
        }

        public static void Load() {
            lock (_lock) {
                _cache.Clear();
                _nextId = 1;
                try {
                    var ta = Resources.Load<TextAsset>(ResourcesLoadKey);
                    if (ta == null) return;
                    var json = ta.text;
                    if (string.IsNullOrWhiteSpace(json)) return;
                    var model = JsonUtility.FromJson<Model>(json);
                    if (model == null) return;
                    int n = Math.Min(model.keys?.Count ?? 0, model.values?.Count ?? 0);
                    for (int i = 0; i < n; i++) {
                        var k = model.keys[i];
                        var v = model.values[i];
                        if (!string.IsNullOrEmpty(k)) _cache[k] = v;
                    }
                    if (model.nextId > 0) _nextId = model.nextId;
                    else {
                        if (_cache.Count > 0) _nextId = (uint)(_cache.Values.Max() + 1);
                    }
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to load tag token cache from Resources: {e.Message}");
                    _cache.Clear();
                    _nextId = 1;
                }
            }
        }

        public static bool TryGetToken(string tagString, out uint token) {
            if (string.IsNullOrEmpty(tagString)) { token = 0; return false; }
            lock (_lock) {
                return _cache.TryGetValue(tagString, out token);
            }
        }

        public static uint GetOrAddToken(string tagString) {
            if (string.IsNullOrEmpty(tagString)) return 0;
            lock (_lock) {
                if (_cache.TryGetValue(tagString, out var existing)) return existing;
                uint token = _nextId++;
                _cache[tagString] = token;
                // persist
                try {
                    SaveUnlocked();
                }
                catch (Exception e) {
                    Debug.LogError($"Failed to save tag token cache: {e.Message}");
                }
                return token;
            }
        }

        public static IReadOnlyDictionary<string, uint> GetAllMappings() {
            lock (_lock) {
                return new Dictionary<string, uint>(_cache);
            }
        }

        private static void SaveUnlocked() {
            // caller must hold lock
            var model = new Model();
            model.nextId = _nextId;
            model.keys = _cache.Keys.ToList();
            model.values = _cache.Values.ToList();
            var json = JsonUtility.ToJson(model);
            var tmp = FilePath + ".tmp";
            // ensure directory
            var dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(tmp, json);
            // atomic replace
            try {
                if (File.Exists(FilePath)) {
                    File.Replace(tmp, FilePath, null);
                }
                else {
                    File.Move(tmp, FilePath);
                }
            }
            catch (PlatformNotSupportedException) {
                // fallback
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tmp, FilePath);
            }
        }
    }
}