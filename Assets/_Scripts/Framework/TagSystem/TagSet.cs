using System.Collections.Generic;

namespace TagSystem {
    public class TagSet {
        private HashSet<uint> tagTokens = new HashSet<uint>();
        private Dictionary<uint,int> tokenCounts = new Dictionary<uint,int>(); // store reference counts for tokens and ancestors

        public void AddTag(Tag tag) {
            if (tag == null) return;
            uint token = tag.tagToken;
            if (token == 0) return;
            if (tagTokens.Contains(token)) return; // already present, idempotent
            tagTokens.Add(token);
            // increment count for self and all ancestors
            IncrementCount(token);
            foreach (var f in TagManager.GetFathers(token)) {
                IncrementCount(f);
            }
        }

        public bool RemoveTag(Tag tag) {
            if (tag == null) return false;
            uint token = tag.tagToken;
            if (token == 0) return false;
            if (!tagTokens.Contains(token)) return false;
            tagTokens.Remove(token);
            // decrement counts
            DecrementCount(token);
            foreach (var f in TagManager.GetFathers(token)) {
                DecrementCount(f);
            }
            return true;
        }

        public bool HasTagExactly(Tag tag) {
            if (tag == null) return false;
            uint token = tag.tagToken;
            if (token == 0) return false;
            return tagTokens.Contains(token);
        }

        public bool HasTag(Tag tag) {
            if (tag == null) return false;
            uint token = tag.tagToken;
            if (token == 0) return false;
            if (tokenCounts.TryGetValue(token, out var c)) return c > 0;
            return false;
        }

        public void ClearAllTag() {
            tagTokens.Clear();
            tokenCounts.Clear();
        }

        private void IncrementCount(uint token) {
            if (token == 0) return;
            if (tokenCounts.TryGetValue(token, out var c)) tokenCounts[token] = c + 1;
            else tokenCounts[token] = 1;
        }

        private void DecrementCount(uint token) {
            if (token == 0) return;
            if (tokenCounts.TryGetValue(token, out var c)) {
                c -= 1;
                if (c <= 0) tokenCounts.Remove(token);
                else tokenCounts[token] = c;
            }
        }
    }
}