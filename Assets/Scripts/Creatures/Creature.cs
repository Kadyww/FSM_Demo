using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Creature : CreatureReferences
{
    [SerializeField] [HideInInspector] private Nest nest;
    public CreatureStates currentState;
    [Header("Stats")] [SerializeField] protected float roamSpeed = 1;

    [SerializeField] protected float chaseSpeed = 3;
    [SerializeField] protected float rotationSpeed = 10;
    [SerializeField] protected float attackDistance = 1;
    [SerializeField] protected float attackRate = 1;
    [SerializeField] protected float chaseDistance = 5;
    [SerializeField] protected float alertDistance = 10;
    [SerializeField] protected float callForHelpDistance = 10;
    [SerializeField] protected float giveUpDistance = 15;
    [SerializeField] protected float minIdleTime = 1;
    [SerializeField] protected float maxIdleTime = 3;
    [SerializeField] protected float maxDistanceToNest = 40;
    [SerializeField] protected float allyRadius = 8;
    private StateMachine stateMachine;

    protected void Awake()
    {
        transform.parent = null;
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new RoamingState(stateMachine, this));
    }

    protected void FixedUpdate()
    {
        UpdateMovement();
    }

    protected virtual void Update()
    {
        stateMachine.Update();
    }

    public float GetRandomIdleTime()
    {
        return Random.Range(minIdleTime, maxIdleTime);
    }

    public void MoveToNest(bool run = false)
    {
        if (!nest)
        {
            Debug.LogWarning("No nest assigned to creature!");
            return;
        }

        MoveToPosition(nest.GetRandomPosition(), run);
    }

    public void MoveToPosition(Vector3 position, bool isChasing = false)
    {
        agent.speed = isChasing ? chaseSpeed : roamSpeed;
        agent.SetDestination(position);
        agent.isStopped = false;
    }

    public void SetNest(Nest _nest)
    {
        nest = _nest;
    }

    private void UpdateMovement()
    {
        if (agent.isStopped || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance) agent.isStopped = true;

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void LookAtTarget()
    {
        if (!target) return;
        var lookAtPosition = target.transform.position;
        lookAtPosition.y = transform.position.y; // Keep the same height
        transform.LookAt(lookAtPosition);
        Debug.DrawLine(transform.position, lookAtPosition, Color.red);
    }

    public int GetNumOfAlliesInRadius()
    {
        var allNestCreatures = nest.GetCreatures();
        allNestCreatures.Remove(this);
        
        // Get all creatures within the ally radius
        var nearbyCreatures = allNestCreatures.Where(c => Vector3.Distance(c.transform.position, transform.position) < allyRadius)
            .ToList();
        return nearbyCreatures.Count;
    }

    
    // Properties
    public Player target { get; set; }
    public Nest Nest => nest;
    public StateMachine StateMachine => stateMachine;
    public float AttackDistance => attackDistance;
    public float AttackRate => attackRate;
    public float ChaseDistance => chaseDistance;
    public float AlertDistance => alertDistance;
    public float CallForHelpDistance => callForHelpDistance;
    public float GiveUpDistance => giveUpDistance;
}