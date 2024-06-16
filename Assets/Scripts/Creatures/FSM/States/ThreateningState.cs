using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThreateningState : CreatureStateBase
{
    private readonly bool isSpotter;
    public bool isMovingToSpotter { get; private set; }

    public ThreateningState(StateMachine stateMachine, Creature creature, Player target, bool isSpotter) : base(stateMachine, creature)
    {
        this.isSpotter = isSpotter;
        creature.target = target;
    }

    public override void Enter()
    {
        creature.currentState =CreatureStates.Threatening;
        if (isSpotter)
        {
            Threaten();
            GroupUp();
        }
        else
        {
            isMovingToSpotter = true;
        }
    }

    public override void Exit()
    {
        creature.Animator.SetBool("Spotted", false);
    }

    public override void Update()
    {
        if (isMovingToSpotter)
        {
            // Check if the creature has reached the spotter
            if (creature.Agent.remainingDistance < 0.1f && creature.Agent.velocity.magnitude < 0.1f &&!creature.Agent.pathPending)
            {
                creature.Agent.isStopped = true;
                isMovingToSpotter = false;
                Threaten();
            }
        }
        else
        {
            creature.LookAtTarget();
        }
    }

    private void GroupUp()
    {
        var allNestCreatures = creature.Nest.GetCreatures();
        var nearbyCreatures = allNestCreatures.Where(c => Vector3.Distance(c.transform.position, creature.transform.position) < creature.CallForHelpDistance)
            .ToList();
        // Only consider creatures in the RoamingState
        nearbyCreatures = nearbyCreatures.Where(c => c.currentState == CreatureStates.Roaming).ToList();
        nearbyCreatures.Remove(creature);
        ArrangeFormation(nearbyCreatures.ToList());
    }

    private void ArrangeFormation(List<Creature> creatures)
    {
        int numCreatures = creatures.Count + 1;
        var direction = (creature.target.transform.position - creature.transform.position).normalized;
        direction.y = 0;
        
        // Generate formation points
        var formationPoints = Formation.GenerateSquareFormation(creature.transform.position,direction, numCreatures, 2);
        var creaturePositions = new Vector3[numCreatures];
        creaturePositions[0] = creature.transform.position;
        for (int i = 1; i < numCreatures; i++)
        {
            creaturePositions[i] = creatures[i - 1].transform.position;
        }
        var targetPositions = Formation.AssignUnitsToFormationPoints(creaturePositions, formationPoints);
        
        // Move creatures to their target positions in the formation
        for (int i = 0; i < numCreatures -1; i++)
        {
            creatures[i].MoveToPosition(targetPositions[i + 1], true);
            creatures[i].StateMachine.ChangeState(new ThreateningState(creatures[i].StateMachine, creatures[i], creature.target, false));
        }
    }

    private void Threaten()
    {
        creature.Animator.SetBool("Spotted", true);
    }
}