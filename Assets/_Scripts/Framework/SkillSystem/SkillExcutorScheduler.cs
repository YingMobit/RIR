using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class SkillExcutorScheduler : MonoBehaviour {
    public int TotalSkillCount => activeExcutor.Count + readyExcutor.Count;
    public int ActiveSkillCount => activeExcutor.Count;
    public int ReadySkillCount => readyExcutor.Count;

    GameObject schdulerPrefab;
    SkillController skillController;
    List<SkillExcutor> activeExcutor = new();
    List<SkillExcutor> readyExcutor = new();
    public void OnLoad(SkillController skillController,SkillExcutor skillExcutor,int count) {
        schdulerPrefab = skillExcutor.gameObject;
        this.skillController = skillController;
        Expand(count);
    }

    public SkillExcutor Schedule() {
        if(readyExcutor.Count != 0) {
            var excutor = readyExcutor[0];
            readyExcutor.RemoveAt(0);
            activeExcutor.Add(excutor);
            return excutor;
        } else {
            return null;
        }
    }

    public void Recycle(SkillExcutor excutor) {
        if(activeExcutor.Remove(excutor)) {
            readyExcutor.Add(excutor);
        } else {
            Debug.LogError($"技能释放器回收失败，当前并未激活该释放器: {excutor.name}");
        }
    }

    public void Expand(int size) {
        for(int i=0;i < size; i++) {
            var excutorGO = PoolCenter.Instance.GetInstance(schdulerPrefab);
            excutorGO.transform.SetParent(transform);
            var excutor = excutorGO.GetComponent<SkillExcutor>();
            excutor.OnLoad(skillController,this);
            readyExcutor.Add(excutor);
        }
    }
}