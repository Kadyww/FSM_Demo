using UnityEngine;

public class AttackState : CreatureStateBase
{
    private float lastAttackTime;

    public AttackState(StateMachine stateMachine, Creature creature, Player target) : base(stateMachine, creature)
    {
        creature.target = target;
    }

    public override void Enter()
    {
        creature.currentState = CreatureStates.Attacking;
        lastAttackTime = Time.time;
        creature.Agent.isStopped = true;
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        if (!creature.target)
        {
            stateMachine.ChangeState(new RoamingState(stateMachine, creature));
            return;
        }

        float distanceToTarget = Vector3.Distance(creature.transform.position, creature.target.transform.position);
        if (distanceToTarget > creature.AttackDistance)
        {
            stateMachine.ChangeState(new ChaseState(stateMachine, creature, creature.target));
        }
        else if (Time.time - lastAttackTime >= creature.AttackRate)
        {
            Attack();
            lastAttackTime = Time.time;
        }

        // Look at the target while attacking
        creature.LookAtTarget();
    }

    private void Attack()
    {
        creature.Animator.SetTrigger("Attack");

        Debug.Log("Attacking the player!");
    }
}