public abstract class CreatureStateBase : IState
{
    protected readonly StateMachine stateMachine;
    protected readonly Creature creature;

    protected CreatureStateBase(StateMachine stateMachine, Creature creature)
    {
        this.stateMachine = stateMachine;
        this.creature = creature;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
}