using UnityEngine;

public class RoamingState : CreatureStateBase
{
    private float idleTimer;
    private bool isMoving;

    public RoamingState(StateMachine stateMachine, Creature creature) : base(stateMachine, creature)
    {
    }

    public override void Enter()
    {
        creature.currentState = CreatureStates.Roaming;
        idleTimer = creature.GetRandomIdleTime();
        isMoving = false;
        creature.target = null;
        creature.Animator.SetFloat("Speed", 0);
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        if (isMoving)
        {
            if (creature.Agent.remainingDistance <= 0.1f)
            {
                isMoving = false;
                idleTimer = creature.GetRandomIdleTime();
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                creature.MoveToNest();
                isMoving = true;
            }
        }
    }
}