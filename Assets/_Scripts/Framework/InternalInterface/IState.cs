public interface IState {
    void OnInit(IStateMachine stateMachine);

    /// <summary>
    /// Called when the state is entered.
    /// </summary>
    void OnEnter();

    /// <summary>
    /// Called when the state is exited.
    /// </summary>
    void OnExit();

    /// <summary>
    /// Called every frame while the state is active.
    /// </summary>
    void OnUpdate();

    /// <summary>
    /// Called when the state is fixed update.
    /// </summary>
    void OnFixedUpdate();

    IStateMachine stateMachine { get; set; }

    public bool Interruptable { get; set; }
    public int Priority { get; set; }
}