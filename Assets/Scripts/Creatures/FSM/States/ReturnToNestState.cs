public class ReturnToNestState : CreatureStateBase
{
    public ReturnToNestState(StateMachine stateMachine, Creature creature) : base(stateMachine, creature)
    {
    }

    public override void Enter()
    {
        creature.currentState = CreatureStates.ReturningToNest;
        creature.MoveToNest(true); 
    }

    public override void Exit()
    {
        creature.Agent.isStopped = true;
    }

    public override void Update()
    {
        // Check if the creature has reached its destination
        if (creature.Agent.remainingDistance <= creature.Agent.stoppingDistance && !creature.Agent.pathPending)
        {
            stateMachine.ChangeState(new RoamingState(stateMachine, creature));
        }
    }
}