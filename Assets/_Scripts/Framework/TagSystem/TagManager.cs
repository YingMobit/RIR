using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TagSystem {
    public static class TagManager {
        private static Dictionary<string, uint> tagStringToToken = new Dictionary<string, uint>(StringComparer.Ordinal);
        // �ϲ��Ĳ㼶����
        private static Node tagHierarchyRoot = new Node(string.Empty, 0);
        private static Dictionary<uint, Node> tagTokenToNodeMap = new Dictionary<uint, Node>();

        internal static uint GetTagToken(string tagString) {
            if (string.IsNullOrWhiteSpace(tagString)) return 0;
            tagString = tagString.Trim();
            tagString = tagString.Replace(" ", "");
            lock (tagStringToToken) {
                if (tagStringToToken.TryGetValue(tagString, out var tok)) return tok;
                uint token = TagTokenCacheManager.GetOrAddToken(tagString);
                tagStringToToken[tagString] = token;
                // �������뵽�㼶��
                InsertTagIntoHierarchy(tagString, token);
                return token;
            }
        }

        internal static bool IsDerivedFrom(uint childToken, uint fatherToken) {
            if (childToken == 0 || fatherToken == 0) return false;
            if (childToken == fatherToken) return true;
            lock (tagHierarchyRoot) {
                if (!tagTokenToNodeMap.TryGetValue(childToken, out var childNode)) return false;
                if (!tagTokenToNodeMap.TryGetValue(fatherToken, out var fatherNode)) return false;
                var cur = childNode.Parent;
                while (cur != null && cur != tagHierarchyRoot) {
                    if (cur.Token == fatherToken) return true;
                    cur = cur.Parent;
                }
                return false;
            }
        }

        internal static IEnumerable<uint> GetFathers(uint childToken) {
            var result = new List<uint>();
            if (childToken == 0) return result;
            lock (tagHierarchyRoot) {
                if (!tagTokenToNodeMap.TryGetValue(childToken, out var node)) return result;
                var cur = node.Parent;
                while (cur != null && cur != tagHierarchyRoot) {
                    if (cur.Token != 0) result.Add(cur.Token);
                    cur = cur.Parent;
                }
            }
            return result;
        }

        private static void InsertTagIntoHierarchy(string tagString, uint token) {
            // Ensure tree modifications are atomic
            lock (tagHierarchyRoot) {
                var parts = tagString.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var cur = tagHierarchyRoot;
                string prefix = string.Empty;
                for (int i = 0; i < parts.Length; i++) {
                    var part = parts[i];
                    prefix = i == 0 ? part : prefix + "." + part;
                    Node child = null;
                    // ����ͨ��ǰ׺�� token �ڸ��ڵ�� token-map �в���
                    if (tagStringToToken.TryGetValue(prefix, out var pToken) && pToken != 0) {
                        child = cur.GetChildByToken(pToken);
                    }
                    // ���ˣ����û�� token���м�ڵ�δ��ע��Ϊ���� tag������ TagName �� Children �����Բ���
                    if (child == null) {
                        child = cur.GetChildByName(part);
                    }
                    if (child == null) {
                        uint nodeToken = 0;
                        if (tagStringToToken.TryGetValue(prefix, out var mapped) && mapped != 0) nodeToken = mapped;
                        child = cur.AddChild(part, nodeToken);
                        // ensure mapping exists for newly created node with non-zero token
                        if (nodeToken != 0) {
                            tagTokenToNodeMap[nodeToken] = child;
                        }
                    } else {
                        // existing child: ensure mapping present if it has token
                        if (child.Token != 0) tagTokenToNodeMap[child.Token] = child;
                    }
                    cur = child;
                }
                // cur Ϊ���սڵ㣬���� token ���ڸ��ڵ�����б��а� token ����
                uint oldToken = cur.Token;
                if (oldToken != token) {
                    if (oldToken != 0 && oldToken != token) {
                        tagTokenToNodeMap.Remove(oldToken);
                    }
                    cur.SetToken(token);
                    if (token != 0) tagTokenToNodeMap[token] = cur;
                } else {
                    if (token != 0) tagTokenToNodeMap[token] = cur;
                }
            }
        }

        private static void LoadTokenFromCache() {
            try {
                TagTokenCacheManager.Load();
                var map = TagTokenCacheManager.GetAllMappings();
                // replace tagStringToToken mapping
                lock (tagStringToToken) {
                    tagStringToToken = new Dictionary<string, uint>(map, StringComparer.Ordinal);
                }
                // rebuild hierarchy incrementally, ensure parents inserted before children
                lock (tagHierarchyRoot) {
                    tagHierarchyRoot = new Node(string.Empty, 0);
                    tagTokenToNodeMap.Clear();
                    // sort keys by number of segments (parents first)
                    var ordered = tagStringToToken.Keys.OrderBy(k => k.Split('.').Length).ToList();
                    foreach (var key in ordered) {
                        InsertTagIntoHierarchy(key, tagStringToToken[key]);
                    }
                }
            }
            catch (Exception e) {
                Debug.LogError($"Failed to load token cache: {e.Message}");
            }
        }

        static TagManager() {
            LoadTokenFromCache();
        }

        // �ڲ��ڵ㶨�壺�� TagName��Token��Parent��Children���� Token �����Լ��� token Ϊ���� childrenMap
        private class Node {
            public string TagName;
            public uint Token;
            public Node Parent;
            public List<Node> Children = new List<Node>();
            // �� token ��Ϊ���Ŀ��ٲ���ӳ�䣨������ Token != 0 ���ӽڵ㣩
            private Dictionary<uint, Node> childrenMapByToken = new Dictionary<uint, Node>();

            public Node(string tagName, uint token) {
                TagName = tagName;
                Token = token;
            }

            // ͨ�� token ���ٲ���
            public Node GetChildByToken(uint token) {
                if (token == 0) return null;
                if (childrenMapByToken.TryGetValue(token, out var n)) return n;
                return null;
            }

            // ͨ�� name ���˲��ң����ԣ�
            public Node GetChildByName(string name) {
                for (int i = 0; i < Children.Count; i++) {
                    if (Children[i].TagName == name) return Children[i];
                }
                return null;
            }

            public Node AddChild(string name, uint token) {
                var n = new Node(name, token) { Parent = this };
                int idx = BinarySearchInsertIndex(Children, n.Token);
                Children.Insert(idx, n);
                if (n.Token != 0) childrenMapByToken[n.Token] = n;
                return n;
            }

            public void SetToken(uint token) {
                if (Token == token) return;
                if (Parent != null && Token != 0) {
                    Parent.childrenMapByToken.Remove(Token);
                }
                Token = token;
                if (Parent != null) {
                    Parent.RepositionChild(this);
                    if (Token != 0) Parent.childrenMapByToken[Token] = this;
                }
            }

            private void RepositionChild(Node child) {
                int oldIndex = Children.IndexOf(child);
                if (oldIndex >= 0) Children.RemoveAt(oldIndex);
                int newIndex = BinarySearchInsertIndex(Children, child.Token);
                Children.Insert(newIndex, child);
            }

            private static int BinarySearchInsertIndex(List<Node> list, uint token) {
                int lo = 0, hi = list.Count; // insert position in [0, Count]
                while (lo < hi) {
                    int mid = (lo + hi) >> 1;
                    uint midToken = list[mid].Token;
                    if (midToken < token) lo = mid + 1; else hi = mid;
                }
                return lo;
            }
        }
    }
}