using System;

[Flags]
public enum ClientEnum {
    WaitOutofRoom   = 1 << 0,
    CharactorChoose = 1 << 1,
    InGame          = 1 << 2
}