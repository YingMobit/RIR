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

        // ��������ݹ��캯��
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
            //ȥ��ǰ��ո��Լ��м�ո�
            _tagString = _tagString.Trim();
            _tagString = _tagString.Replace(" ", "");
            // ���⣺��ֹ������������ַ�δ���ϸ�У�飬�������豣֤��ʽ
        }

        private void GetToken() {
            _tagToken = TagManager.GetTagToken(_tagString);
        }
    }
}