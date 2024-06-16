using System;
using UnityEngine;

public class Springrana : Creature
{
    protected override void Update()
    {
        base.Update();
        DetectPlayer();
    }
    
    private void DetectPlayer()
    {
        var closestPlayer = PlayerManager.GetClosestPlayer(transform.position);
        if (!closestPlayer)
            return;
        var distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.transform.position);
        switch (currentState)
        {
            case CreatureStates.Roaming:
                // If the player is within alert distance, change to ThreateningState
                if (distanceToPlayer < alertDistance)
                {
                    StateMachine.ChangeState(new ThreateningState(StateMachine, this, closestPlayer,true));
                }
                break;
            case CreatureStates.Threatening:
                var threateningState = (ThreateningState) StateMachine.CurrentState;
                // If the player is out of give up distance and the creature is not moving to the spotter, change to RoamingState
                if (distanceToPlayer > giveUpDistance && !threateningState.isMovingToSpotter)
                {
                    StateMachine.ChangeState(new RoamingState(StateMachine, this));
                }
                // If the player is within chase distance, change to ChaseState
                else if (distanceToPlayer <= chaseDistance)
                {
                    StateMachine.ChangeState(new ChaseState(StateMachine, this, closestPlayer));
                }
                break;
            case CreatureStates.Chasing:
                var distanceToNest = Vector3.Distance(transform.position, Nest.Center);
                // If the creature is too far from the nest, return to the nest
                if (distanceToNest > maxDistanceToNest)
                {
                    StateMachine.ChangeState(new ReturnToNestState(StateMachine, this));
                    break;
                }
                
                var numOfAlliesInRadius = GetNumOfAlliesInRadius();
                // If there are no allies in the radius, regroup
                if (numOfAlliesInRadius == 0)
                {
                    StateMachine.ChangeState(new RegroupState(StateMachine, this));
                    break;
                }
                
                // If the player is within attack distance, change to AttackState
                if (distanceToPlayer <= attackDistance)
                {
                    StateMachine.ChangeState(new AttackState(StateMachine, this, closestPlayer));
                }
                // If the player is out of give up distance, change to RoamingState
                else if (distanceToPlayer > giveUpDistance)
                {
                    StateMachine.ChangeState(new RoamingState(StateMachine, this));
                }
                break;
            case CreatureStates.Attacking:
                // If the player is out of attack distance, change to ChaseState
                if (distanceToPlayer > attackDistance)
                {
                    StateMachine.ChangeState(new ChaseState(StateMachine, this, closestPlayer));
                }
                break;
            case CreatureStates.ReturningToNest:
                //TODO: Implement the logic for the ReturningToNest state when the player is detected
                break;
            case CreatureStates.Regrouping:
                //TODO: Implement the logic for the Regrouping state when the player is detected
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}