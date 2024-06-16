using System.Collections.Generic;
using UnityEngine;

public class Nest : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private Vector3 center;
    [SerializeField]private List<Creature> creatures;

    public Vector3 Center => center;
    public float Radius => radius;

    public void AddCreature(Creature creature)
    {
        if(creatures.Contains(creature)) return;
        creatures.Add(creature);
    }

    public List<Creature> GetCreatures()
    {
        //return a copy of the list
        return new List<Creature>(creatures);
    }

    // Method to get a random position within the nest's radius
    public Vector3 GetRandomPosition()
    {
        // get random point in circle
        float angle = Random.Range(0, 2 * Mathf.PI);
        float distance = Random.Range(0, radius);
        float x = distance * Mathf.Cos(angle);
        float z = distance * Mathf.Sin(angle);
        return center + new Vector3(x, 0, z);
    }

    public void Initialize(int size, Vector3 _center, float cellSize)
    {
        float diagonalLength = cellSize * size;
        radius = Mathf.Sqrt(diagonalLength * diagonalLength / 2f) / 2f;
        center = _center;
        creatures = new List<Creature>();
    }

    public bool IsEmpty()
    {
        return creatures.Count == 0;
    }

    public void RemoveCreature(Creature creature)
    {
        creatures.Remove(creature);
    }
}