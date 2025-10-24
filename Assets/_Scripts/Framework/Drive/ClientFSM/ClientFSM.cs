using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ClientFSM {
    private Dictionary<ClientEnum, ClientState> StatesMap = new() {
        { ClientEnum.WaitOutofRoom,new ClientWaitOutofRoomState()},
        { ClientEnum.CharactorChoose, new ClientCharactorShooseState()},
        { ClientEnum.InGame, new ClientInGameState()}
    };

    private ClientState currentState;
    private ClientFSMContext context;

    public ClientFSM(ClientEnum initialStateEnum,ClientFSMContext context) {
        currentState = StatesMap[initialStateEnum];
        currentState.EnterState(this,context);
        this.context = context;
    }

    public void Update() { 
        currentState.UpdateState(this, context);
    }

    public void SwitchState(ClientEnum newStateEnum) {
        currentState.ExitState(this, context);
        currentState = StatesMap[newStateEnum];
        currentState.EnterState(this, context);
    }
}