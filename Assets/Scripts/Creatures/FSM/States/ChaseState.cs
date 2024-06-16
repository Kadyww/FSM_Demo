using UnityEngine;

public class ChaseState : CreatureStateBase
{
    public ChaseState(StateMachine stateMachine, Creature creature, Player target) : base(stateMachine, creature)
    {
        creature.target = target;
    }

    public override void Enter()
    {
        creature.currentState = CreatureStates.Chasing;
        creature.Agent.isStopped = false;
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        // If there is no target, switch to RoamingState
        if (!creature.target)
        {
            stateMachine.ChangeState(new RoamingState(stateMachine, creature));
            return;
        }
        creature.MoveToPosition(creature.target.transform.position, true);
    }
}