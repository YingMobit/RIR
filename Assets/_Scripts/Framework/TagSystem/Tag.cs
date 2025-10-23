using System;
using UnityEngine;

namespace TagSystem {
    [Serializable]
    public class Tag : IEquatable<Tag> {
        [SerializeField, InspectorName("Tag")] private string _tagString;
        internal uint tagToken { get {
                if(!_isInitialized) {
                    NormalizeTagString();
                    GetToken();
                    _isInitialized = true;
                }
                return _tagToken;
            }
            private set {
                _tagToken = value;
            }
        }
        private uint _tagToken;
        private bool _isInitialized = false;

        // 新增：便捷构造函数
        public Tag(string tagString) {
            _tagString = tagString;
            _isInitialized = false;
        }

        public bool Matches(Tag tag) {
            if (tag == null) return false;
            return _tagToken == tag._tagToken;
        }

        public bool DerivedFrom(Tag maybeFather) {
            if (maybeFather == null) return false;
            return TagManager.IsDerivedFrom(this.tagToken, maybeFather.tagToken);
        }

        public bool Equals(Tag other) {
            if (other == null) return false;
            return Matches(other);
        }

        private void NormalizeTagString() {
            if (string.IsNullOrEmpty(_tagString)) return;
            //去除前后空格以及中间空格
            _tagString = _tagString.Trim();
            _tagString = _tagString.Replace(" ", "");
            // 额外：禁止连续点或特殊字符未做严格校验，调用者需保证格式
        }

        private void GetToken() {
            _tagToken = TagManager.GetTagToken(_tagString);
        }
    }
}