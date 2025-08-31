using System;
using System.Collections.Generic;

public enum DamageTag {

}

public static class DamageTagExtension {
    public static List<DamageTag> GetDamageTags(this int curTags) {
        var res = new List<DamageTag>();
        var tags = Enum.GetValues(typeof(DamageTag));
        foreach(var tag in tags) {
            DamageTag damageTag = (DamageTag)tag;
            int tagValue = (int)damageTag;

            // ��鵱ǰ��־�Ƿ�������ö��ֵ
            if((curTags & tagValue) == tagValue && tagValue != 0) {
                res.Add(damageTag);
            }
        }
        return res;
    }

    public static int CombineTags(this int curTages,DamageTag newTag) {
        return curTages |= (int)newTag;
    }
}
