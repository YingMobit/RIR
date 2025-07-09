using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class InputManager : Singleton<InputManager> {
    private Dictionary<PlayerInputType,KeyCode> InputKeyCodeMap = new();

    protected override void Awake() {
        base.Awake();
        Init();
    }

    private void Init() {

    }
}

public enum PlayerInputType {
    PlayerMoveForward,
    PlayerMoveBack,
    PlayerMoveLeft,
    PlayerMoveRight,
    PlayerJump,
    PlayerAttack,
    PlayerInteract,
    PauseGame,
    OpenMap,
    OpenInventory
}