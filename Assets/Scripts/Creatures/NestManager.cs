using System.Collections.Generic;
using UnityEngine;

public class NestManager : MonoBehaviour
{
    private static NestManager instance;
    private readonly HashSet<Nest> nests;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    public void AddNest(Nest nest)
    {
        nests.Add(nest);
    }

    public void RemoveNest(Nest nest)
    {
        nests.Remove(nest);
    }
}