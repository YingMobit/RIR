using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vector2SmoothEffector : InputProcessor<Vector2> {
    public float SmoothRate;
    public float Gravity;
    private bool inited = false;
    private Vector2SmoothRecord record;
#if UNITY_EDITOR
    static Vector2SmoothEffector() {
        Init();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() {
        InputSystem.RegisterProcessor<Vector2SmoothEffector>();
    }

    public override Vector2 Process(Vector2 value,InputControl control) {
        if(!inited) {
            record.value = Vector2.zero;
            record.time = Time.timeAsDouble - 1;
            inited = true;
        }

        double cur_Time = Time.timeAsDouble;
        var deltaTime = cur_Time - record.time;

        //���С��Ϊ��ͬ֡����,ʹ�û���
        if(deltaTime < 0.001d) {
            return record.value;
        }

        //����Ϊ0
        if(value.magnitude == 0) {
            //ʹ����������
            float gravityAmount = (float)(Gravity * deltaTime);
            record.value = Vector2.MoveTowards(record.value,Vector2.zero,gravityAmount);
        } else {
            //ƽ������
            float smoothFactor = 1f - Mathf.Exp(-SmoothRate * (float)deltaTime);
            record.value = Vector2.Lerp(record.value,value,smoothFactor);
        }
        record.time = cur_Time;
        return record.value;
    }
}

struct Vector2SmoothRecord {
    public Vector2 value;
    public double time;
}
