using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ClientState {
    public abstract void EnterState(ClientFSM serverFSM,ClientFSMContext context);
    public abstract void UpdateState(ClientFSM serverFSM,ClientFSMContext context);
    public abstract void ExitState(ClientFSM serverFSM,ClientFSMContext context);
}
