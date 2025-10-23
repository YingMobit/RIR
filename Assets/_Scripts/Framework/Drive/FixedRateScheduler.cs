using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Drive {
    public static class FixedRateScheduler {
        public sealed class Config {
            public double RateHz = 60.0;         // Ŀ��Ƶ��
            public bool UseUnscaledTime = true;  // �Ƿ�ʹ����ʵʱ�䣨���� timeScale Ӱ�죩
            public int MaxStepsPerFrame = 4;     // ��֡��ಹ�ܲ�������ѩ��
        }

        public static event Action<long,double> OnTick; // (tickId, dt)

        static Config _cfg = new();
        static double _dt;
        static double _nextTickTime;
        static long _tickId;
        static bool _running;

        // ���� PlayerLoop �Ļص����ͣ������ǷǷ��;�̬������ʵ������ί�У�
        struct LoopNode { }

        public static void Start(Config cfg = null) {
            if(_running)
                return;
            if(cfg != null)
                _cfg = cfg;

            _dt = 1.0 / Math.Max(1.0,_cfg.RateHz);
            double now = _cfg.UseUnscaledTime ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;
            _nextTickTime = now + _dt;
            _tickId = 0;

            InjectIntoPlayerLoop();
            _running = true;
        }

        public static void Stop() {
            if(!_running)
                return;
            RemoveFromPlayerLoop();
            _running = false;
        }

        public static void SetRate(double hz) {
            _cfg.RateHz = hz;
            _dt = 1.0 / Math.Max(1.0,_cfg.RateHz);
            double now = _cfg.UseUnscaledTime ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;
            _nextTickTime = now + _dt;
        }

        static void Update() {
            double now = _cfg.UseUnscaledTime ? Time.realtimeSinceStartupAsDouble : Time.timeAsDouble;
            int steps = 0;

            // ��һ�δ���ʱ�䷨���ȶ�����Ư�ƣ�һ֡�ڿɲ��ܶಽ
            while(now + 1e-9 >= _nextTickTime && steps < _cfg.MaxStepsPerFrame) {
                OnTick?.Invoke(++_tickId,_dt);
                _nextTickTime += _dt;
                steps++;
            }

            // �����������΢׷�룬���ⳤ����β
            if(steps == _cfg.MaxStepsPerFrame && now - _nextTickTime > _dt * 2)
                _nextTickTime = now + _dt;
        }

        static void InjectIntoPlayerLoop() {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            // �����ǵĽڵ�嵽 EarlyUpdate ֮ǰ��Ҳ�ɲ嵽 Update/PreLateUpdate �Ƚ׶Σ�
            var newSystem = new PlayerLoopSystem {
                type = typeof(LoopNode),
                updateDelegate = Update
            };

            InsertBefore<EarlyUpdate>(ref playerLoop,newSystem);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void RemoveFromPlayerLoop() {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            RemoveType(ref playerLoop,typeof(LoopNode));
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        static void InsertBefore<TPhase>(ref PlayerLoopSystem loop,PlayerLoopSystem node) {
            for(int i = 0; i < loop.subSystemList.Length; i++) {
                ref var sub = ref loop.subSystemList[i];
                if(sub.type == typeof(TPhase)) {
                    var list = new System.Collections.Generic.List<PlayerLoopSystem>(sub.subSystemList ?? Array.Empty<PlayerLoopSystem>());
                    list.Insert(0,node); // �嵽�ý׶���ǰ
                    sub.subSystemList = list.ToArray();
                    return;
                }
                // �ݹ����
                if(sub.subSystemList != null && sub.subSystemList.Length > 0) {
                    InsertBefore<TPhase>(ref sub,node);
                }
            }
        }

        static void RemoveType(ref PlayerLoopSystem loop,Type type) {
            if(loop.subSystemList == null)
                return;
            var list = new System.Collections.Generic.List<PlayerLoopSystem>(loop.subSystemList);
            for(int i = list.Count - 1; i >= 0; i--) {
                var s = list[i];
                if(s.type == type)
                    list.RemoveAt(i);
                else if(s.subSystemList != null && s.subSystemList.Length > 0) {
                    RemoveType(ref s,type);
                    list[i] = s;
                }
            }
            loop.subSystemList = list.ToArray();
        }
    }
}