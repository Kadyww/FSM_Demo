using System.Linq;
using UnityEngine;

public class RegroupState : CreatureStateBase
{
    private bool isMovingToCenter;

    public RegroupState(StateMachine stateMachine, Creature creature) : base(stateMachine, creature)
    {
    }

    public override void Enter()
    {
        creature.currentState = CreatureStates.Regrouping;
        MoveToIdleAlliesCenter();
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        if (isMovingToCenter)
        {
            // Check if the creature has reached the center of idle allies
            if (creature.Agent.remainingDistance <= 0.1f)
            {
                isMovingToCenter = false;
                Regroup();
            }
        }
    }
    
    
    // Method to move the creature to the center of idle allies
    private void MoveToIdleAlliesCenter()
    {
        var allNestCreatures = creature.Nest.GetCreatures();
        var idleCreatures = allNestCreatures.Where(c => c.currentState != CreatureStates.Attacking || c.currentState != CreatureStates.Chasing).ToList();
        
        // If no idle allies are found, change to RoamingState
        if (idleCreatures.Count == 0)
        {
            stateMachine.ChangeState(new RoamingState(stateMachine, creature));
            return;
        }

        // Calculate the center position of all idle creatures
        Vector3 center = Vector3.zero;
        foreach (var idleCreature in idleCreatures)
        {
            center += idleCreature.transform.position;
        }

        center /= idleCreatures.Count;
        creature.MoveToPosition(center, true);
        isMovingToCenter = true;
    }

    
    // Method to regroup nearby creatures and change their state to ChaseState
    private void Regroup()
    {
        var allNestCreatures = creature.Nest.GetCreatures();
        var nearbyCreatures = allNestCreatures.Where(c => Vector3.Distance(c.transform.position, creature.transform.position) < creature.CallForHelpDistance)
            .ToList();
        nearbyCreatures = nearbyCreatures.Where(c => c.currentState != CreatureStates.Attacking || c.currentState != CreatureStates.Chasing).ToList();
        nearbyCreatures.Add(creature);
        
        // Change the state of all nearby creatures to ChaseState
        foreach (var nearbyCreature in nearbyCreatures)
        {
            nearbyCreature.StateMachine.ChangeState(new ChaseState(nearbyCreature.StateMachine, nearbyCreature, creature.target));
        }
    }
}